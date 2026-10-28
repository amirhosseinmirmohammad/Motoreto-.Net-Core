using Shop.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Data;

/// <summary>DbContext اصلی فروشگاه.</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<SellerOffer> SellerOffers => Set<SellerOffer>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogComment> BlogComments => Set<BlogComment>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<NewsLetterSubscriber> NewsLetterSubscribers => Set<NewsLetterSubscriber>();
    public DbSet<SiteMessage> SiteMessages => Set<SiteMessage>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<SellerCategory> SellerCategories => Set<SellerCategory>();
    public DbSet<BrandCategory> BrandCategories => Set<BrandCategory>();
    public DbSet<Affiliate> Affiliates => Set<Affiliate>();
    public DbSet<AffiliateLead> AffiliateLeads => Set<AffiliateLead>();
    public DbSet<ShopFeed> ShopFeeds => Set<ShopFeed>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();

    // ---- آگهی‌های دست دوم (مدل دیوار) ----
    public DbSet<ClassifiedCategory> ClassifiedCategories => Set<ClassifiedCategory>();
    public DbSet<ClassifiedAttribute> ClassifiedAttributes => Set<ClassifiedAttribute>();
    public DbSet<ClassifiedAd> ClassifiedAds => Set<ClassifiedAd>();
    public DbSet<ClassifiedChatMessage> ClassifiedChatMessages => Set<ClassifiedChatMessage>();
    public DbSet<ClassifiedFavorite> ClassifiedFavorites => Set<ClassifiedFavorite>();
    public DbSet<ClassifiedSavedSearch> ClassifiedSavedSearches => Set<ClassifiedSavedSearch>();
    public DbSet<ClassifiedAdViewDaily> ClassifiedAdViewDailies => Set<ClassifiedAdViewDaily>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ایندکس یکتا برای upsert بازدید روزانه آگهی (ON CONFLICT)
        modelBuilder.Entity<ClassifiedAdViewDaily>()
            .HasIndex(v => new { v.AdId, v.Date })
            .IsUnique();

        // جفت‌کلیدهای junction (دسته‌بندی چندگانه محصول + دسته‌های فعالیت فروشنده + برند↔دسته)
        modelBuilder.Entity<ProductCategory>(e => e.HasKey(pc => new { pc.ProductId, pc.CategoryId }));
        modelBuilder.Entity<SellerCategory>(e => e.HasKey(sc => new { sc.SellerId, sc.CategoryId }));
        modelBuilder.Entity<BrandCategory>(e => e.HasKey(bc => new { bc.BrandId, bc.CategoryId }));

        // افیلیت: کد یکتا + یک کد برای هر کاربر
        modelBuilder.Entity<Affiliate>(e =>
        {
            e.HasIndex(a => a.Code).IsUnique();
            e.HasIndex(a => a.UserId).IsUnique();
        });

        // جدول‌های Identity: نام‌گذاری یکدست (identity_*)
        string[] identityTables =
        [
            "AspNetRoles", "AspNetRoleClaims", "AspNetUsers", "AspNetUserClaims",
            "AspNetUserLogins", "AspNetUserRoles", "AspNetUserTokens"
        ];
        string[] snakeNames =
        [
            "identity_roles", "identity_role_claims", "identity_users", "identity_user_claims",
            "identity_user_logins", "identity_user_roles", "identity_user_tokens"
        ];

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var table = entityType.GetTableName();
            if (table is null) continue;

            var index = Array.IndexOf(identityTables, table);
            if (index >= 0)
            {
                entityType.SetTableName(snakeNames[index]);
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
