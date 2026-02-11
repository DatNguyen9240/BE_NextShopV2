using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.repositories;
using NextShopV2.Domain.Entities.Marketing;
using NextShopV2.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class WelcomeVoucherIssuanceRepository : IWelcomeVoucherIssuanceRepository
    {
        private readonly AppDbContext _context;
        public WelcomeVoucherIssuanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AnyByIdentifierHashAsync(string identifierHash)
        {
            return await _context.Set<WelcomeVoucherIssuance>().AnyAsync(w => w.IdentifierHash == identifierHash);
        }

        public async Task AddAsync(WelcomeVoucherIssuance issuance)
        {
            await _context.Set<WelcomeVoucherIssuance>().AddAsync(issuance);
            await _context.SaveChangesAsync();
        }
    }
}