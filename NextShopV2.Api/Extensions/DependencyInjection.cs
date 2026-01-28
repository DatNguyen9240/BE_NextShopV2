using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using Npgsql;
using StackExchange.Redis;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PayOS;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NextShopV2.Api.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                                  ?? configuration["DATABASE_URL"]
                                  ?? Environment.GetEnvironmentVariable("Postgres.DATABASE_URL")
                                  ?? configuration["Postgres:DATABASE_URL"];

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
                    catch { }
                }

                if (!isPostgresConfigured)
                {
                    var pgHost = Environment.GetEnvironmentVariable("PGHOST") ?? configuration["PGHOST"];
                    var pgDb = Environment.GetEnvironmentVariable("PGDATABASE") ?? configuration["PGDATABASE"];
                    var pgUser = Environment.GetEnvironmentVariable("PGUSER") ?? configuration["PGUSER"];
                    var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? configuration["PGPASSWORD"];
                    var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? configuration["PGPORT"] ?? "5432";

                    if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDb) && !string.IsNullOrWhiteSpace(pgUser) && !string.IsNullOrWhiteSpace(pgPassword))
                    {
                        var pgConn = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword};SSL Mode=Prefer;Trust Server Certificate=true";
                        options.UseNpgsql(pgConn, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    }
                    else
                    {
                        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
                        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
                        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
                        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
                        
                        string sqlConn;
                        if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName))
                        {
                            sqlConn = $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
                        }
                        else
                        {
                            sqlConn = configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=NextShopDB;Trusted_Connection=True;TrustServerCertificate=True;";
                        }
                        
                        options.UseSqlServer(sqlConn, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    }
                }

                options.ConfigureWarnings(w =>
                {
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.FirstWithoutOrderByAndFilterWarning);
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.MultipleCollectionIncludeWarning);
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                });
            });

            return services;
        }

        public static IServiceCollection AddRedisAndDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConfig = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
                              ?? configuration.GetConnectionString("Redis")
                              ?? "localhost:6379";

            redisConfig = System.Text.RegularExpressions.Regex.Replace(redisConfig, @"(?i)\b(AbortOnConnectFail|abortConnect)=[^,;]+[,;]?", "").Trim().TrimEnd(',', ';');

            IConnectionMultiplexer redisMultiplexer;
            try
            {
                var options = ConfigurationOptions.Parse(redisConfig);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 10000;
                redisMultiplexer = ConnectionMultiplexer.Connect(options);
            }
            catch
            {
                redisMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
            }

            services.AddSingleton<IConnectionMultiplexer>(redisMultiplexer);

            var dpBuilder = services.AddDataProtection()
                .SetApplicationName("NextShopV2")
                .PersistKeysToStackExchangeRedis(redisMultiplexer, "DataProtection-Keys-V3");

            // Support optional file-system backing for keys and custom certificate path via environment variables
            var certPathEnv = Environment.GetEnvironmentVariable("DP_CERT_PATH");
            var certPath = !string.IsNullOrEmpty(certPathEnv) ? certPathEnv : Path.Combine(Directory.GetCurrentDirectory(), "dp_key.pfx");
            var certPassword = Environment.GetEnvironmentVariable("DP_CERT_PASSWORD") ?? "NextShopDefaultPassword123!"; 
            var keysPath = Environment.GetEnvironmentVariable("DP_KEYS_PATH");

            if (!string.IsNullOrEmpty(keysPath))
            {
                try
                {
                    var dir = new DirectoryInfo(keysPath);
                    if (!dir.Exists) dir.Create();
                    dpBuilder.PersistKeysToFileSystem(dir);
                    Console.WriteLine($"🗄️ DataProtection keys persisted to file system: {keysPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to persist DataProtection keys to file system: {ex.Message}");
                }
            }

            try
            {
                if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
                {
                    var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword);
                    dpBuilder.ProtectKeysWithCertificate(cert);
                    Console.WriteLine("🛡️ DataProtection is protected with certificate.");
                }
                else if (OperatingSystem.IsWindows())
                {
                    // Use Windows DPAPI on Windows hosts
                    dpBuilder.ProtectKeysWithDpapi();
                    Console.WriteLine("🛡️ DataProtection is protected with Windows DPAPI.");
                }
                else
                {
                    // If keys are persisted to Redis, prefer to log that fact rather than warn about missing certificate
                    if (redisMultiplexer != null)
                    {
                        Console.WriteLine("ℹ️ DataProtection keys are persisted to Redis. To additionally protect keys at rest, set DP_CERT_PATH to a .pfx certificate.");
                    }
                    else if (!string.IsNullOrEmpty(keysPath))
                    {
                        Console.WriteLine("🗄️ DataProtection keys persisted to filesystem (un-encrypted). Consider setting DP_CERT_PATH to protect them with a certificate.");
                    }
                    else
                    {
                        Console.WriteLine("ℹ️ DataProtection is using default protection (no certificate found). To protect keys in Linux, set DP_CERT_PATH or DP_KEYS_PATH.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ DataProtection encryption not configured: {ex.Message}");
            }

            services.AddSingleton<IDistributedCache>(provider =>
            {
                var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
                return new RedisCache(new RedisCacheOptions
                {
                    ConnectionMultiplexerFactory = () => Task.FromResult(multiplexer)
                });
            });

            services.AddScoped<IDatabase>(serviceProvider =>
            {
                var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
                return redis.GetDatabase();
            });

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? configuration["Jwt:Key"] ?? "";
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs/notifications") || path.StartsWithSegments("/hubs/shipment-tracking")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            try
                            {
                                var db = context.HttpContext.RequestServices.GetService<IDatabase>();
                                var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                                if (db != null && token != null)
                                {
                                    var authHeader = context.Request.Headers["Authorization"].ToString();
                                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var key = $"blacklist:{authHeader.Substring(7).Trim()}";
                                        var exists = await db.StringGetAsync(key);
                                        if (!exists.IsNullOrEmpty) context.Fail("Token is blacklisted");
                                    }
                                }
                            }
                            catch { }
                        }
                    };
                });

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "NextShop API", 
                    Version = "v1",
                    Description = "NextShop E-commerce API"
                });

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
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                            Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Core & Shared
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddScoped<NextShopV2.Shared.Interfaces.ICacheService>(provider =>
            {
                var redis = provider.GetRequiredService<IConnectionMultiplexer>();
                return new NextShopV2.Shared.Services.RedisCacheService(redis, TimeSpan.FromMinutes(5));
            });
            services.AddScoped<NextShopV2.Shared.Interfaces.IOrderResolutionService, NextShopV2.Shared.Services.OrderResolutionService>();
            services.AddScoped<NextShopV2.Shared.Interfaces.ILoggingService, NextShopV2.Shared.Services.LoggingService>();

            // Repositories & Services
            services.AddScoped<NextShopV2.Application.Interfaces.IAuthService, NextShopV2.Application.Services.AuthService>();
            services.AddScoped<NextShopV2.Application.Interfaces.IUserRepository, NextShopV2.Infrastructure.Repositories.UserRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.IAdvertisementService, NextShopV2.Application.Services.AdvertisementService>();
            services.AddScoped<NextShopV2.Application.Interfaces.IAdvertisementRepository, NextShopV2.Infrastructure.Repositories.AdvertisementRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.IAnnouncementService, NextShopV2.Application.Services.AnnouncementService>();
            services.AddScoped<NextShopV2.Application.Interfaces.IAnnouncementRepository, NextShopV2.Infrastructure.Repositories.AnnouncementRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.IFooterInfoService, NextShopV2.Application.UseCases.FooterInfoService>();
            services.AddScoped<NextShopV2.Domain.Repositories.IFooterInfoRepository, NextShopV2.Infrastructure.Repositories.FooterInfoRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductService, NextShopV2.Application.Services.ProductService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductRepository, NextShopV2.Infrastructure.Repositories.ProductRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IUploadService, NextShopV2.Application.Services.UploadService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IUploadRepository, NextShopV2.Infrastructure.Repositories.UploadRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.services.ICategoryService, NextShopV2.Application.Services.CategoryService>();
            services.AddScoped<NextShopV2.Application.Interfaces.repositories.ICategoryRepository, NextShopV2.Infrastructure.Repositories.CategoryRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.services.IProductCategoryService, NextShopV2.Application.Services.ProductCategoryService>();
            services.AddScoped<NextShopV2.Application.Interfaces.repositories.IProductCategoryRepository, NextShopV2.Infrastructure.Repositories.ProductCategoryRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductVariantService, NextShopV2.Application.Services.ProductVariantService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductVariantRepository, NextShopV2.Infrastructure.Repositories.ProductVariantRepository>();            // Product attribute service & repository
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductAttributeService, NextShopV2.Application.Services.ProductAttributeService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductAttributeRepository, NextShopV2.Infrastructure.Repositories.ProductAttributeRepository>();            services.AddScoped<NextShopV2.Application.Interfaces.Services.IOrderService, NextShopV2.Application.Services.OrderService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IOrderRepository, NextShopV2.Infrastructure.Repositories.OrderRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IShipmentService, NextShopV2.Application.Services.ShipmentService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IShipmentRepository, NextShopV2.Infrastructure.Repositories.ShipmentRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.ITrackingService, NextShopV2.Application.Services.TrackingService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.ITrackingEventRepository, NextShopV2.Infrastructure.Repositories.TrackingEventRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IInventoryService, NextShopV2.Application.Services.InventoryService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IInventoryTransactionRepository, NextShopV2.Infrastructure.Repositories.InventoryTransactionRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.ICouponService, NextShopV2.Application.Services.CouponService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.ICouponRepository, NextShopV2.Infrastructure.Repositories.CouponRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductLikeService, NextShopV2.Application.Services.ProductLikeService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IProductLikeRepository, NextShopV2.Infrastructure.Repositories.ProductLikeRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IReviewService, NextShopV2.Application.Services.ReviewService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IReviewRepository, NextShopV2.Infrastructure.Repositories.ReviewRepository>();
            
            services.AddSingleton<NextShopV2.Application.Interfaces.Services.IFirebaseNotificationService, NextShopV2.Infrastructure.Services.FirebaseNotificationService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IEmailService, NextShopV2.Infrastructure.Services.EmailService>();
            services.AddScoped<NextShopV2.Domain.Repositories.IFirebasePushTokenRepository, NextShopV2.Infrastructure.Repositories.FirebasePushTokenRepository>();
            services.AddScoped<NextShopV2.Domain.Repositories.INotificationHistoryRepository, NextShopV2.Infrastructure.Repositories.NotificationHistoryRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IPasskeyRepository, NextShopV2.Infrastructure.Repositories.PasskeyRepository>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IWebAuthnService, NextShopV2.Infrastructure.Services.WebAuthnService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IPushNotificationService, NextShopV2.Application.UseCases.FirebaseNotificationService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.ISocketNotificationService, NextShopV2.Api.Services.SocketNotificationService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IPaymentService, NextShopV2.Infrastructure.Services.PaymentService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IProductVariantCartService, NextShopV2.Application.UseCases.ProductVariantCartService>();
            services.AddScoped<NextShopV2.Application.Interfaces.Services.IRedisCartService, NextShopV2.Infrastructure.Services.RedisCartService>();

            // External Clients
            services.AddSingleton<PayOSClient>(sp =>
            {
                return new PayOSClient(new PayOSOptions
                {
                    ClientId = configuration["PayOS:ClientId"] ?? Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"),
                    ApiKey = configuration["PayOS:ApiKey"] ?? Environment.GetEnvironmentVariable("PAYOS_API_KEY"),
                    ChecksumKey = configuration["PayOS:ChecksumKey"] ?? Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"),
                    LogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                });
            });

            return services;
        }
    }
}
