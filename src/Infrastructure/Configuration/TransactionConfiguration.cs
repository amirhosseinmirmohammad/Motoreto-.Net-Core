using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(current => current.Acceptor)
                   .HasMaxLength(50)
                   .IsRequired()
                   .IsUnicode();

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.AcceptorPhoneNumber)
                   .IsRequired();

            builder.Property(current => current.AcceptorPostalCode)
                   .IsRequired();

            builder.Property(current => current.BankName)
                   .HasMaxLength(50)
                   .IsRequired()
                   .IsUnicode();

            builder.Property(current => current.CardNumber)
                   .IsRequired();

            builder.Property(current => current.InvoiceNumber)
                   .IsRequired();

            builder.Property(current => current.Number)
                .IsRequired();

            builder.HasOne(current => current.Order)
                   .WithMany(order => order.Transactions)
                   .HasForeignKey(current => current.OrderId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(current => current.RecievedDocumentNumber)
                   .IsRequired();

            builder.Property(current => current.TerminalNumber)
                   .IsRequired();
        }
    }
}
