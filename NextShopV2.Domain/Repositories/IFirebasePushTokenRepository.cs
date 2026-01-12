using NextShopV2.Domain.Entities.Notifications;

namespace NextShopV2.Domain.Repositories
{
    public interface IFirebasePushTokenRepository
    {
        Task<PushToken?> GetByTokenAsync(string token);
        Task<PushToken?> GetByTokenIncludingInactiveAsync(string token);
        Task<IEnumerable<PushToken>> GetActiveTokensAsync();
        Task<IEnumerable<PushToken>> GetTokensByUserIdAsync(Guid userId);
        Task AddAsync(PushToken pushToken);
        Task UpdateAsync(PushToken pushToken);
        Task DeleteAsync(string token);
        Task<bool> TokenExistsAsync(string token);
    }
}