using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using FerrexWeb.Services;
using FerrexWeb.Pages;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorBootstrap();
builder.Services.AddOptions();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddScoped<CircuitHandler, CircuitSessionHandler>();
builder.Services.AddScoped<CircuitAccessor>();
builder.Services.AddAuthorizationCore();
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<ProductoService>();
builder.Services.AddSingleton<ProductStateService>();
builder.Services.AddSingleton<CategoryStateContainer>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<SubCategoryService>();
builder.Services.AddScoped<SubCategory2Service>();
builder.Services.AddScoped<ProductSyncService>();
builder.Services.AddScoped<QuotationService>();
builder.Services.AddScoped<FreightQuotationService>();
builder.Services.AddTransient<SeekerService>();
builder.Services.AddTransient<PdfService>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();
app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=63072000; includeSubDomains; preload";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' " +
            "https://code.jquery.com " +
            "https://cdnjs.cloudflare.com " +
            "https://unpkg.com " +
            "https://maps.googleapis.com " +
            "https://maps.gstatic.com; " +
        "connect-src 'self' " +
            "https://maps.googleapis.com " +
            "https://maps.gstatic.com; " +
        "style-src 'self' 'unsafe-inline' " +
            "https://fonts.googleapis.com " +
            "https://cdn.jsdelivr.net " +
            "https://cdnjs.cloudflare.com " +
            "https://unpkg.com; " +
        "img-src 'self' data: " +
            "https://maps.gstatic.com; " +
        "font-src 'self' " +
            "https://fonts.gstatic.com " +
            "https://cdn.jsdelivr.net " +
            "https://cdnjs.cloudflare.com; " +
        "frame-ancestors 'none'; " +
        "object-src 'none';";

    await next();
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();