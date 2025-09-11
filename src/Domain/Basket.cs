using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
   public class Basket
    {
        public Basket()
        {
            ProductInBaskets = new List<ProductInBasket>();
        }

        public long Id { get; set; }

        [DisplayName("شناسه کاربر")]
        [Display(Name = "شناسه کاربر")]
        public Guid UserId { get; set; }

        [DisplayName("شناسه مهمان")]
        [Display(Name = "شناسه مهمان")]
        public string GuId { get; set; }

        [DisplayName("تاریخ ایجاد سبد")]
        [Display(Name = "تاریخ ایجاد سبد")]
        public DateTime CreateDate { get; set; }

        public virtual ICollection<ProductInBasket> ProductInBaskets { get; set; }
    }
}
