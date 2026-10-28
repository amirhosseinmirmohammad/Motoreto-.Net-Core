using Shop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Icon).HasMaxLength(50);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.ParentId);

        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Children)
               .HasForeignKey(c => c.ParentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.EnglishName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(220).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.MainImage).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Description).IsRequired();

        // ایندکس‌های پرفورمنس برای فیلتر/مرتب‌سازی پرتکرار
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => new { p.CategoryId, p.IsActive });
        builder.HasIndex(p => new { p.IsActive, p.IsSpecial });
        builder.HasIndex(p => new { p.IsActive, p.IsAmazing });
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.UnitPrice);
        builder.HasIndex(p => p.SalesCount);

        builder.HasMany(p => p.Offers)
               .WithOne(o => o.Product)
               .HasForeignKey(o => o.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>پیشنهاد فروش فروشنده برای یک کالا (مارکت‌پلیس).</summary>
public class SellerOfferConfiguration : IEntityTypeConfiguration<SellerOffer>
{
    public void Configure(EntityTypeBuilder<SellerOffer> builder)
    {
        // هر فروشنده برای هر کالا فقط یک پیشنهاد دارد
        builder.HasIndex(o => new { o.ProductId, o.SellerId }).IsUnique();
        builder.HasIndex(o => o.SellerId);
        builder.HasIndex(o => o.IsActive);

        builder.Property(o => o.Warranty).HasMaxLength(150);

        builder.HasOne(o => o.Seller)
               .WithMany(s => s.Offers)
               .HasForeignKey(o => o.SellerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>تاریخچه قیمت برای نمودار روند قیمت.</summary>
public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.HasIndex(h => new { h.ProductId, h.RecordedAt });

        builder.HasOne(h => h.Product)
               .WithMany()
               .HasForeignKey(h => h.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>کارت انبار (دفتر کل گردش موجودی).</summary>
public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasIndex(m => new { m.ProductId, m.CreatedAt });
        builder.HasIndex(m => m.Type);

        builder.Property(m => m.Reference).HasMaxLength(60);
        builder.Property(m => m.Note).HasMaxLength(300);

        builder.HasOne(m => m.Product)
               .WithMany()
               .HasForeignKey(m => m.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(m => m.Type).HasConversion<int>();
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.Property(i => i.Url).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Alt).HasMaxLength(200);

        builder.HasIndex(i => new { i.ProductId, i.SortOrder });

        builder.HasOne(i => i.Product)
               .WithMany(p => p.Images)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.Property(c => c.GuestToken).HasMaxLength(64);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.GuestToken);

        builder.HasMany(c => c.Items)
               .WithOne(i => i.Cart)
               .HasForeignKey(i => i.CartId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        // یک محصول/آفر فقط یک‌بار در سبد (تعداد در فیلد Quantity)
        builder.HasIndex(i => new { i.CartId, i.ProductId, i.OfferId });
        builder.HasIndex(i => i.ProductId);

        builder.HasOne(i => i.Product)
               .WithMany(p => p.CartItems)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
