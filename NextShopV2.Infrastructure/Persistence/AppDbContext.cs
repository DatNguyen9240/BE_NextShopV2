using Microsoft.EntityFrameworkCore;
using NextShopV2.Domain.Entities.Users;
using NextShopV2.Domain.Entities.Products;
using NextShopV2.Domain.Entities.Carts;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Domain.Entities.Interactions;
using NextShopV2.Domain.Entities.Coupons;
using NextShopV2.Domain.Entities.Payments;
using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductLike> ProductLikes { get; set; }
        public DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductMedia> ProductMedia { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<OrderCoupon> OrderCoupons { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductLike>()
                .HasKey(l => new { l.ProductId, l.UserId });

            modelBuilder.Entity<OrderCoupon>()
                .HasKey(oc => new { oc.OrderId, oc.CouponId });

                modelBuilder.Entity<ProductMedia>()
                    .HasKey(pm => pm.MediaId);

                modelBuilder.Entity<InventoryTransaction>()
                    .HasKey(it => it.TransactionId);

            modelBuilder.Entity<ProductVariant>()
                .HasKey(pv => pv.VariantId);

            // Ví dụ cho Coupon
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountPercent)
                .HasPrecision(10, 2); // decimal(10,2)

            // Ví dụ cho Order
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // Các entity khác tương tự:
            modelBuilder.Entity<OrderCoupon>()
                .Property(oc => oc.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.AdditionalPrice)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
