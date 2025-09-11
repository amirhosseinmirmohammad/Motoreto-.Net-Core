using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.Property(current => current.Id)
                   .IsRequired();

            builder.Property(current => current.Description)
                   .IsRequired(false);

            builder.HasOne(current => current.User)
                   .WithMany(user => user.Payments)
                   .HasForeignKey(current => current.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(current => current.Order)
                   .WithMany(order => order.Payments)
                   .HasForeignKey(current => current.OrderId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(current => current.Transaction)
                   .WithMany(transaction => transaction.Payments)
                   .HasForeignKey(current => current.TransactionId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
