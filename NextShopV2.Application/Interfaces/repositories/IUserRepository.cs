using NextShopV2.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetById(Guid id);
        void Add(User user);
        bool ExistsByEmail(string email);
        void Save();

        // Async methods
        Task<List<User>> GetAllAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task SaveAsync();

        // Unset default flag for user's addresses; if excludeAddressId is provided, leave that address alone
        void UnsetDefaultAddresses(Guid userId, Guid? excludeAddressId = null);

        // Try to update an address atomically; returns true if a row was updated
        bool TryUpdateAddress(Guid addressId, string fullAddress, double? latitude, double? longitude, bool isDefault);

        // Insert an address record (no user object required)
        void InsertAddress(Domain.Entities.Users.Address address);
    }
}