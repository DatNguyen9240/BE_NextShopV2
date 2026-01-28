using Microsoft.AspNetCore.HttpOverrides;
using NextShopV2.Api.Extensions;
using NextShopV2.Shared.Extensions.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Load Environment Variables
var environment = builder.Environment.EnvironmentName;
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", $".env.{environment.ToLower()}");
if (!File.Exists(envPath)) envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath)) DotNetEnv.Env.Load(envPath);

// Normalize port envs to avoid Kestrel "Overriding HTTP_PORTS" warning when ASPNETCORE_URLS is explicitly configured
var aspnetUrls = builder.Configuration["ASPNETCORE_URLS"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrEmpty(aspnetUrls))
{
    var prevHttpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
    var prevHttpsPorts = Environment.GetEnvironmentVariable("HTTPS_PORTS");
    if (!string.IsNullOrEmpty(prevHttpPorts) || !string.IsNullOrEmpty(prevHttpsPorts))
    {
        Console.WriteLine($"ℹ️ Clearing HTTP_PORTS/HTTPS_PORTS (was: HTTP_PORTS='{prevHttpPorts}', HTTPS_PORTS='{prevHttpsPorts}') because ASPNETCORE_URLS is set to '{aspnetUrls}'.");
        Environment.SetEnvironmentVariable("HTTP_PORTS", "");
        Environment.SetEnvironmentVariable("HTTPS_PORTS", "");
    }
}

// 2. Add Services via Extension Methods
builder.Services.AddDatabaseConfiguration(builder.Configuration)
                .AddRedisAndDataProtection(builder.Configuration)
                .AddJwtAuthentication(builder.Configuration)
                .AddSwaggerConfiguration()
                .AddApplicationServices(builder.Configuration);

// Standard MVC & SignalR
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.Converters.Add(new NextShopV2.Shared.Json.DateTimeUtcConverter());
});
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

// Forwarded Headers for Reverse Proxy (Production)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "";
        var origins = new List<string> { "http://localhost:3000" };
        if (!string.IsNullOrEmpty(frontendUrl)) origins.Add(frontendUrl);
        
        policy.WithOrigins(origins.ToArray()).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// Configure Https redirection port
var httpsPortEnv = builder.Configuration["ASPNETCORE_HTTPS_PORT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
if (int.TryParse(httpsPortEnv, out int httpsPort) && httpsPort != 0)
{
    builder.Services.AddHttpsRedirection(options => { options.HttpsPort = httpsPort; });
}

var app = builder.Build();

app.UseForwardedHeaders();

// 3. Configure Middleware Pipeline
app.UseCors("AllowLocalDev");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextShop API V1");
    c.DocumentTitle = "NextShop API Documentation";
});

app.UseGlobalExceptionHandler();

app.UseWebSockets();

if (app.Environment.IsProduction()) app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SignalR Hubs
app.MapHub<NextShopV2.Api.Hubs.SocketNotificationHub>("/hubs/notifications");
app.MapHub<NextShopV2.Api.Hubs.ShipmentTrackingHub>("/hubs/shipment-tracking");

// 4. Database Migrations (with retry/wait for DB readiness)
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var maxRetries = int.TryParse(Environment.GetEnvironmentVariable("DB_MIGRATE_RETRIES") ?? config["DB_MIGRATE_RETRIES"], out var r) ? r : 10;
    var delayMs = int.TryParse(Environment.GetEnvironmentVariable("DB_MIGRATE_DELAY_MS") ?? config["DB_MIGRATE_DELAY_MS"], out var d) ? d : 3000;

    var context = scope.ServiceProvider.GetRequiredService<NextShopV2.Infrastructure.Persistence.AppDbContext>();
    var succeeded = false;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            if (!context.Database.CanConnect())
            {
                Console.WriteLine($"Waiting for database to be ready (attempt {attempt}/{maxRetries})...");
                System.Threading.Thread.Sleep(delayMs);
                continue;
            }

            context.Database.Migrate();
            Console.WriteLine("✅ Database migration completed successfully.");
            succeeded = true;
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database migration failed (attempt {attempt}/{maxRetries}): {ex.Message}");
            if (attempt == maxRetries) break;
            System.Threading.Thread.Sleep(delayMs);
        }
    }

    if (!succeeded)
    {
        Console.WriteLine("⚠️ Database migration could not be applied after retries. Please check DB connectivity and permissions.");
    }
}

app.Run();
