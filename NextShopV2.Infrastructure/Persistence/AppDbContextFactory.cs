using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using System;
using DotNetEnv;

namespace NextShopV2.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Set environment to Development for design-time
            var environment = "Development";
            
            // Get root directory (where .sln and .env files are located)
            var currentDir = Directory.GetCurrentDirectory();
            var envName = $".env.{environment.ToLower()}";
            
            // Try current dir, then parent dir
            var envPath = Path.Combine(currentDir, envName);
            if (!File.Exists(envPath)) envPath = Path.Combine(currentDir, "..", envName);
            if (!File.Exists(envPath)) envPath = Path.Combine(currentDir, ".env");
            if (!File.Exists(envPath)) envPath = Path.Combine(currentDir, "..", ".env");
            
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Build connection string
            var defaultConn = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            
            string sqlConn;
            if (!string.IsNullOrEmpty(defaultConn))
            {
                sqlConn = defaultConn;
            }
            else if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbName))
            {
                sqlConn = $"Server={dbHost};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
            }
            else
            {
                // Fallback using the credentials from your .env as a template
                sqlConn = "Server=localhost\\SQLEXPRESS03;Database=NextShopDB;User Id=sa;Password=12345;TrustServerCertificate=True;";
            }

            optionsBuilder.UseSqlServer(sqlConn);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}