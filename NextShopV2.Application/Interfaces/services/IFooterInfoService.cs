using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Application.Interfaces
{
    public interface IFooterInfoService
    {
        Task<FooterInfo?> GetAsync();
        Task<FooterInfo> UpdateAsync(FooterInfo footerInfo);
    }
}