using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class MarketingElementRepository : IMarketingElementRepository
    {
        private readonly AppDbContext _context;

        public MarketingElementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MarketingElement?> GetActiveAsync()
        {
            return await _context.MarketingElements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MarketingElement>> GetAllAsync()
        {
            return await _context.MarketingElements
                .OrderByDescending(a => a.UpdatedAt)
                .ToListAsync();
        }

        public async Task<MarketingElement?> GetByIdAsync(Guid id)
        {
            return await _context.MarketingElements.FindAsync(id);
        }

        public async Task AddAsync(MarketingElement marketingElement)
        {
            await _context.MarketingElements.AddAsync(marketingElement);
        }

        public async Task DeleteAsync(MarketingElement marketingElement)
        {
            _context.MarketingElements.Remove(marketingElement);
        }

        public async Task UpdateAsync(MarketingElement marketingElement)
        {
            _context.MarketingElements.Update(marketingElement);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}