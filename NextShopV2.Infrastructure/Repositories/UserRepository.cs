using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            // Prefer EF Core server-side update for safety and to avoid raw SQL.
            // This produces a single SQL UPDATE executed on the server.
            if (excludeAddressId.HasValue)
            {
                _context.Addresses
                    .Where(a => a.UserId == userId && a.AddressId != excludeAddressId.Value)
                    .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, _ => false));
            }
            else
            {
                _context.Addresses
                    .Where(a => a.UserId == userId)
                    .ExecuteUpdate(s => s.SetProperty(a => a.IsDefault, _ => false));
            }
        }

        // Try to update address atomically using a direct SQL update to avoid tracked entity concurrency issues
        public bool TryUpdateAddress(Guid addressId, string fullAddress, double? latitude, double? longitude, bool isDefault)
        {
            // Use EF Core ExecuteUpdate for an atomic update without tracking.
            var rows = _context.Addresses
                .Where(a => a.AddressId == addressId)
                .ExecuteUpdate(s => s
                    .SetProperty(a => a.FullAddress, _ => fullAddress)
                    .SetProperty(a => a.Latitude, _ => latitude)
                    .SetProperty(a => a.Longitude, _ => longitude)
                    .SetProperty(a => a.IsDefault, _ => isDefault)
                );

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

        // Async methods
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
