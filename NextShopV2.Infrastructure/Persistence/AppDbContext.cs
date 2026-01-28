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

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<ProductLike> ProductLikes { get; set; } = null!;
        public DbSet<Advertisement> Advertisements { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<FooterInfo> FooterInfos { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Orders.Shipment> Shipments { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Orders.TrackingEvent> TrackingEvents { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<OrderCoupon> OrderCoupons { get; set; } = null!;
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;

        // Product attributes and values
        public DbSet<NextShopV2.Domain.Entities.Products.ProductAttribute> ProductAttributes { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Products.AttributeValue> AttributeValues { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Products.VariantAttributeValue> VariantAttributeValues { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Products.CategoryAttribute> CategoryAttributes { get; set; } = null!;

        // Push notification tokens (FCM)
        public DbSet<NextShopV2.Domain.Entities.Notifications.PushToken> PushTokens { get; set; } = null!;
        public DbSet<NextShopV2.Domain.Entities.Notifications.NotificationHistory> NotificationHistories { get; set; } = null!;

        // Passkeys (WebAuthn)
        public DbSet<NextShopV2.Domain.Entities.Security.Passkey> Passkeys { get; set; } = null!;

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

            // Product attributes
            modelBuilder.Entity<NextShopV2.Domain.Entities.Products.ProductAttribute>(eb => {
                eb.HasKey(a => a.AttributeId);
                eb.Property(a => a.Name).IsRequired();
            });

            modelBuilder.Entity<NextShopV2.Domain.Entities.Products.AttributeValue>(eb => {
                eb.HasKey(av => av.AttributeValueId);
                eb.Property(av => av.Value).IsRequired();
                eb.HasOne(av => av.Attribute)
                    .WithMany(a => a.Values)
                    .HasForeignKey(av => av.AttributeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NextShopV2.Domain.Entities.Products.VariantAttributeValue>(eb => {
                eb.HasKey(vav => new { vav.VariantId, vav.AttributeValueId });
                eb.HasOne(vav => vav.Variant)
                    .WithMany()
                    .HasForeignKey(vav => vav.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
                eb.HasOne(vav => vav.AttributeValue)
                    .WithMany(av => av.VariantAttributeValues)
                    .HasForeignKey(vav => vav.AttributeValueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NextShopV2.Domain.Entities.Products.CategoryAttribute>(eb => {
                eb.HasKey(ca => new { ca.CategoryId, ca.AttributeId });
                eb.HasOne(ca => ca.Category)
                    .WithMany()
                    .HasForeignKey(ca => ca.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                eb.HasOne(ca => ca.Attribute)
                    .WithMany()
                    .HasForeignKey(ca => ca.AttributeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

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

            modelBuilder.Entity<User>(eb => {
                eb.HasIndex(u => u.Phone).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
