using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace NHIGIA.Modern.Infrastructure;

public sealed class KeycloakAvailabilityProbe
{
    private static readonly TimeSpan AvailableCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan UnavailableCacheDuration = TimeSpan.FromSeconds(5);

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KeycloakAvailabilityProbe> _logger;
    private readonly SemaphoreSlim _probeLock = new(1, 1);

    public KeycloakAvailabilityProbe(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<KeycloakAvailabilityProbe> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        var keycloak = _configuration.GetSection("Authentication:Keycloak");
        if (!keycloak.GetValue<bool>("Enabled")) return true;

        var authority = keycloak["Authority"]?.TrimEnd('/');
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri))
        {
            _logger.LogError("Keycloak Authority không hợp lệ.");
            return false;
        }

        var cacheKey = $"keycloak-availability:{authorityUri.AbsoluteUri}";
        if (_cache.TryGetValue(cacheKey, out bool cachedResult)) return cachedResult;

        await _probeLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out cachedResult)) return cachedResult;

            var timeoutSeconds = Math.Clamp(
                keycloak.GetValue("AvailabilityTimeoutSeconds", 8),
                2,
                30);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var metadataUri = new Uri($"{authority}/.well-known/openid-configuration");
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(
                    metadataUri,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Dịch vụ Keycloak trả về HTTP {StatusCode} tại discovery endpoint.",
                        (int)response.StatusCode);
                    Cache(cacheKey, false);
                    return false;
                }

                await using var content = await response.Content.ReadAsStreamAsync(timeout.Token);
                using var document = await JsonDocument.ParseAsync(content, cancellationToken: timeout.Token);
                var root = document.RootElement;
                var valid = root.TryGetProperty("issuer", out var issuer) &&
                            !string.IsNullOrWhiteSpace(issuer.GetString()) &&
                            root.TryGetProperty("authorization_endpoint", out var authorizationEndpoint) &&
                            !string.IsNullOrWhiteSpace(authorizationEndpoint.GetString()) &&
                            root.TryGetProperty("token_endpoint", out var tokenEndpoint) &&
                            !string.IsNullOrWhiteSpace(tokenEndpoint.GetString());

                if (!valid)
                {
                    _logger.LogWarning("Discovery document của Keycloak thiếu endpoint OIDC bắt buộc.");
                }

                Cache(cacheKey, valid);
                return valid;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Dịch vụ Keycloak không phản hồi discovery endpoint trong {TimeoutSeconds} giây.",
                    timeoutSeconds);
                Cache(cacheKey, false);
                return false;
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(exception, "Không thể kết nối discovery endpoint của Keycloak.");
                Cache(cacheKey, false);
                return false;
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Discovery endpoint của Keycloak không trả về JSON hợp lệ.");
                Cache(cacheKey, false);
                return false;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Không thể kiểm tra trạng thái dịch vụ Keycloak.");
                Cache(cacheKey, false);
                return false;
            }
        }
        finally
        {
            _probeLock.Release();
        }
    }

    private void Cache(string key, bool available) =>
        _cache.Set(
            key,
            available,
            available ? AvailableCacheDuration : UnavailableCacheDuration);
}
