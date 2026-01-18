using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Domain.Repositories;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class FooterInfoRepository : IFooterInfoRepository
    {
        private readonly AppDbContext _context;

        public FooterInfoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FooterInfo?> GetAsync()
        {
            return await _context.FooterInfos.FirstOrDefaultAsync();
        }

        public async Task AddAsync(FooterInfo footerInfo)
        {
            _context.FooterInfos.Add(footerInfo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FooterInfo footerInfo)
        {
            _context.FooterInfos.Update(footerInfo);
            await _context.SaveChangesAsync();
        }
    }
}