using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using StackExchange.Redis;
using NextShopV2.Api.Services;

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
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AdvertisementCacheService>();

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Global error handling middleware
app.UseMiddleware<NextShopV2.Api.Middlewares.ExceptionMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
