using Microsoft.AspNetCore.Authentication.Cookies;
using System.Runtime.InteropServices;
using GraduationPhotosManagement.Captcha;
using Microsoft.EntityFrameworkCore;
using GraduationPhotosManagement.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSingleton<SecurityCodeHelper>();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<PhotoDbContext>(options =>
 options.UseNpgsql("Host=127.0.0.1;Username=everything411;Password=1.2.3,4,;Database=photodb"));

//builder.Services.AddDbContext<PhotoDbContext>(options =>
//  options.UseInMemoryDatabase("PhotoDbContext"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/AccessDenied/";
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.WebHost.UseKestrel(serverOptions =>
    {
        serverOptions.ListenUnixSocket("/tmp/kestrel.sock");
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();


var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    CheckConsentNeeded = context => false,
};

app.UseCookiePolicy(cookiePolicyOptions);

app.MapRazorPages();
app.MapControllers();

app.Run();
