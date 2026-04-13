using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using FerrexWeb.Services;
using FerrexWeb.Pages;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Logging detallado para producción
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);


builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    options.DetailedErrors = !builder.Environment.IsProduction();
});
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
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        });

    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    if (!builder.Environment.IsProduction())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});
builder.Services.AddSingleton<ProductoService>();
builder.Services.AddSingleton<ProductStateService>();
builder.Services.AddSingleton<CategoryStateContainer>();
builder.Services.AddSingleton<GoogleCalendarService>();
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
builder.Services.AddScoped<FreightConfirmationService>();
builder.Services.AddScoped<FreightQuotationService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddTransient<SeekerService>();
builder.Services.AddTransient<PdfService>();
builder.Services.AddTransient<IEmailSender, BrevoEmailSender>();

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
    context.Response.Headers["Permissions-Policy"] = "geolocation=(self), microphone=(), camera=()";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=63072000; includeSubDomains; preload";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-eval' 'unsafe-inline' " +
            "https://maps.googleapis.com " +
            "https://maps.gstatic.com " +
            "https://code.jquery.com " +
            "https://cdnjs.cloudflare.com " +
            "https://unpkg.com " +
            "https://*.tawk.to " +
            "https://embed.tawk.to; " +
        "connect-src 'self' " +
            "https://maps.googleapis.com " +
            "https://maps.gstatic.com " +
            "https://*.tawk.to " +
            "wss://*.tawk.to " +
            "wss: ws:; " +
        "style-src 'self' 'unsafe-inline' " +
            "https://fonts.googleapis.com " +
            "https://cdn.jsdelivr.net " +
            "https://cdnjs.cloudflare.com " +
            "https://unpkg.com " +
            "https://embed.tawk.to " +
            "https://*.tawk.to; " +
        "img-src 'self' data: https://maps.gstatic.com https://maps.googleapis.com https://*.gstatic.com https://*.tawk.to https:; " +
        "font-src 'self' data: " +
            "https://fonts.gstatic.com " +
            "https://cdn.jsdelivr.net " +
            "https://cdnjs.cloudflare.com " +
            "https://embed.tawk.to " +
            "https://*.tawk.to; " +
        "frame-src https://*.tawk.to; " +
        "child-src https://*.tawk.to; " +
        "frame-ancestors 'none'; " +
        "object-src 'none';";

    await next();
});

// Endpoint de diagnóstico - visita /api/health para ver el estado de la BD
app.MapGet("/api/health", async (IDbContextFactory<ApplicationDbContext> factory, ILogger<Program> logger) =>
{
    var result = new Dictionary<string, string>();
    result["timestamp"] = DateTime.UtcNow.ToString("o");
    result["environment"] = app.Environment.EnvironmentName;
    result["dotnet_version"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

    try
    {
        using var db = await factory.CreateDbContextAsync();
        var canConnect = await db.Database.CanConnectAsync();
        result["database_connection"] = canConnect ? "OK" : "FAILED";

        if (canConnect)
        {
            var userCount = await db.Users.CountAsync();
            result["users_count"] = userCount.ToString();
            var catCount = await db.Categories.CountAsync();
            result["categories_count"] = catCount.ToString();
            var rolesCount = await db.Roles.CountAsync();
            result["roles_count"] = rolesCount.ToString();
        }
    }
    catch (Exception ex)
    {
        result["database_connection"] = "ERROR";
        logger.LogError(ex, "Health check - DB connection failed");
    }

    return Results.Json(result);
});

// Test de conexión a BD al iniciar
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    using var scope = app.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var db = await factory.CreateDbContextAsync();
    var canConnect = await db.Database.CanConnectAsync();
    startupLogger.LogInformation("=== DB Connection Test: {Status} ===", canConnect ? "SUCCESS" : "FAILED");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "=== DB Connection Test FAILED at startup ===");
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();