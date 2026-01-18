using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Domain.Repositories
{
    public interface IFooterInfoRepository
    {
        Task<FooterInfo?> GetAsync();
        Task AddAsync(FooterInfo footerInfo);
        Task UpdateAsync(FooterInfo footerInfo);
    }
}