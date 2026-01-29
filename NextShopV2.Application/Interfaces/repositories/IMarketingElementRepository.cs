using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces
{
    public interface IMarketingElementRepository
    {
        Task<MarketingElement?> GetActiveAsync();
        Task<List<MarketingElement>> GetAllAsync();
        Task<MarketingElement?> GetByIdAsync(Guid id);
        Task AddAsync(MarketingElement marketingElement);
        Task UpdateAsync(MarketingElement marketingElement);
        Task DeleteAsync(MarketingElement marketingElement);
        Task SaveAsync();
    }
}