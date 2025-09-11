using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
    {
        public void Configure(EntityTypeBuilder<BlogComment> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Text)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(t => t.Blog)
                .WithMany(blog => blog.BlogComments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
