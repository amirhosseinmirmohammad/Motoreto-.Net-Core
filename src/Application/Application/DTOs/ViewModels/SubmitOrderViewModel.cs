using Domain;

namespace Application.DTOs.ViewModels
{
    public class SubmitOrderViewModel
    {
        public IEnumerable<BasketViewModel> basket { get; set; }

        public IEnumerable<DiscountViewModel> discount { get; set; }

        public IEnumerable<Product> Products { get; set; }

        public double FinalPrice { get; set; }

        public UserOrder order { get; set; }
    }

    public class UserOrder
    {
        public string UserOrderDescription { get; set; }
        public string Discount { get; set; }
        public byte PaymentType { get; set; }
        public byte Type { get; set; }
    }
}
