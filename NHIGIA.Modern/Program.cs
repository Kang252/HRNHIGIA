using Microsoft.AspNetCore.Authentication.Cookies;
using NHIGIA.Modern.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "NHIGIA.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<HrmDataStore>();
builder.Services.AddScoped<HrmUserAccessor>();

var app = builder.Build();

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
