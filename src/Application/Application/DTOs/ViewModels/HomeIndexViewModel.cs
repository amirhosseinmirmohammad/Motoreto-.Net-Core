namespace Application.DTOs.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Domain.Slider> Sliders { get; set; }

        public Domain.Application Application { get; set; }

        public Domain.Product Wonder { get; set; }

        public List<Domain.Category> Categories { get; set; }

        public List<Domain.Category> CarCategories { get; set; }

        public List<Domain.Product> LatestProducts { get; set; }

        public List<Domain.Product> SpecialProducts { get; set; }
    }
}
