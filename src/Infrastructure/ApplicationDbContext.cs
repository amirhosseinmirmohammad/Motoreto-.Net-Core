using Domain;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : base(context) { }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SiteMessage> SiteMessages { get; set; }

        public DbSet<State> States { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductInOrder> ProductInOrders { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<UserTransactions> UserTransactions { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<NewsLetter> NewsLetters { get; set; }

        public DbSet<Discount> Discounts { get; set; }

        public DbSet<Slider> Sliders { get; set; }

        public DbSet<Gallery> Galleries { get; set; }

        public DbSet<Basket> Baskets { get; set; }

        public DbSet<ProductInBasket> ProductInBaskets { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<BlogComment> BlogComments { get; set; }

        public DbSet<Domain.File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new ApplicationConfiguration().Configure(modelBuilder.Entity<Application>());
            new BlogCommentConfiguration().Configure(modelBuilder.Entity<BlogComment>());
            new BlogConfiguration().Configure(modelBuilder.Entity<Blog>());
            new CategoryConfiguration().Configure(modelBuilder.Entity<Category>());
            new CityConfiguration().Configure(modelBuilder.Entity<City>());
            new CommentConfiguration().Configure(modelBuilder.Entity<Comment>());
            new DiscountConfiguration().Configure(modelBuilder.Entity<Discount>());
            new FileConfiguration().Configure(modelBuilder.Entity<Domain.File>());
            new GalleryConfiguration().Configure(modelBuilder.Entity<Gallery>());
            new ImageConfiguration().Configure(modelBuilder.Entity<Image>());
            new NotificationConfiguration().Configure(modelBuilder.Entity<Notification>());
            new OrderConfiguration().Configure(modelBuilder.Entity<Order>());
            new PaymentConfiguration().Configure(modelBuilder.Entity<Payment>());
            new ProductInBasketConfiguration().Configure(modelBuilder.Entity<ProductInBasket>());
            new ProductInOrderConfiguration().Configure(modelBuilder.Entity<ProductInOrder>());
            new ProductConfiguration().Configure(modelBuilder.Entity<Product>());
            new SiteMessageConfiguration().Configure(modelBuilder.Entity<SiteMessage>());
            new SliderConfiguration().Configure(modelBuilder.Entity<Slider>());
            new StateConfiguration().Configure(modelBuilder.Entity<State>());
            new TransactionConfiguration().Configure(modelBuilder.Entity<Transaction>());
            new FavoriteConfiguration().Configure(modelBuilder.Entity<Favorite>());
            new NewsLetterConfiguration().Configure(modelBuilder.Entity<NewsLetter>());
        }
    }
}
