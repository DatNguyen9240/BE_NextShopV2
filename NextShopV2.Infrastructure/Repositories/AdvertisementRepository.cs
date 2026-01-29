using NextShopV2.Application.Interfaces;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace NextShopV2.Infrastructure.Repositories
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly AppDbContext _context;
        public AdvertisementRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Advertisement>> GetAllAsync()
            => await _context.Advertisements
                .OrderBy(a => a.Type)
                .ThenBy(a => a.SortOrder)
                .ToListAsync();

        public async Task<List<Advertisement>> GetByTypeAsync(string type)
            => await _context.Advertisements
                .Where(a => !string.IsNullOrEmpty(a.Type) && a.Type.ToLower() == type.ToLower())
                .OrderBy(a => a.SortOrder)
                .ToListAsync();

        public async Task<Advertisement?> GetByIdAsync(Guid id)
            => await _context.Advertisements.FindAsync(id);

        public async Task<List<Advertisement>> GetByIdsAsync(List<Guid> ids)
            => await _context.Advertisements.Where(a => ids.Contains(a.Id)).ToListAsync();

        public async Task ShiftSortOrdersInRangeAsync(string type, int startInclusive, int endInclusive, int delta)
        {
            if (delta == 0) return;

            // Defensive check: ensure Advertisements table exists in the connected database/schema
            if (!await AdvertisementsTableExistsAsync())
            {
                // Table missing -> log and skip to avoid 500 during runtime when DB is not in expected state
                Console.WriteLine("⚠️ Advertisements table does not exist in the connected database. Skipping sort update.");
                return;
            }

            var normalizedType = type ?? string.Empty;

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(normalizedType))
                    {
                        if (delta > 0)
                        {
                            await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Advertisements SET SortOrder = SortOrder + {delta} WHERE (Type IS NULL OR Type = '') AND SortOrder >= {startInclusive} AND SortOrder <= {endInclusive}");
                        }
                        else
                        {
                            await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Advertisements SET SortOrder = SortOrder - {Math.Abs(delta)} WHERE (Type IS NULL OR Type = '') AND SortOrder >= {startInclusive} AND SortOrder <= {endInclusive}");
                        }
                    }
                    else
                    {
                        if (delta > 0)
                        {
                            await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Advertisements SET SortOrder = SortOrder + {delta} WHERE LOWER(Type) = LOWER({normalizedType}) AND SortOrder >= {startInclusive} AND SortOrder <= {endInclusive}");
                        }
                        else
                        {
                            await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Advertisements SET SortOrder = SortOrder - {Math.Abs(delta)} WHERE LOWER(Type) = LOWER({normalizedType}) AND SortOrder >= {startInclusive} AND SortOrder <= {endInclusive}");
                        }
                    }
                }
                catch (System.IO.EndOfStreamException ex)
                {
                    // Connection was closed unexpectedly; throw to allow the execution strategy to retry if appropriate
                    Console.WriteLine($"⚠️ EndOfStream while executing sort update: {ex.Message}");
                    throw;
                }
                catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "42P01") // relation does not exist
                {
                    Console.WriteLine($"⚠️ Database error (relation not found) while updating sort orders: {ex.Message}");
                    // Table vanished between check and execution - skip to avoid throwing 500 to client
                    return;
                }
            });
        }

        private async Task<bool> AdvertisementsTableExistsAsync()
        {
            try
            {
                var conn = _context.Database.GetDbConnection();
                var wasClosed = conn.State == System.Data.ConnectionState.Closed;
                if (wasClosed) await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                // Check any schema for a table named 'advertisements' (case-insensitive)
                cmd.CommandText = @"SELECT EXISTS(
                    SELECT 1 FROM information_schema.tables WHERE LOWER(table_name) = 'advertisements'
                )";
                var existsObj = await cmd.ExecuteScalarAsync();

                var exists = existsObj is bool b && b;

                if (!exists)
                {
                    // Gather debug info to help identify if schema/search_path is different or DB mismatch
                    using var infoCmd = conn.CreateCommand();
                    infoCmd.CommandText = "SELECT current_database(), current_schema(), current_setting('search_path')";
                    using var reader = await infoCmd.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        var db = reader.IsDBNull(0) ? "(null)" : reader.GetString(0);
                        var schema = reader.IsDBNull(1) ? "(null)" : reader.GetString(1);
                        var searchPath = reader.IsDBNull(2) ? "(null)" : reader.GetString(2);
                        Console.WriteLine($"⚠️ Advertisements table not found. DB={db}, current_schema={schema}, search_path={searchPath}");
                    }
                }

                if (wasClosed) await conn.CloseAsync();
                return exists;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"⚠️ Error checking Advertisements table existence: {ex.Message}");
                return false;
            }
        }

        public async Task IncrementSortOrdersFromAsync(string type, int fromOrder)
            => await ShiftSortOrdersInRangeAsync(type ?? string.Empty, fromOrder, int.MaxValue, 1);

        public async Task DecrementSortOrdersAfterAsync(string type, int afterOrder)
            => await ShiftSortOrdersInRangeAsync(type ?? string.Empty, afterOrder + 1, int.MaxValue, -1);

        public async Task AddAsync(Advertisement banner)
        {
            await _context.Advertisements.AddAsync(banner);
        }
        public Task UpdateAsync(Advertisement banner)
        {
            _context.Advertisements.Update(banner);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(Advertisement banner)
        {
            _context.Advertisements.Remove(banner);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(List<Advertisement> banners)
        {
            _context.Advertisements.RemoveRange(banners);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
