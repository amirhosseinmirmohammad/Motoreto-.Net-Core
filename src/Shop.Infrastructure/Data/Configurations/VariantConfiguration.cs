using Shop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Infrastructure.Data.Configurations;

/// <summary>تنوع محصول (SKU): هر ترکیب ویژگی‌ها با قیمت/موجودی مستقل.</summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.Property(v => v.AttributesJson).HasMaxLength(1000).IsRequired();
        builder.Property(v => v.Image).HasMaxLength(300);

        builder.HasIndex(v => new { v.ProductId, v.IsActive });

        builder.HasOne(v => v.Product)
               .WithMany(p => p.Variants)
               .HasForeignKey(v => v.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
