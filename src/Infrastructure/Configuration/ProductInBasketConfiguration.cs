using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductInBasketConfiguration : IEntityTypeConfiguration<ProductInBasket>
    {
        public void Configure(EntityTypeBuilder<ProductInBasket> builder)
        {
            builder.HasKey(current => current.Id);

            builder.Property(current => current.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Count)
                   .IsRequired();

            builder.HasOne(current => current.Basket)
                   .WithMany(basket => basket.ProductInBaskets)
                   .HasForeignKey(current => current.BasketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(current => current.Product)
                   .WithMany(product => product.ProductInBaskets)
                   .HasForeignKey(current => current.ProductId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
