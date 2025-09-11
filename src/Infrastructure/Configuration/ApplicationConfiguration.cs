using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.Property(current => current.Id)
                   .IsRequired();

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.UserName)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(current => current.Title)
                   .IsRequired()
                   .IsUnicode()
                   .HasMaxLength(256);

            builder.Property(current => current.EmailAddress)
                   .IsRequired()
                   .HasMaxLength(256);
                   
            builder.Property(current => current.Password)
                   .IsRequired()
                   .HasMaxLength(256);
                   
            builder.Property(current => current.PortNumber)
                   .IsRequired();

            builder.Property(current => current.FromNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(current => current.SmtpServer)
                   .IsRequired()
                   .HasMaxLength(256);
        }
    }
}
