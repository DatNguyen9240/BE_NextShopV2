using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Linq;

namespace NextShopV2.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }
        public User? GetById(Guid id)
        {
            // Include addresses so callers (e.g., GetMe) receive the user's addresses without lazy loading
            return _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.Id == id);
        }
        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public bool ExistsByEmail(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        // Unset default flags in DB atomically to avoid concurrency on tracked entities
        public void UnsetDefaultAddresses(Guid userId, Guid? excludeAddressId = null)
        {
            if (excludeAddressId.HasValue)
            {
                _context.Database.ExecuteSqlRaw("UPDATE Addresses SET IsDefault = 0 WHERE UserId = {0} AND AddressId <> {1}", userId, excludeAddressId.Value);
            }
            else
            {
                _context.Database.ExecuteSqlRaw("UPDATE Addresses SET IsDefault = 0 WHERE UserId = {0}", userId);
            }
        }

        // Try to update address atomically using a direct SQL update to avoid tracked entity concurrency issues
        public bool TryUpdateAddress(Guid addressId, string fullAddress, double? latitude, double? longitude, bool isDefault)
        {
            // Use parameterized ExecuteSqlRaw to prevent SQL injection and handle nulls
            var rows = _context.Database.ExecuteSqlRaw(
                "UPDATE Addresses SET FullAddress = {1}, Latitude = {2}, Longitude = {3}, IsDefault = {4} WHERE AddressId = {0}",
                addressId, fullAddress, latitude.HasValue ? (object)latitude.Value : DBNull.Value, longitude.HasValue ? (object)longitude.Value : DBNull.Value, isDefault ? 1 : 0);

            return rows > 0;
        }

        // Insert an address directly
        public void InsertAddress(Domain.Entities.Users.Address address)
        {
            _context.Addresses.Add(address);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
