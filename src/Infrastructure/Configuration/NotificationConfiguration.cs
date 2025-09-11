using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(current => current.Id)
                   .IsRequired();

            builder.Property(t => t.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(current => current.Text)
                   .IsRequired()
                   .IsUnicode();

            builder.HasMany(current => current.Users)
                .WithMany(user => user.Notifications)
                .UsingEntity<Dictionary<string, object>>(
                    "UserInNotifications",
                    j => j.HasOne<User>()
                          .WithMany()
                          .HasForeignKey("UserId")
                          .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<Notification>()
                          .WithMany()
                          .HasForeignKey("NotificationId")
                          .OnDelete(DeleteBehavior.Cascade)
                );
        }
    }
}
