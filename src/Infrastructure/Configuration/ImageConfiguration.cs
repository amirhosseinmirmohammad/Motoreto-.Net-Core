using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.Property(current => current.Id)
                   .IsRequired();

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Alt)
                   .IsRequired()
                   .IsUnicode()
                   .HasMaxLength(256);

            builder.Property(current => current.Link)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(current => current.Source)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(current => current.Title)
                   .IsRequired()
                   .IsUnicode()
                   .HasMaxLength(256);

            builder.HasOne(current => current.Product)
                    .WithMany(product => product.Images)
                    .HasForeignKey(current => current.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
