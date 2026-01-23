using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using Npgsql;
using StackExchange.Redis;
using NextShopV2.Application.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NextShopV2.Shared.Extensions.Web;
using PayOS;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using DotNetEnv;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Load .env file based on environment
var environment = builder.Environment.EnvironmentName;
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", $".env.{environment.ToLower()}");

// Fallback to .env if environment-specific file doesn't exist
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
}

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

// DataProtection is configured later after Redis IConnectionMultiplexer is registered

// Add DbContext with dynamic provider selection
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Support Railway/Template DATABASE_URL (e.g. postgres://user:pass@host:port/db)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                      ?? builder.Configuration["DATABASE_URL"]
                      ?? Environment.GetEnvironmentVariable("Postgres.DATABASE_URL")
                      ?? builder.Configuration["Postgres:DATABASE_URL"];

    bool isPostgresConfigured = false;

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var npgBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
                Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                SslMode = SslMode.Prefer
            };
            options.UseNpgsql(npgBuilder.ConnectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            isPostgresConfigured = true;
        }
        catch
        {
            // If parsing fails, fall back to environment variables below
        }
    }

    if (!isPostgresConfigured)
    {
        // Prioritize PostgreSQL if environment variables exist
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        if (string.IsNullOrWhiteSpace(pgHost)) pgHost = builder.Configuration["PGHOST"];
        
        var pgDb = Environment.GetEnvironmentVariable("PGDATABASE");
        if (string.IsNullOrWhiteSpace(pgDb)) pgDb = builder.Configuration["PGDATABASE"];
        
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        if (string.IsNullOrWhiteSpace(pgUser)) pgUser = builder.Configuration["PGUSER"];
        
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        if (string.IsNullOrWhiteSpace(pgPassword)) pgPassword = builder.Configuration["PGPASSWORD"];
        
        var pgPort = Environment.GetEnvironmentVariable("PGPORT");
        if (string.IsNullOrWhiteSpace(pgPort)) pgPort = builder.Configuration["PGPORT"];
        if (string.IsNullOrWhiteSpace(pgPort)) pgPort = "5432";

        bool hasPostgres = !string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDb) && !string.IsNullOrWhiteSpace(pgUser) && !string.IsNullOrWhiteSpace(pgPassword);

        if (hasPostgres)
        {
            var pgConn = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword};SSL Mode=Prefer;Trust Server Certificate=true";
            options.UseNpgsql(pgConn, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
        else
        {
            // Fallback: SQL Server
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "localhost";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "NextShopDB";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
            
            // If DB_HOST is localhost and no connection string, assume local SQL Server default
            var sqlConn = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(sqlConn))
            {
                 sqlConn = $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
            }
            
            options.UseSqlServer(sqlConn, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        }
    }

    // Configure global EF Core warnings handling:
    // - Use split queries to avoid expensive single-query includes of multiple collections.
    // - Suppress FirstWithoutOrderBy warning when queries intentionally rely on single-record lookups.
    // - Suppress PendingModelChangesWarning to allow app startup even if snapshot is slightly out of sync
    options.ConfigureWarnings(w =>
    {
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.FirstWithoutOrderByAndFilterWarning);
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning);
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
    });
});

// --- Redis Configuration ---
var redisConfig = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
                  ?? builder.Configuration.GetConnectionString("Redis")
                  ?? "localhost:6379";

// Standardize connection string: remove problematic 'AbortOnConnectFail' or 'abortConnect' 
redisConfig = System.Text.RegularExpressions.Regex.Replace(redisConfig, @"(?i)\b(AbortOnConnectFail|abortConnect)=[^,;]+[,;]?", "");
redisConfig = redisConfig.Trim().TrimEnd(',', ';');

IConnectionMultiplexer redisMultiplexer;
try
{
    var options = StackExchange.Redis.ConfigurationOptions.Parse(redisConfig);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 10000; // Increase timeout for production stability
    redisMultiplexer = ConnectionMultiplexer.Connect(options);
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Strict Redis parsing failed ({ex.Message}). Retrying with lenient options.");
    redisMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
}

// Register IConnectionMultiplexer as a Singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);

// Configure DataProtection to use Redis for key persistence
var dpBuilder = builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redisMultiplexer, "DataProtection-Keys");

// Encrypt keys at rest using a self-signed certificate to remove the 'No XML encryptor' warning
// In a real production app, you'd use a certificate from a Key Vault or a persistent file.
try
{
    var certPath = Path.Combine(Directory.GetCurrentDirectory(), "dp_key.pfx");
    var certPassword = Environment.GetEnvironmentVariable("DP_CERT_PASSWORD") ?? "NextShopDefaultPassword123!"; 
    
    System.Security.Cryptography.X509Certificates.X509Certificate2? cert = null;
    
    if (File.Exists(certPath))
    {
        cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath, certPassword);
    }
    else
    {
        // Generate a temporary self-signed certificate for local/simple cloud persistence
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var request = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            "cn=NextShopDataProtection", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
        
        // Export to file so it's reused if the container is not destroyed (or for local testing)
        // Note: On Railway ephemeral disks, this file disappears on redeploy, but the warning will stay gone 
        // as long as the app is running.
        File.WriteAllBytes(certPath, cert.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx, certPassword));
    }
    
    if (cert != null)
    {
        dpBuilder.ProtectKeysWithCertificate(cert);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Note: DataProtection encryption not configured (using unencrypted keys): {ex.Message}");
}

// Register IDistributedCache using the common IConnectionMultiplexer
builder.Services.AddSingleton<IDistributedCache>(provider =>
{
    var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
    return new Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache(new RedisCacheOptions
    {
        ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer)
    });
});

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // Ensure DateTime objects are serialized as UTC ISO strings (append Z) to avoid client timezone issues
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    opts.JsonSerializerOptions.Converters.Add(new NextShopV2.Shared.Json.DateTimeUtcConverter());
});

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();

// CORS: allow local Next.js dev origin and production frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "";
        var origins = new List<string> { "http://localhost:3000" };
        
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            origins.Add(frontendUrl);
        }
        
        policy.WithOrigins(origins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NextShop API", 
        Version = "v1",
        Description = "NextShop E-commerce API"
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT token (without 'Bearer ' prefix)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "";
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // Support passing access_token in query string for SignalR WebSocket requests
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/shipment-tracking")))
                {
                    context.Token = accessToken;
                }
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                try
                {
                    var db = context.HttpContext.RequestServices.GetService<StackExchange.Redis.IDatabase>();
                    var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                    if (db != null && token != null)
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            var key = $"blacklist:{authHeader.Substring(7).Trim()}";
                            var exists = await db.StringGetAsync(key);
                            if (!exists.IsNullOrEmpty)
                            {
                                context.Fail("Token is blacklisted");
                            }
                        }
                    }
                }
                catch
                {
                    // ignore Redis issues and allow token
                }
            }
        };
    });

// Register SignalR
builder.Services.AddSignalR();

builder.Services.AddAuthorization();

// Configure Https redirection port so middleware can determine redirect target.
var httpsPortEnv = builder.Configuration["ASPNETCORE_HTTPS_PORT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
int httpsPort = 0;
if (!string.IsNullOrWhiteSpace(httpsPortEnv))
{
    int.TryParse(httpsPortEnv, out httpsPort);
}
if (httpsPort == 0)
{
    httpsPort = 7264; // fallback dev SSL port
}
builder.Services.AddHttpsRedirection(options => { options.HttpsPort = httpsPort; });

// Memory cache for WebAuthn challenges
builder.Services.AddMemoryCache();

// Register shared cache service
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.ICacheService>(provider =>
{
    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
    return new NextShopV2.Shared.Services.RedisCacheService(redis, TimeSpan.FromMinutes(5));
});


// Register DI for OrderResolutionService (shared utility)
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.IOrderResolutionService, NextShopV2.Shared.Services.OrderResolutionService>();

// Register DI for AuthService and UserRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAuthService, NextShopV2.Application.Services.AuthService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IUserRepository, NextShopV2.Infrastructure.Repositories.UserRepository>();

// Register DI for BannerService and BannerRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAdvertisementService, NextShopV2.Application.Services.AdvertisementService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAdvertisementRepository, NextShopV2.Infrastructure.Repositories.AdvertisementRepository>();

// Register DI for AnnouncementService and AnnouncementRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAnnouncementService, NextShopV2.Application.Services.AnnouncementService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAnnouncementRepository, NextShopV2.Infrastructure.Repositories.AnnouncementRepository>();

// Register DI for FooterInfoService and FooterInfoRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IFooterInfoService, NextShopV2.Application.UseCases.FooterInfoService>();
builder.Services.AddScoped<NextShopV2.Domain.Repositories.IFooterInfoRepository, NextShopV2.Infrastructure.Repositories.FooterInfoRepository>();


// Register DI for ProductService and ProductRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductService, NextShopV2.Application.Services.ProductService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductRepository, NextShopV2.Infrastructure.Repositories.ProductRepository>();
// Register DI for UploadService and UploadRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IUploadService, NextShopV2.Application.Services.UploadService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IUploadRepository, NextShopV2.Infrastructure.Repositories.UploadRepository>();

// Register DI for LoggingService
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.ILoggingService, NextShopV2.Shared.Services.LoggingService>();

// Register DI for CategoryService and CategoryRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.services.ICategoryService, NextShopV2.Application.Services.CategoryService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.repositories.ICategoryRepository, NextShopV2.Infrastructure.Repositories.CategoryRepository>();

// Register DI for ProductCategoryService and ProductCategoryRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.services.IProductCategoryService, NextShopV2.Application.Services.ProductCategoryService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.repositories.IProductCategoryRepository, NextShopV2.Infrastructure.Repositories.ProductCategoryRepository>();

// Register DI for ProductVariantService and ProductVariantRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductVariantService, NextShopV2.Application.Services.ProductVariantService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductVariantRepository, NextShopV2.Infrastructure.Repositories.ProductVariantRepository>();

// Register DI for OrderService and OrderRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IOrderService, NextShopV2.Application.Services.OrderService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IOrderRepository, NextShopV2.Infrastructure.Repositories.OrderRepository>();

// Register DI for ShipmentService and ShipmentRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IShipmentService, NextShopV2.Application.Services.ShipmentService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IShipmentRepository, NextShopV2.Infrastructure.Repositories.ShipmentRepository>();

// Register DI for TrackingService and TrackingEventRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.ITrackingService, NextShopV2.Application.Services.TrackingService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.ITrackingEventRepository, NextShopV2.Infrastructure.Repositories.TrackingEventRepository>();

builder.Services.AddHttpClient();

// Register DI for InventoryService and InventoryTransactionRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IInventoryService, NextShopV2.Application.Services.InventoryService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IInventoryTransactionRepository, NextShopV2.Infrastructure.Repositories.InventoryTransactionRepository>();

// Register DI for CouponService and CouponRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.ICouponService, NextShopV2.Application.Services.CouponService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.ICouponRepository, NextShopV2.Infrastructure.Repositories.CouponRepository>();

// Register DI for ProductLikeService and ProductLikeRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductLikeService, NextShopV2.Application.Services.ProductLikeService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductLikeRepository, NextShopV2.Infrastructure.Repositories.ProductLikeRepository>();

// Register DI for ReviewService and ReviewRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IReviewService, NextShopV2.Application.Services.ReviewService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IReviewRepository, NextShopV2.Infrastructure.Repositories.ReviewRepository>();

// Register Firebase Notification Service
builder.Services.AddSingleton<NextShopV2.Application.Interfaces.Services.IFirebaseNotificationService, NextShopV2.Infrastructure.Services.FirebaseNotificationService>();

// Register DI for Email sender service
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IEmailService, NextShopV2.Infrastructure.Services.EmailService>();

// Register DI for Firebase PushTokenRepository and NotificationHistoryRepository
builder.Services.AddScoped<NextShopV2.Domain.Repositories.IFirebasePushTokenRepository, NextShopV2.Infrastructure.Repositories.FirebasePushTokenRepository>();
builder.Services.AddScoped<NextShopV2.Domain.Repositories.INotificationHistoryRepository, NextShopV2.Infrastructure.Repositories.NotificationHistoryRepository>();

            // Passkey repository
            builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IPasskeyRepository, NextShopV2.Infrastructure.Repositories.PasskeyRepository>();
            // WebAuthn service
            builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IWebAuthnService, NextShopV2.Infrastructure.Services.WebAuthnService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IPushNotificationService, NextShopV2.Application.UseCases.FirebaseNotificationService>();

// Register DI for SocketNotificationService
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.ISocketNotificationService, NextShopV2.Api.Services.SocketNotificationService>();

// Register DI for PaymentService
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IPaymentService, NextShopV2.Infrastructure.Services.PaymentService>();

// Register DI for Redis Cart Service (from Shared)
builder.Services.AddScoped<IDatabase>(serviceProvider =>
{
    var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
    return redis.GetDatabase();
});

builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductVariantCartService, NextShopV2.Application.UseCases.ProductVariantCartService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IRedisCartService, NextShopV2.Infrastructure.Services.RedisCartService>();

// Configure PayOS client for payment requests
builder.Services.AddSingleton<PayOSClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new PayOSClient(new PayOSOptions
    {
        ClientId = config["PayOS:ClientId"] ?? Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"),
        ApiKey = config["PayOS:ApiKey"] ?? Environment.GetEnvironmentVariable("PAYOS_API_KEY"),
        ChecksumKey = config["PayOS:ChecksumKey"] ?? Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"),
        LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
    });
});

var app = builder.Build();

// Enable CORS as early as possible so preflight requests are handled
app.UseCors("AllowLocalDev");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextShop API V1");
    c.DocumentTitle = "NextShop API Documentation";
});

// Global error handling middleware
app.UseGlobalExceptionHandler();

// Only use HTTPS redirection when not in Development to avoid redirecting
// local HTTP dev requests to an HTTPS port that may not be listening.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NextShopV2.Api.Hubs.SocketNotificationHub>("/hubs/notifications");
app.MapHub<NextShopV2.Api.Hubs.ShipmentTrackingHub>("/hubs/shipment-tracking");

// SignalR hub for notifications

// Automatically apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<NextShopV2.Infrastructure.Persistence.AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
    }
}

app.Run();
