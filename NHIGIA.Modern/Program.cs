using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using NHIGIA.Modern.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
var keycloak = builder.Configuration.GetSection("Authentication:Keycloak");
var keycloakEnabled = keycloak.GetValue<bool>("Enabled");
var keycloakRequireHttpsMetadata = keycloak.GetValue("RequireHttpsMetadata", true);

if (keycloakEnabled && !keycloakRequireHttpsMetadata && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("Authentication:Keycloak:RequireHttpsMetadata chỉ được đặt false trong môi trường Development.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Send anonymous requests through the local login endpoint first. It can
        // verify that Keycloak is reachable and render a controlled 503 response
        // instead of allowing an OIDC metadata failure to become an HTTP 500.
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "NHIGIA.Auth.v3";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Assistant", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
                context.Request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsJsonAsync(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Assistant", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
                context.Request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(new { success = false, message = "Tài khoản không có quyền sử dụng chức năng này." });
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
if (keycloakEnabled)
{
    var authority = keycloak["Authority"];
    var clientId = keycloak["ClientId"];
    if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(clientId))
    {
        throw new InvalidOperationException("Authentication:Keycloak:Authority và ClientId là bắt buộc khi bật Keycloak.");
    }

    builder.Services.AddAuthentication()
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = authority.TrimEnd('/');
            options.ClientId = clientId;
            options.ClientSecret = keycloak["ClientSecret"];
            // Keycloak local runs on HTTP only during Development. Production metadata must always use HTTPS.
            options.RequireHttpsMetadata = keycloakRequireHttpsMetadata;
            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";
            options.ResponseType = "code";
            options.UsePkce = true;
            // The OIDC handler reads the saved ID token when it builds Keycloak's
            // RP-initiated logout request (id_token_hint).
            options.SaveTokens = true;
            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = keycloak["UsernameClaim"] ?? "preferred_username"
            };
            options.Events.OnTokenValidated = context =>
            {
                var usernameClaim = keycloak["UsernameClaim"] ?? "preferred_username";
                var username = context.Principal?.FindFirst(usernameClaim)?.Value;
                if (string.IsNullOrWhiteSpace(username))
                {
                    context.Fail("Keycloak không trả về tên tài khoản.");
                    return Task.CompletedTask;
                }

                var store = context.HttpContext.RequestServices.GetRequiredService<HrmDataStore>();
                NHIGIA.Modern.Models.HrmUserAccountModel user;
                try
                {
                    user = store.FindUser(username);
                }
                catch (Exception exception)
                {
                    context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Keycloak")
                        .LogError(exception, "Không thể kiểm tra tài khoản HRM sau khi Keycloak xác thực {Username}", username);
                    context.Fail("Không thể kiểm tra quyền truy cập HRM.");
                    return Task.CompletedTask;
                }

                if (user == null || !user.IsActive)
                {
                    context.Fail("Tài khoản chưa được cấp quyền vào HRM.");
                    return Task.CompletedTask;
                }

                var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity == null)
                {
                    context.Fail("Không thể tạo phiên đăng nhập HRM.");
                    return Task.CompletedTask;
                }

                foreach (var claim in identity.Claims.Where(claim =>
                             claim.Type is System.Security.Claims.ClaimTypes.NameIdentifier or
                                 System.Security.Claims.ClaimTypes.Name or
                                 System.Security.Claims.ClaimTypes.Role or
                                 "display_name" or "department_name" or "department_id" or "supervisor_id").ToList())
                {
                    identity.RemoveClaim(claim);
                }

                identity.AddClaim(new global::System.Security.Claims.Claim(global::System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()));
                identity.AddClaim(new global::System.Security.Claims.Claim(global::System.Security.Claims.ClaimTypes.Name, user.Username));
                identity.AddClaim(new global::System.Security.Claims.Claim(global::System.Security.Claims.ClaimTypes.Role, user.RoleCode));
                identity.AddClaim(new global::System.Security.Claims.Claim("display_name", user.DisplayName ?? user.Username));
                identity.AddClaim(new global::System.Security.Claims.Claim("department_name", user.DepartmentName ?? string.Empty));
                if (user.DepartmentId.HasValue) identity.AddClaim(new global::System.Security.Claims.Claim("department_id", user.DepartmentId.Value.ToString()));
                if (user.SupervisorUserId.HasValue) identity.AddClaim(new global::System.Security.Claims.Claim("supervisor_id", user.SupervisorUserId.Value.ToString()));

                store.MarkLogin(user.Id, context.HttpContext.Connection.RemoteIpAddress?.ToString());
                return Task.CompletedTask;
            };
            options.Events.OnRemoteFailure = context =>
            {
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Keycloak")
                    .LogWarning(context.Failure, "Đăng nhập Keycloak không thành công");
                context.Response.Redirect("/Account/Login?error=keycloak");
                context.HandleResponse();
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToIdentityProviderForSignOut = context =>
            {
                // Sessions created before SaveTokens was enabled do not contain an
                // ID token. Keycloak accepts client_id as the fallback identifier
                // when post_logout_redirect_uri is present.
                if (string.IsNullOrWhiteSpace(context.ProtocolMessage.IdTokenHint))
                {
                    context.ProtocolMessage.ClientId = clientId;
                }

                return Task.CompletedTask;
            };
        });
}
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "NHIGIA.Antiforgery.v2";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddSingleton<SqlDataProtectionKeyRepository>();
var dataProtection = builder.Services.AddDataProtection().SetApplicationName("NHIGIA.Modern.v1");
var dataProtectionPath = builder.Configuration["HRM_DATA_PROTECTION_PATH"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}
else
{
    builder.Services.AddOptions<KeyManagementOptions>()
        .Configure<SqlDataProtectionKeyRepository>((options, repository) => options.XmlRepository = repository);
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<KeycloakAvailabilityProbe>();
builder.Services.AddSingleton<HrmDataStore>();
builder.Services.AddSingleton<IOpenCvFaceMatcher, OpenCvFaceMatcher>();
builder.Services.AddSingleton<HanetAttendanceSyncService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<HanetAttendanceSyncService>());

builder.Services.AddScoped<WorkItemStore>();
builder.Services.AddScoped<RecruitmentIntegrationStore>();
builder.Services.AddScoped<HrmUserAccessor>();
builder.Services.AddScoped<HrmAssistantService>();
builder.Services.AddHttpClient<GeminiAssistantClient>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient("Hanet", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("NHIGIA-HRM/1.0");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    var store = app.Services.GetRequiredService<HrmDataStore>();
    store.EnsureSchema(Path.Combine(app.Environment.ContentRootPath, "App_Data", "hrm-mvp.sql"));
    store.EnsureSchema(Path.Combine(app.Environment.ContentRootPath, "App_Data", "remote-attendance.sql"));
    store.EnsureSchema(Path.Combine(app.Environment.ContentRootPath, "App_Data", "face-enrollment.sql"));
    store.EnsureSchema(Path.Combine(app.Environment.ContentRootPath, "App_Data", "opencv-face-trial.sql"));
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Không thể khởi tạo schema HRM");
}

app.Run();
