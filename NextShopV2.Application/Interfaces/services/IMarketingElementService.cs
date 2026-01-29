using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces
{
    public interface IMarketingElementService
    {
        Task<MarketingElement?> GetActiveAsync();
        Task<List<MarketingElement>> GetAllAsync();
        Task<MarketingElement> CreateAsync(MarketingElement marketingElement);
        Task<bool> UpdateAsync(Guid id, MarketingElement marketingElement);
        Task<bool> DeleteAsync(Guid id);
    }
}