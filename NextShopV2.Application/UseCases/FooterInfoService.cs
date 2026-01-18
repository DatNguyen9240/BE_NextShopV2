using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Domain.Repositories;
using NextShopV2.Application.Interfaces;

namespace NextShopV2.Application.UseCases
{
    public class FooterInfoService : IFooterInfoService
    {
        private readonly IFooterInfoRepository _footerInfoRepository;

        public FooterInfoService(IFooterInfoRepository footerInfoRepository)
        {
            _footerInfoRepository = footerInfoRepository;
        }

        public async Task<FooterInfo?> GetAsync()
        {
            return await _footerInfoRepository.GetAsync();
        }

        public async Task<FooterInfo> UpdateAsync(FooterInfo footerInfo)
        {
            var existing = await _footerInfoRepository.GetAsync();
            if (existing == null)
            {
                footerInfo.CreatedAt = DateTime.UtcNow;
                footerInfo.UpdatedAt = DateTime.UtcNow;
                await _footerInfoRepository.AddAsync(footerInfo);
                return footerInfo;
            }
            else
            {
                existing.ClassName = footerInfo.ClassName;
                existing.UpdatedAt = DateTime.UtcNow;
                await _footerInfoRepository.UpdateAsync(existing);
                return existing;
            }
        }
    }
}