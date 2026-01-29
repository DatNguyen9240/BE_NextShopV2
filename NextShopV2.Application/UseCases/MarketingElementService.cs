using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Services
{
    public class MarketingElementService : IMarketingElementService
    {
        private readonly IMarketingElementRepository _repo;

        public MarketingElementService(IMarketingElementRepository repo)
        {
            _repo = repo;
        }

        public async Task<MarketingElement?> GetActiveAsync()
        {
            return await _repo.GetActiveAsync();
        }

        public async Task<List<MarketingElement>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<MarketingElement> CreateAsync(MarketingElement marketingElement)
        {
            marketingElement.UpdatedAt = DateTime.UtcNow;
            await _repo.AddAsync(marketingElement);
            await _repo.SaveAsync();
            return marketingElement;
        }

        public async Task<bool> UpdateAsync(Guid id, MarketingElement marketingElement)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = marketingElement.Name;
            existing.ClassName = marketingElement.ClassName;
            existing.IsActive = marketingElement.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            if (marketingElement.IsActive)
            {
                // Deactivate all others
                var all = await _repo.GetAllAsync();
                foreach (var a in all.Where(a => a.Id != id && a.IsActive))
                {
                    a.IsActive = false;
                    await _repo.UpdateAsync(a);
                }
            }

            await _repo.UpdateAsync(existing);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            await _repo.DeleteAsync(existing);
            await _repo.SaveAsync();
            return true;
        }
    }
}