using Microsoft.EntityFrameworkCore;
using NextShopV2.Domain.Entities;
using NextShopV2.Domain.Repositories;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class TaxSettingRepository : ITaxSettingRepository
    {
        private readonly AppDbContext _context;

        public TaxSettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaxSetting>> GetAllAsync()
        {
            return await _context.TaxSettings.ToListAsync();
        }

        public async Task<TaxSetting?> GetByKeyAsync(string key)
        {
            return await _context.TaxSettings
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<TaxSetting?> GetByIdAsync(Guid id)
        {
            return await _context.TaxSettings.FindAsync(id);
        }

        public async Task AddAsync(TaxSetting setting)
        {
            await _context.TaxSettings.AddAsync(setting);
        }

        public async Task UpdateAsync(TaxSetting setting)
        {
            _context.TaxSettings.Update(setting);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var setting = await GetByIdAsync(id);
            if (setting != null)
            {
                _context.TaxSettings.Remove(setting);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
