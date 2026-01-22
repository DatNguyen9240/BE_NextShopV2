using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
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

// Add DbContext with dynamic provider selection
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsProduction())
    {
        // PostgreSQL for Production (Railway)
        options.UseNpgsql(connectionString);
    }
    else
    {
        // SQL Server for Development
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"
    )
);

// Add distributed cache using Redis
builder.Services.AddSingleton<IDistributedCache>(provider =>
{
    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
    var options = new RedisCacheOptions
    {
        Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"
    };
    return new RedisCache(options);
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

// CORS: allow local Next.js dev origin and ngrok for testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://3b1cfe4e17af.ngrok-free.app")
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "")),
            ClockSkew = TimeSpan.FromMinutes(5)
        };

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
            OnTokenValidated = context =>
            {
                try
                {
                    var db = context.HttpContext.RequestServices.GetService<StackExchange.Redis.IDatabase>();
                    var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                    if (db != null && token != null)
                    {
                        var key = $"blacklist:{context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim()}";
                        var exists = db.StringGet(key);
                        if (!exists.IsNullOrEmpty)
                        {
                            // token is blacklisted
                            context.Fail("Token is blacklisted");
                        }
                    }
                }
                catch
                {
                    // ignore Redis issues and allow token (or you can fail)
                }
                return System.Threading.Tasks.Task.CompletedTask;
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
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NextShopV2.Api.Hubs.SocketNotificationHub>("/hubs/notifications");
app.MapHub<NextShopV2.Api.Hubs.ShipmentTrackingHub>("/hubs/shipment-tracking");

// SignalR hub for notifications
app.Run();
