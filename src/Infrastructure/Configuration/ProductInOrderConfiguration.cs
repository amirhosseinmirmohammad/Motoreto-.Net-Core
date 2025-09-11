using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductInOrderConfiguration : IEntityTypeConfiguration<ProductInOrder>
    {
        public void Configure(EntityTypeBuilder<ProductInOrder> builder)
        {
            builder.HasKey(current => current.Id);

            builder.Property(current => current.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Count)
                   .IsRequired();

            builder.HasOne(current => current.Order)
                   .WithMany(order => order.Products)
                   .HasForeignKey(current => current.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(current => current.Product)
                   .WithMany(product => product.Orders)
                   .HasForeignKey(current => current.ProductId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
