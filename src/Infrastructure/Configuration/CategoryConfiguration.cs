using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(current => current.Id);

            builder.HasOne(current => current.Parent)
                .WithMany(category => category.SubCategories)
                .HasForeignKey(current => current.ParentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
