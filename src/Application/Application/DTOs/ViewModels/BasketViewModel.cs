using DataLayer.Models;

namespace Application.DTOs.ViewModels
{
    public class BasketViewModel
    {
        public Product Product { get; set; }
        public int Count { get; set; }
    }

    public class SeenViewModel
    {
        public Product Product { get; set; }
        public int Count { get; set; }
    }
}
