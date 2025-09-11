using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(current => current.Id);

            builder.HasOne(current => current.Category)
                   .WithMany(category => category.Products)
                   .HasForeignKey(current => current.CategoryId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(current => current.Stock)
                   .IsRequired(false);
        }
    }
}
