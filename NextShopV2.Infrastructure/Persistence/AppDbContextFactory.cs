using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using System;
using DotNetEnv;
using Npgsql;

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

            // Build connection string (prefer Postgres if DATABASE_URL or PG* env vars exist)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                              ?? config["DATABASE_URL"]
                              ?? Environment.GetEnvironmentVariable("Postgres.DATABASE_URL")
                              ?? config["Postgres:DATABASE_URL"];

            // Postgres env vars
            var pgHost = Environment.GetEnvironmentVariable("PGHOST") ?? config["PGHOST"];
            var pgDb = Environment.GetEnvironmentVariable("PGDATABASE") ?? config["PGDATABASE"];
            var pgUser = Environment.GetEnvironmentVariable("PGUSER") ?? config["PGUSER"];
            var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? config["PGPASSWORD"];
            var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? config["PGPORT"] ?? "5432";

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

                    optionsBuilder.UseNpgsql(npgBuilder.ConnectionString);
                }
                catch
                {
                    // Fall back to SQL Server path below if parsing fails
                }
            }
            else if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDb) && !string.IsNullOrWhiteSpace(pgUser) && !string.IsNullOrWhiteSpace(pgPassword))
            {
                var pgConn = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword};SSL Mode=Prefer;Trust Server Certificate=true";
                optionsBuilder.UseNpgsql(pgConn);
            }
            else
            {
                // Fallback to SQL Server (existing behavior)
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
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}