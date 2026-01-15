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

        // Push notification tokens (FCM)
        public DbSet<NextShopV2.Domain.Entities.Notifications.PushToken> PushTokens { get; set; }
        public DbSet<NextShopV2.Domain.Entities.Notifications.NotificationHistory> NotificationHistories { get; set; }

        // Passkeys (WebAuthn)
        public DbSet<NextShopV2.Domain.Entities.Security.Passkey> Passkeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductLike>()
                .HasKey(l => new { l.ProductId, l.UserId });

            modelBuilder.Entity<OrderCoupon>()
                .HasKey(oc => new { oc.OrderId, oc.CouponId });

                modelBuilder.Entity<InventoryTransaction>()
                    .HasKey(it => it.TransactionId);

            modelBuilder.Entity<ProductVariant>()
                .HasKey(pv => pv.VariantId);

            // Ví dụ cho Coupon
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountPercent)
                .HasPrecision(10, 2); // decimal(10,2)
            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinOrderAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MaxDiscountAmount)
                .HasPrecision(18, 2);

            // Ví dụ cho Order
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 0);

            // Các entity khác tương tự:
            modelBuilder.Entity<OrderCoupon>()
                .Property(oc => oc.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Snapshot numeric fields precision
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.DiscountAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TaxAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TaxRate)
                .HasPrecision(5, 4);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalAmount)
                .HasPrecision(18, 2);

            // Make Variant relation optional and set delete behavior to SetNull so orders keep history when variants/products are removed
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Variant)
                .WithMany()
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.BasePrice)
                .HasPrecision(18, 0);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.DiscountAmount)
                .HasPrecision(18, 0);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.DiscountPercent)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.PriceAfterDiscount)
                .HasPrecision(18, 0);


            // Configure PushToken
            modelBuilder.Entity<NextShopV2.Domain.Entities.Notifications.PushToken>(eb => {
                eb.HasKey(p => p.PushTokenId);
                eb.Property(p => p.Token).IsRequired();
                eb.Property(p => p.Platform).HasMaxLength(50).IsRequired();
            });

            // Configure Passkey
            modelBuilder.Entity<NextShopV2.Domain.Entities.Security.Passkey>(eb => {
                eb.HasKey(p => p.Id);
                eb.Property(p => p.CredentialId).IsRequired();
                eb.Property(p => p.PublicKey).IsRequired();
                eb.Property(p => p.Counter).HasDefaultValue(0);
                eb.Property(p => p.Transports);
                eb.Property(p => p.CreatedAt);
                eb.Property(p => p.LastUsedAt);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
