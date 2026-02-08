using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Domain.Entities;
using NextShopV2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class TaxSettingService : ITaxSettingService
    {
        private readonly ITaxSettingRepository _settingRepo;
        private const string TaxRateKey = "TaxRate";

        public TaxSettingService(ITaxSettingRepository settingRepo)
        {
            _settingRepo = settingRepo;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var settings = await _settingRepo.GetAllAsync();
            return settings.ToDictionary(s => s.Key, s => s.Value);
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _settingRepo.GetByKeyAsync(key);
            return setting?.Value;
        }

        public async Task<decimal> GetTaxRateAsync()
        {
            var value = await GetValueAsync(TaxRateKey);
            if (string.IsNullOrEmpty(value))
            {
                // Default to 10% if not set
                return 0.10m;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
            {
                return rate;
            }

            return 0.10m;
        }

        public async Task SetValueAsync(string key, string value, string? description = null)
        {
            var existing = await _settingRepo.GetByKeyAsync(key);
            
            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
                if (description != null)
                {
                    existing.Description = description;
                }
                await _settingRepo.UpdateAsync(existing);
            }
            else
            {
                var newSetting = new TaxSetting
                {
                    SettingId = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };
                await _settingRepo.AddAsync(newSetting);
            }

            await _settingRepo.SaveAsync();
        }

        public async Task DeleteAsync(string key)
        {
            var setting = await _settingRepo.GetByKeyAsync(key);
            if (setting != null)
            {
                await _settingRepo.DeleteAsync(setting.SettingId);
                await _settingRepo.SaveAsync();
            }
        }
    }
}
