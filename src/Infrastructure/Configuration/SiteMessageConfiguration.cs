using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class SiteMessageConfiguration : IEntityTypeConfiguration<SiteMessage>
    {
        public void Configure(EntityTypeBuilder<SiteMessage> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(t => t.FullName)
                   .IsRequired();

            builder.Property(t => t.Body)
                   .IsRequired();
        }
    }
}
