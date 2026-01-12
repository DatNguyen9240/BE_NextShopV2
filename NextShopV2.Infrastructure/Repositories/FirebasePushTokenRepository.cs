using NextShopV2.Domain.Repositories;
using NextShopV2.Domain.Entities.Notifications;
using NextShopV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class FirebasePushTokenRepository : IFirebasePushTokenRepository
    {
        private readonly AppDbContext _context;

        public FirebasePushTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PushToken?> GetByTokenAsync(string token)
        {
            return await _context.PushTokens
                .FirstOrDefaultAsync(pt => pt.Token == token && pt.IsActive);
        }

        public async Task<PushToken?> GetByTokenIncludingInactiveAsync(string token)
        {
            return await _context.PushTokens
                .FirstOrDefaultAsync(pt => pt.Token == token);
        }

        public async Task<IEnumerable<PushToken>> GetActiveTokensAsync()
        {
            return await _context.PushTokens
                .Where(pt => pt.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<PushToken>> GetTokensByUserIdAsync(Guid userId)
        {
            return await _context.PushTokens
                .Where(pt => pt.UserId == userId && pt.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(PushToken pushToken)
        {
            await _context.PushTokens.AddAsync(pushToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PushToken pushToken)
        {
            _context.PushTokens.Update(pushToken);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string token)
        {
            var pushToken = await GetByTokenAsync(token);
            if (pushToken != null)
            {
                pushToken.IsActive = false;
                await UpdateAsync(pushToken);
            }
        }

        public async Task<bool> TokenExistsAsync(string token)
        {
            return await _context.PushTokens
                .AnyAsync(pt => pt.Token == token && pt.IsActive);
        }
    }
}