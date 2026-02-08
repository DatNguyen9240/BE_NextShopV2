using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface ITaxSettingService
    {
        Task<Dictionary<string, string>> GetAllAsync();
        Task<string?> GetValueAsync(string key);
        Task<decimal> GetTaxRateAsync(); // Default tax rate
        Task SetValueAsync(string key, string value, string? description = null);
        Task DeleteAsync(string key);
    }
}
