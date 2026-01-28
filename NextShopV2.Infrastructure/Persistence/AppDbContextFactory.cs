
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;
using System;
using System.Collections.Generic;
using DotNetEnv;

namespace NextShopV2.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Set environment to Development for design-time
            var environment = "Development";

            // Walk up the directory tree to find .env files (allows running from different CWDs)
            var currentDir = Directory.GetCurrentDirectory();
            var envName = $".env.{environment.ToLower()}";
            string? envPath = null;
            DirectoryInfo? dirInfo = new DirectoryInfo(currentDir);
            while (dirInfo != null)
            {
                var candidate1 = Path.Combine(dirInfo.FullName, envName);
                var candidate2 = Path.Combine(dirInfo.FullName, ".env");
                if (File.Exists(candidate1))
                {
                    envPath = candidate1;
                    break;
                }
                if (File.Exists(candidate2))
                {
                    envPath = candidate2;
                    break;
                }
                dirInfo = dirInfo.Parent;
            }

            if (!string.IsNullOrEmpty(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }


            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Prefer Postgres if environment provides connection details (matches runtime wiring)
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? Environment.GetEnvironmentVariable("Postgres.DATABASE_URL");
            bool isPostgres = false;

            if (!string.IsNullOrWhiteSpace(databaseUrl))
            {
                try
                {
                    // Sanitize value (trim whitespace, remove accidental trailing braces, decode percent-encoding)
                    var raw = Uri.UnescapeDataString(databaseUrl.Trim()).Trim();
                    while (raw.EndsWith("}") || raw.EndsWith(")")) raw = raw.Substring(0, raw.Length - 1).TrimEnd();

                    var uri = new Uri(raw);
                    var userInfo = uri.UserInfo.Split(':', 2);

                    // Parse query params (e.g., sslmode=require, trustServerCertificate=true)
                    var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(uri.Query))
                    {
                        var q = uri.Query.TrimStart('?');
                        foreach (var pair in q.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var kv = pair.Split(new[] { '=' }, 2);
                            var key = Uri.UnescapeDataString(kv[0]);
                            var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
                            queryParams[key] = val;
                        }
                    }

                    // Map sslmode if present, fallback to environment PGSSLMODE, otherwise Require for cloud hosts
                    Npgsql.SslMode sslMode = Npgsql.SslMode.Require;
                    if (queryParams.TryGetValue("sslmode", out var sslModeVal) && !string.IsNullOrWhiteSpace(sslModeVal))
                    {
                        switch (sslModeVal.ToLowerInvariant())
                        {
                            case "disable": sslMode = Npgsql.SslMode.Disable; break;
                            case "prefer": sslMode = Npgsql.SslMode.Prefer; break;
                            case "require": sslMode = Npgsql.SslMode.Require; break;
                            case "verify-ca": sslMode = Npgsql.SslMode.VerifyCA; break;
                            case "verify-full": sslMode = Npgsql.SslMode.VerifyFull; break;
                            default: sslMode = Npgsql.SslMode.Require; break;
                        }
                    }
                    else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PGSSLMODE")))
                    {
                        var envSsl = Environment.GetEnvironmentVariable("PGSSLMODE");
                        if (!string.IsNullOrEmpty(envSsl))
                        {
                            if (envSsl.Equals("disable", StringComparison.OrdinalIgnoreCase)) sslMode = Npgsql.SslMode.Disable;
                        }
                    }

                    var trustServerCertificate = false;
                    if (queryParams.TryGetValue("trustservercertificate", out var trustVal) && !string.IsNullOrWhiteSpace(trustVal))
                    {
                        bool.TryParse(trustVal, out trustServerCertificate);
                    }
                    else if (queryParams.TryGetValue("trustServerCertificate", out var trustVal2) && !string.IsNullOrWhiteSpace(trustVal2))
                    {
                        bool.TryParse(trustVal2, out trustServerCertificate);
                    }

                    var npgBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                    {
                        Host = uri.Host,
                        Port = uri.Port > 0 ? uri.Port : 5432,
                        Database = uri.AbsolutePath.TrimStart('/'),
                        Username = userInfo.Length > 0 ? userInfo[0] : string.Empty,
                        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
                        SslMode = sslMode
                    };

                    // Mask password for logs
                    var maskedUser = !string.IsNullOrEmpty(npgBuilder.Username) ? npgBuilder.Username : "unknown";
                    var maskedLog = $"{maskedUser}:****@{npgBuilder.Host}:{npgBuilder.Port}/{npgBuilder.Database}";
                    Console.WriteLine($"🔧 Design-time using Postgres: {maskedLog} (sslmode={npgBuilder.SslMode}, trustServerCert={trustServerCertificate})");

                    optionsBuilder.UseNpgsql(npgBuilder.ConnectionString, o =>
                    {
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        // Add transient retry policy to handle brief connectivity hiccups
                        o.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null);
                    });
                    isPostgres = true;
                }
                catch (Exception ex)
                {
                    // Log/ignore - design-time factory should not throw for env parsing problems
                    Console.WriteLine($"⚠️ Invalid DATABASE_URL format: {ex.Message}");
                }
            }

            if (!isPostgres)
            {
                var pgHost = Environment.GetEnvironmentVariable("PGHOST");
                var pgDb = Environment.GetEnvironmentVariable("PGDATABASE");
                var pgUser = Environment.GetEnvironmentVariable("PGUSER");
                var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
                var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";

                if (!string.IsNullOrWhiteSpace(pgHost) && !string.IsNullOrWhiteSpace(pgDb) && !string.IsNullOrWhiteSpace(pgUser) && !string.IsNullOrWhiteSpace(pgPassword))
                {
                    var pgConn = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPassword};SSL Mode=Require";
                    optionsBuilder.UseNpgsql(pgConn, o =>
                    {
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        o.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null);
                    });
                    isPostgres = true;
                }
            }

            if (!isPostgres)
            {
                // Fall back to SQL Server (existing behavior)
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
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}