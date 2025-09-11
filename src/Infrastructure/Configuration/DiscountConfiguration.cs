using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.HasKey(current => current.Id);

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Percent)
                   .IsRequired(false);

            builder.Property(current => current.Title)
                   .IsRequired();

            builder.Property(current => current.Code)
                   .IsRequired();

            builder.HasMany(current => current.Users)
                .WithMany(user => user.Discounts)
                .UsingEntity<Dictionary<string, object>>(
                    "UserDiscounts",
                    j => j.HasOne<User>()
                          .WithMany()
                          .HasForeignKey("UserId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Discount>()
                          .WithMany()
                          .HasForeignKey("DiscountId")
                          .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}
