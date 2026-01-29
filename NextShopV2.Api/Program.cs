using Microsoft.AspNetCore.HttpOverrides;
using NextShopV2.Api.Extensions;
using NextShopV2.Shared.Extensions.Web;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. Load Environment Variables
var environment = builder.Environment.EnvironmentName;
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", $".env.{environment.ToLower()}");
if (!File.Exists(envPath)) envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath)) DotNetEnv.Env.Load(envPath);

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

// 4. Database Migrations
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<NextShopV2.Infrastructure.Persistence.AppDbContext>();

        // Detect provider and model hints to avoid applying Postgres migrations on SQL Server
        var providerName = context.Database.ProviderName ?? "unknown";
        var modelUsesPostgresTypes = context.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Any(p => {
                var ct = p.GetColumnType();
                if (!string.IsNullOrEmpty(ct))
                {
                    var lower = ct.ToLowerInvariant();
                    if (lower.Contains("timestamp with time zone") || lower.Contains("uuid") || lower.Contains("character varying") || lower.Contains("numeric(") || lower.Contains("double precision"))
                        return true;
                }
                if (p.GetAnnotations().Any(a => a.Name.StartsWith("Npgsql", StringComparison.OrdinalIgnoreCase))) return true;
                return false;
            });

        Console.WriteLine($"ℹ️ Database provider: {providerName}. Detected Postgres-specific model types: {modelUsesPostgresTypes}.");

        if (modelUsesPostgresTypes && providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var msg = "❌ Detected a provider mismatch: EF model appears configured for PostgreSQL (Npgsql types) but application is using SQL Server provider. Please set DATABASE_URL or PG* environment variables to point to a PostgreSQL database in production.";
            Console.WriteLine(msg);
            throw new InvalidOperationException(msg);
        }

        context.Database.Migrate();
        Console.WriteLine("✅ Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
        Console.WriteLine(ex.ToString());
        // Fail fast in production so team notices; non-production continue running to allow local debugging.
        if (app.Environment.IsProduction())
        {
            Console.WriteLine("❌ Application will stop due to database configuration error.");
            Environment.Exit(1);
        }
    }
}

app.Run();
