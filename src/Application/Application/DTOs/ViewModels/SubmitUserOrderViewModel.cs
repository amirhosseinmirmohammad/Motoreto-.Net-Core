using Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ViewModels
{
    public class SubmitUserOrderViewModel
    {
        public SubmitUserOrderViewModel()
        {
            basket = new List<BasketViewModel>();
            discount = new List<DiscountViewModel>();
        }

        [DisplayName("نام و نام خانوادگی کاربر")]
        [Required(ErrorMessage = "لطفا نام و نام خانوادگی خود را وارد نمایید .")]
        public string FullName { get; set; }

        [DisplayName("شماره موبایل فعال")]
        [RegularExpression(@"(\+98|0)?9\d{9}", ErrorMessage = "لطفا شماره موبایل معتبری را وارد کنید")]
        [Required(ErrorMessage = "لطفا شماره موبایل فعالی را وارد نمایید .")]
        public string Phone { get; set; }

        [DisplayName("آدرس کاربر")]
        [Required(ErrorMessage = "لطفا آدرس کامل خود را وارد نمایید .")]
        public string UserAddress { get; set; }

        [DisplayName("توضیحات کاربر هنگام ثبت سفارش")]
        public string UserOrderDescription { get; set; }

        [DisplayName("کد تخفیف")]
        public string Discount { get; set; }

        public int StateId { get; set; }
        public int CityId { get; set; }

        public byte PaymentType { get; set; }
        public byte Type { get; set; }

        // -------------------------------
        // چیزهایی که ویو نیاز داشت
        // -------------------------------

        /// <summary>
        /// سبد خرید
        /// </summary>
        public IEnumerable<BasketViewModel> basket { get; set; }

        /// <summary>
        /// تخفیف‌های اعمال‌شده
        /// </summary>
        public IEnumerable<DiscountViewModel> discount { get; set; }

        /// <summary>
        /// محصولات نهایی
        /// </summary>
        public IEnumerable<Product> Products { get; set; }

        /// <summary>
        /// جمع کل
        /// </summary>
        public double FinalPrice { get; set; }

        /// <summary>
        /// سفارش (برای توضیحات و جزئیات بیشتر)
        /// </summary>
        public UserOrder order { get; set; }
    }
}
