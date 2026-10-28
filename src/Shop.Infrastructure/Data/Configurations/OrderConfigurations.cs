using Shop.Domain;
using Shop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(o => o.ReceiverName).HasMaxLength(100).IsRequired();
        builder.Property(o => o.Phone).HasMaxLength(15).IsRequired();
        builder.Property(o => o.Address).HasMaxLength(500).IsRequired();
        builder.Property(o => o.PostalCode).HasMaxLength(12);
        builder.Property(o => o.Note).HasMaxLength(500);
        builder.Property(o => o.DiscountCode).HasMaxLength(50);
        builder.Property(o => o.CancelReason).HasMaxLength(400);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => new { o.UserId, o.CreatedAt });
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);

        builder.Property(o => o.Status).HasConversion<int>();
        builder.Property(o => o.PaymentMethod).HasConversion<int>();
        builder.Property(o => o.ShippingMethod).HasConversion<int>();

        builder.HasOne(o => o.User)
               .WithMany(u => u.Orders)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.State)
               .WithMany()
               .HasForeignKey(o => o.StateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.City)
               .WithMany()
               .HasForeignKey(o => o.CityId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
               .WithOne(p => p.Order)
               .HasForeignKey(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(i => i.ProductName).HasMaxLength(220).IsRequired();
        builder.Property(i => i.ProductImage).HasMaxLength(300).IsRequired();

        builder.Property(i => i.SellerId).HasColumnName("seller_id");

        builder.HasIndex(i => i.OrderId);
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.SellerId);

        builder.HasOne(i => i.Product)
               .WithMany(p => p.OrderItems)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Gateway).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Authority).HasMaxLength(64).IsRequired();
        builder.Property(p => p.RefId).HasMaxLength(64);

        builder.HasIndex(p => p.Authority).IsUnique();
        builder.HasIndex(p => p.OrderId);

        builder.Property(p => p.Status).HasConversion<int>();
    }
}

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasIndex(r => new { r.UserId, r.Status });
        builder.HasIndex(r => new { r.Status, r.ExpiresAt });
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.OfferId);

        builder.HasOne(r => r.Product)
               .WithMany()
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Order)
               .WithMany()
               .HasForeignKey(r => r.OrderId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(r => r.Status).HasConversion<int>();
    }
}

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.Property(d => d.Code).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Title).HasMaxLength(150).IsRequired();

        builder.HasIndex(d => d.Code).IsUnique();
    }
}
