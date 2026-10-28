using Shop.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shop.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Text).HasMaxLength(2000).IsRequired();

        builder.HasIndex(c => new { c.ProductId, c.IsApproved });
        builder.HasIndex(c => c.UserId);

        builder.HasOne(c => c.Product)
               .WithMany(p => p.Comments)
               .HasForeignKey(c => c.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
               .WithMany(u => u.Comments)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(220).IsRequired();
        builder.Property(b => b.ShortDescription).HasMaxLength(500).IsRequired();
        builder.Property(b => b.CoverImage).HasMaxLength(300).IsRequired();

        builder.HasIndex(b => b.Slug).IsUnique();
        builder.HasIndex(b => new { b.IsVisible, b.CreatedAt });

        builder.HasMany(b => b.Comments)
               .WithOne(c => c.Blog)
               .HasForeignKey(c => c.BlogId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BlogCommentConfiguration : IEntityTypeConfiguration<BlogComment>
{
    public void Configure(EntityTypeBuilder<BlogComment> builder)
    {
        builder.Property(c => c.FullName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Text).HasMaxLength(2000).IsRequired();

        builder.HasIndex(c => new { c.BlogId, c.IsApproved });
    }
}

public class SliderConfiguration : IEntityTypeConfiguration<Slider>
{
    public void Configure(EntityTypeBuilder<Slider> builder)
    {
        builder.Property(s => s.Title).HasMaxLength(150).IsRequired();
        builder.Property(s => s.SubTitle).HasMaxLength(250);
        builder.Property(s => s.Image).HasMaxLength(300).IsRequired();
        builder.Property(s => s.Link).HasMaxLength(300);
        builder.Property(s => s.Theme).HasMaxLength(20);

        builder.HasIndex(s => new { s.IsActive, s.SortOrder });
    }
}

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.Property(s => s.StoreName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Tagline).HasMaxLength(200);
        builder.Property(s => s.LogoUrl).HasMaxLength(300);
        builder.Property(s => s.PrimaryColor).HasMaxLength(9);
        builder.Property(s => s.SupportPhone).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(150).IsRequired();
        builder.Property(s => s.InstagramUrl).HasMaxLength(300);
        builder.Property(s => s.TelegramUrl).HasMaxLength(300);
        builder.Property(s => s.PaymentGateway).HasMaxLength(50);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(50);
        builder.Property(u => u.LastName).HasMaxLength(50);
        builder.Property(u => u.Mobile).HasMaxLength(15);
        builder.Property(u => u.AddressLine).HasMaxLength(300);
        builder.Property(u => u.PostalCode).HasMaxLength(12);

        builder.HasIndex(u => u.Mobile);

        builder.HasOne(u => u.State)
               .WithMany()
               .HasForeignKey(u => u.StateId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.City)
               .WithMany()
               .HasForeignKey(u => u.CityId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.StateId);

        builder.HasOne(c => c.State)
               .WithMany(s => s.Cities)
               .HasForeignKey(c => c.StateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Name).IsUnique();
    }
}

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();

        builder.HasOne(f => f.User)
               .WithMany(u => u.Favorites)
               .HasForeignKey(f => f.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Product)
               .WithMany(p => p.Favorites)
               .HasForeignKey(f => f.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NewsLetterSubscriberConfiguration : IEntityTypeConfiguration<NewsLetterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsLetterSubscriber> builder)
    {
        builder.Property(n => n.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(n => n.Email).IsUnique();
    }
}

public class SiteMessageConfiguration : IEntityTypeConfiguration<SiteMessage>
{
    public void Configure(EntityTypeBuilder<SiteMessage> builder)
    {
        builder.Property(m => m.FullName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Email).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Subject).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Body).HasMaxLength(2000).IsRequired();

        builder.HasIndex(m => new { m.IsRead, m.CreatedAt });
    }
}
