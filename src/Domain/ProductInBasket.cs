using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
   public class ProductInBasket
    {
        public ProductInBasket()
        {
        }
        public int Id { get; set; }

        public long ProductId { get; set; }

        public virtual Product Product { get; set; }

        public long BasketId { get; set; }

        public virtual Basket Basket { get; set; }

        [DisplayName("تعداد محصولات")]
        [Display(Name = "تعداد محصولات")]
        public int Count { get; set; }
    }
}
