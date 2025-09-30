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
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        public void Add(User user)
        {
            _context.Users.Add(user);
        }
        public bool ExistsByEmail(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
