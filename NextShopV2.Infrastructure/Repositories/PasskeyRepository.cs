using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Security;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure.Repositories
{
    public class PasskeyRepository : IPasskeyRepository
    {
        private readonly AppDbContext _context;

        public PasskeyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Passkey passkey)
        {
            _context.Passkeys.Add(passkey);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.Passkeys.FindAsync(id);
            if (entity != null)
            {
                _context.Passkeys.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Passkey?> GetByCredentialIdAsync(string credentialId)
        {
            return await _context.Passkeys.FirstOrDefaultAsync(p => p.CredentialId == credentialId);
        }

        public async Task<IEnumerable<Passkey>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Passkeys.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task UpdateAsync(Passkey passkey)
        {
            _context.Passkeys.Update(passkey);
            await _context.SaveChangesAsync();
        }
    }
}