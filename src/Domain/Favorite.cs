using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Domain
{
    public class Favorite
    {
        public Favorite()
        {
        }

        internal class configuration : EntityTypeConfiguration<Favorite>
        {
            public configuration()
            {
                HasRequired(ne => ne.Users)
                .WithMany(applicationUser => applicationUser.Favorites)
                .HasForeignKey(id => id.UserId)
                .WillCascadeOnDelete(false);

                HasRequired(ne => ne.Product)
                .WithMany(Product => Product.Favorites)
                .HasForeignKey(id => id.ProductId)
                .WillCascadeOnDelete(false);
            }
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        [Bindable(false)]
        public int Id { get; set; }

        [Display(Name = "نام کاربر")]
        [DisplayName("نام کاربر")]
        public string UserId { get; set; }

        public virtual User Users { get; set; }

        [Display(Name = "نام کاربر")]
        [DisplayName("نام کاربر")]
        public long ProductId { get; set; }

        public virtual Product Product { get; set; }

        public DateTime? Date { get; set; }
    }
}
