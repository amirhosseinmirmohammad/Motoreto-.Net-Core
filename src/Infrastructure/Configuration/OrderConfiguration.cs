using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(current => current.Id);

            builder.Property(current => current.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Receiver)
                   .IsRequired();

            builder.Property(current => current.OrderDate)
                   .IsRequired();

            builder.Property(current => current.TotalPrice)
                   .IsRequired(false);

            builder.Property(current => current.UserId)
                   .IsRequired(false);

            builder.Property(current => current.FactorNumber)
                   .IsRequired(false);

            builder.Property(current => current.IsCanceled)
                    .IsRequired(false);

            builder.Property(current => current.CancelDescription)
                   .IsRequired(false);

            builder.HasOne(current => current.User)
                   .WithMany(user => user.Orders)
                   .HasForeignKey(current => current.UserId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
