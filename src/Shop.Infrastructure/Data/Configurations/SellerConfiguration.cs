using Shop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Infrastructure.Data.Configurations;

/// <summary>فروشگاه فروشنده مارکت‌پلیس.</summary>
public class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.Property(s => s.StoreName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(170).IsRequired();
        builder.Property(s => s.BusinessLicenseNumber).HasMaxLength(80).IsRequired();
        builder.Property(s => s.BusinessLicenseFileUrl).HasMaxLength(300);
builder.Property(s => s.NationalCardFileUrl).HasMaxLength(300);
builder.Property(s => s.CompanyAdFileUrl).HasMaxLength(300);
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.Address).HasMaxLength(400);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.RejectReason).HasMaxLength(400);

        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.OwnerUserId).IsUnique();
        builder.HasIndex(s => s.Status);

        builder.HasOne(s => s.Owner)
               .WithOne()
               .HasForeignKey<Seller>(s => s.OwnerUserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Offers)
               .WithOne(o => o.Seller)
               .HasForeignKey(o => o.SellerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.Status).HasConversion<int>();
    }
}
