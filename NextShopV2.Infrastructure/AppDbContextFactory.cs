using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NextShopV2.Infrastructure.Persistence;

namespace NextShopV2.Infrastructure
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}