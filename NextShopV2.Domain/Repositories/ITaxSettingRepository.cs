using NextShopV2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Domain.Repositories
{
    public interface ITaxSettingRepository
    {
        Task<List<TaxSetting>> GetAllAsync();
        Task<TaxSetting?> GetByKeyAsync(string key);
        Task<TaxSetting?> GetByIdAsync(Guid id);
        Task AddAsync(TaxSetting setting);
        Task UpdateAsync(TaxSetting setting);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
    }
}
