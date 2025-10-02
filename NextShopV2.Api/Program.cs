using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using StackExchange.Redis;
using NextShopV2.Application.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NextShopV2.Shared.Extensions.Web;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"
    )
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
        

    });

builder.Services.AddAuthorization();

// Register shared cache service
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.ICacheService>(provider =>
{
    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
    return new NextShopV2.Shared.Services.RedisCacheService(redis, TimeSpan.FromMinutes(5));
});

// Register advertisement cache service
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IAdvertisementCacheService, NextShopV2.Application.Services.AdvertisementCacheService>();

// Register DI for OrderResolutionService (shared utility)
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.IOrderResolutionService, NextShopV2.Shared.Services.OrderResolutionService>();

// Register DI for AuthService and UserRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IAuthService, NextShopV2.Application.Services.AuthService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IUserRepository, NextShopV2.Infrastructure.Repositories.UserRepository>();

// Register DI for BannerService and BannerRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IBannerService, NextShopV2.Application.Services.BannerService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.IBannerRepository, NextShopV2.Infrastructure.Repositories.BannerRepository>();


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

// Register DI for InventoryService and InventoryTransactionRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.IInventoryService, NextShopV2.Application.Services.InventoryService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.IInventoryTransactionRepository, NextShopV2.Infrastructure.Repositories.InventoryTransactionRepository>();

// Register DI for CouponService and CouponRepository
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Services.ICouponService, NextShopV2.Application.Services.CouponService>();
builder.Services.AddScoped<NextShopV2.Application.Interfaces.Repositories.ICouponRepository, NextShopV2.Infrastructure.Repositories.CouponRepository>();

// Register DI for Redis Cart Service (from Shared)
builder.Services.AddScoped<IDatabase>(serviceProvider =>
{
    var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
    return redis.GetDatabase();
});
builder.Services.AddScoped<NextShopV2.Shared.Services.IProductVariantService, NextShopV2.Application.Services.ProductVariantCartService>();
builder.Services.AddScoped<NextShopV2.Shared.Interfaces.IRedisCartService, NextShopV2.Shared.Services.RedisCartService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NextShop API V1");
    c.DocumentTitle = "NextShop API Documentation";
});

// Global error handling middleware
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
