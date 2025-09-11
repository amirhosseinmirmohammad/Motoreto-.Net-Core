using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class BlogConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId);

            builder.HasOne(current => current.User)
                .WithMany(applicationUser => applicationUser.Blogs)
                .HasForeignKey(current => current.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(current => current.Category)
                .WithMany(category => category.Blogs)
                .HasForeignKey(current => current.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
