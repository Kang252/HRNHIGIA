using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public sealed class HanetAttendanceSyncService : BackgroundService
{
    private readonly HrmDataStore _store;
    private readonly IHttpClientFactory _clients;
    private readonly ILogger<HanetAttendanceSyncService> _logger;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private DateTime _lastPreviousDaySyncAttempt = DateTime.MinValue;

    public HanetAttendanceSyncService(HrmDataStore store, IHttpClientFactory clients, ILogger<HanetAttendanceSyncService> logger)
    {
        _store = store;
        _clients = clients;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = HrmDataStore.CurrentVietnamTime();
            var shouldRefreshPreviousDay = _lastPreviousDaySyncAttempt.Date != now.Date
                || (now.Hour < 12 && now - _lastPreviousDaySyncAttempt >= TimeSpan.FromHours(1));
            if (shouldRefreshPreviousDay)
            {
                _lastPreviousDaySyncAttempt = now;
                if (!await TrySynchronizeBackgroundDate(now.Date.AddDays(-1), stoppingToken)) break;
            }

            if (!await TrySynchronizeBackgroundDate(now.Date, stoppingToken)) break;
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    private async Task<bool> TrySynchronizeBackgroundDate(DateTime date, CancellationToken stoppingToken)
    {
        try
        {
            await SynchronizeDate(date, stoppingToken);
            return true;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Background HANET attendance synchronization failed for {Date}", date);
            try { _store.UpdateHanetSyncStatus("FAILED", $"Đồng bộ nền HANET ngày {date:dd/MM/yyyy} lỗi: {exception.Message}"); } catch { }
            return true;
        }
    }

    public async Task<HanetAttendanceSyncResult> SynchronizeToday(CancellationToken stoppingToken)
        => await SynchronizeDate(HrmDataStore.CurrentVietnamTime().Date, stoppingToken);

    public async Task<HanetAttendanceSyncResult> SynchronizeDate(DateTime date, CancellationToken stoppingToken)
    {
        date = date.Date;
        var today = HrmDataStore.CurrentVietnamTime().Date;
        if (date > today) throw new InvalidOperationException("Không thể đồng bộ ngày trong tương lai.");
        if (date < today.AddDays(-31)) throw new InvalidOperationException("Chỉ hỗ trợ đồng bộ lại dữ liệu trong 31 ngày gần nhất.");
        await _syncLock.WaitAsync(stoppingToken);
        try
        {
            return await SynchronizeDateCore(date, stoppingToken);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task<HanetAttendanceSyncResult> SynchronizeDateCore(DateTime date, CancellationToken stoppingToken)
    {
        var settings = _store.GetHanetSettings(true);
        if (!settings.IsEnabled) throw new InvalidOperationException("Tích hợp HANET đang tắt.");
        if (string.IsNullOrWhiteSpace(settings.AccessToken)) throw new InvalidOperationException("Chưa có Access Token HANET.");
        if (string.IsNullOrWhiteSpace(settings.PlaceId)) throw new InvalidOperationException("Chưa có Place ID HANET.");

        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var endpoint = settings.ApiBaseUrl.TrimEnd('/') + "/person/getCheckinByPlaceIdInDay";
        var client = _clients.CreateClient("Hanet");
        var events = new List<HanetWebhookEvent>();
        const int pageSize = 500;
        for (var page = 0; page < 20; page++)
        {
            using var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = settings.AccessToken.Trim(), ["placeID"] = settings.PlaceId.Trim(),
                ["date"] = date.ToString("yyyy-MM-dd"), ["type"] = "0", ["exType"] = "1,2",
                ["page"] = page.ToString(CultureInfo.InvariantCulture), ["size"] = pageSize.ToString(CultureInfo.InvariantCulture)
            }), stoppingToken);
            var body = await response.Content.ReadAsStringAsync(stoppingToken);
            var json = JsonNode.Parse(body)?.AsObject() ?? new JsonObject();
            var code = Value(json, "returnCode", "code");
            if (!response.IsSuccessStatusCode || (code != "1" && code != "200" && !string.IsNullOrEmpty(code)))
                throw new InvalidOperationException(Value(json, "returnMessage", "message") ?? $"HTTP {(int)response.StatusCode}");
            var rows = json["data"] as JsonArray ?? new JsonArray();
            foreach (var row in rows.OfType<JsonObject>())
            {
                if (!TryParseTime(Value(row, "checkinTime", "date", "time", "timestamp", "check_time"), out var checkTime)) continue;
                var personId = Value(row, "personID", "personId", "person_id");
                var aliasId = Value(row, "aliasID", "aliasId", "alias_id");
                var deviceId = Value(row, "deviceID", "deviceId", "device_id");
                var eventId = Value(row, "id", "eventId", "eventID");
                var payload = row.ToJsonString();
                events.Add(new HanetWebhookEvent
                {
                    EventKey = eventId ?? Hash($"sync|{settings.PlaceId}|{personId}|{aliasId}|{deviceId}|{checkTime:O}"),
                    PersonId = personId, AliasId = aliasId, PlaceId = settings.PlaceId.Trim(), DeviceId = deviceId,
                    CheckTime = checkTime, EventType = Value(row, "type", "eventType", "event_type", "action_type") ?? "SYNC",
                    PayloadJson = payload
                });
            }
            if (rows.Count < pageSize) break;
        }
        var inserted = _store.SaveAttendanceEvents(events);
        var message = $"Đồng bộ HANET {date:dd/MM/yyyy}: nhận {events.Count} lượt, thêm {inserted} lượt mới trong {elapsed.Elapsed.TotalSeconds:0.0} giây.";
        _store.UpdateHanetSyncStatus("SUCCESS", message);
        _logger.LogInformation("{Message}", message);
        return new HanetAttendanceSyncResult(date, events.Count, inserted, elapsed.Elapsed);
    }

    private static string Value(JsonObject source, params string[] names)
    {
        foreach (var name in names)
            foreach (var property in source)
                if (string.Equals(name, property.Key, StringComparison.OrdinalIgnoreCase)) return property.Value?.ToString();
        return null;
    }

    private static bool TryParseTime(string value, out DateTime result)
    {
        if (long.TryParse(value, out var epoch))
        {
            if (epoch > 9999999999) epoch /= 1000;
            var zone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");
            result = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(epoch), zone).DateTime;
            return true;
        }
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result)
            || DateTime.TryParse(value, new CultureInfo("vi-VN"), DateTimeStyles.AssumeLocal, out result);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

public sealed record HanetAttendanceSyncResult(DateTime Date, int Received, int Inserted, TimeSpan Elapsed);
