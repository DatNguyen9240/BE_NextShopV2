using StackExchange.Redis;
using Newtonsoft.Json;
using NextShopV2.Shared.Interfaces;
using NextShopV2.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Services
{
    /// <summary>
    /// Generic Redis cache service implementation
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly TimeSpan _defaultExpiry;

        public RedisCacheService(IConnectionMultiplexer redis, TimeSpan? defaultExpiry = null)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _defaultExpiry = defaultExpiry ?? TimeSpan.FromMinutes(30); // Default 30 minutes
        }

        /// <summary>
        /// Get cached item by key
        /// </summary>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var cached = await _database.StringGetAsync(key);
            if (!cached.HasValue)
                return null;

            return JsonConvert.DeserializeObject<T>(cached!);
        }

        /// <summary>
        /// Set item in cache with optional expiry
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            var serialized = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, serialized, expiry ?? _defaultExpiry);
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        /// <summary>
        /// Remove all keys matching pattern
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var keys = server.Keys(pattern: pattern).ToList();
            
            if (keys.IsNullOrEmpty()) 
                return;
            
            var deleteTasks = keys.Select(key => _database.KeyDeleteAsync(key));
            await Task.WhenAll(deleteTasks);
        }
    }
}