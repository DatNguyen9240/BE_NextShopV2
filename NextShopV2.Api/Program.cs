using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var enableHttps = Environment.GetEnvironmentVariable("ENABLE_HTTPS");
if (enableHttps == "true")
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();
