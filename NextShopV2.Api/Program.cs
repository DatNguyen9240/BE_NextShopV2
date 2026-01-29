using Microsoft.AspNetCore.HttpOverrides;
using NextShopV2.Api.Extensions;
using NextShopV2.Shared.Extensions.Web;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

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
        context.Database.Migrate();
        Console.WriteLine("✅ Database migration completed successfully.");

        // Log DB metadata to help debug missing table issues (database, search_path, existence of advertisements table)
        try
        {
            var conn = context.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT current_database();";
            var db = cmd.ExecuteScalar()?.ToString() ?? "<unknown>";

            cmd.CommandText = "SHOW search_path;";
            var searchPath = cmd.ExecuteScalar()?.ToString() ?? "<unknown>";

            cmd.CommandText = "SELECT to_regclass('public.advertisements')::text;";
            var toReg = cmd.ExecuteScalar()?.ToString() ?? "NULL";

            // List matching tables in any schema
            cmd.CommandText = "SELECT table_schema, table_name FROM information_schema.tables WHERE lower(table_name) LIKE 'advert%';";
            var tables = new List<string>();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var schema = rdr.IsDBNull(0) ? "<null>" : rdr.GetString(0);
                    var name = rdr.IsDBNull(1) ? "<null>" : rdr.GetString(1);
                    tables.Add($"{schema}.{name}");
                }
            }

            // List applied migrations
            cmd.CommandText = "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";";
            var migrations = new List<string>();
            using (var rdr2 = cmd.ExecuteReader())
            {
                while (rdr2.Read())
                {
                    migrations.Add(rdr2.IsDBNull(0) ? "<null>" : rdr2.GetString(0));
                }
            }

            Console.WriteLine($"🗄️ Database: {db}; search_path: {searchPath}; public.advertisements: {toReg}");
            Console.WriteLine($"📋 Matching tables: {(tables.Any() ? string.Join(", ", tables) : "<none>")}");
            Console.WriteLine($"📦 Applied migrations: {(migrations.Any() ? string.Join(", ", migrations) : "<none>")}");

            conn.Close();
        }
        catch (Exception innerEx)
        {
            Console.WriteLine($"⚠️ Failed to query DB metadata: {innerEx.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
    }
}

app.Run();
