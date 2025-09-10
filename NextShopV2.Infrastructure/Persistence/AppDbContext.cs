using Microsoft.EntityFrameworkCore;
using NextShopV2.Domain.Entities;

namespace NextShopV2.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
