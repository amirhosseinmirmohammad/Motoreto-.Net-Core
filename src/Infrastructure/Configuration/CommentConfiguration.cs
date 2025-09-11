using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Text)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasOne(t => t.User)
                   .WithMany(u => u.Comments)
                   .HasForeignKey(d => d.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.Product)
                   .WithMany(product => product.Comments)
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
