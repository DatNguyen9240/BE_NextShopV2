using StackExchange.Redis;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Interfaces
{
    /// <summary>
    /// Generic cache service interface
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }
}