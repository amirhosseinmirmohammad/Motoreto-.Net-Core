using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class FileConfiguration : IEntityTypeConfiguration<Domain.File>
    {
        public void Configure(EntityTypeBuilder<Domain.File> builder)
        {
            builder.HasKey(current => current.Id);

            builder.Property(current => current.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Extension)
                   .IsRequired()
                   .IsUnicode(true)
                   .HasMaxLength(30);

            builder.Property(current => current.FullPath)
                   .IsRequired()
                   .IsUnicode(true)
                   .HasMaxLength(int.MaxValue);

            builder.Property(current => current.FileName)
                   .IsRequired()
                   .IsUnicode(true)
                   .HasMaxLength(250);

            builder.Property(current => current.FileNameWithoutExtension)
                   .IsRequired()
                   .IsUnicode(true)
                   .HasMaxLength(200);

            builder.Property(current => current.Directory)
                   .IsRequired()
                   .IsUnicode(true)
                   .HasMaxLength(int.MaxValue);

            builder.Property(current => current.FileSize)
                    .IsRequired();

            builder.Property(current => current.UploadDate)
                   .IsRequired();

            builder.HasOne(current => current.Blog)
                   .WithMany(blog => blog.Images)
                   .HasForeignKey(current => current.BlogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
