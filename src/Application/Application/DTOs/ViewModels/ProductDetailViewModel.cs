using DataLayer.Models;

namespace Application.DTOs.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product product { get; set; }
        public Comment comment { get; set; }
    }
}
