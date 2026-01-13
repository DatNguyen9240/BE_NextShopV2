using NextShopV2.Domain.Entities.Security;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IPasskeyRepository
    {
        Task<Passkey?> GetByCredentialIdAsync(string credentialId);
        Task<IEnumerable<Passkey>> GetByUserIdAsync(Guid userId);
        Task AddAsync(Passkey passkey);
        Task UpdateAsync(Passkey passkey);
        Task DeleteAsync(string id);
    }
}