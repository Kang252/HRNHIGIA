using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.HttpOverrides;
using NHIGIA.Modern.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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
    options.KnownNetworks.Clear();
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
builder.Services.AddSingleton<HrmDataStore>();
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
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Không thể khởi tạo schema HRM");
}

app.Run();
