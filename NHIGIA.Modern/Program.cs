using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using NHIGIA.Modern.Infrastructure;
using NHIGIA.Modern.Infrastructure.Workflow;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "NHIGIA.Auth.v2";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var secretKey = builder.Configuration["JWT_SECRET_KEY"] ?? JwtService.DefaultSecretKey;
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = JwtService.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtService.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
var dataProtection = builder.Services.AddDataProtection();
var dataProtectionPath = builder.Configuration["HRM_DATA_PROTECTION_PATH"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<HrmDataStore>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<WorkItemStore>();
builder.Services.AddScoped<HrmUserAccessor>();
builder.Services.AddScoped<AiAnalyticsEngine>();
builder.Services.AddScoped<HrmWorkflowEngine>();
builder.Services.AddScoped<GpsAttendanceService>();
builder.Services.AddScoped<EmployeeSelfServiceStore>();
builder.Services.AddScoped<AiHrAssistantService>();

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
    store.EnsureSchema(Path.Combine(app.Environment.ContentRootPath, "App_Data", "hrm-upgrade-v2.sql"));
}
catch (Exception exception)
{
    app.Logger.LogError(exception, "Không thể khởi tạo schema HRM");
}

app.Run();
