namespace Application.DTOs.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int MessagesCount { get; set; }

        public int OrdersCount { get; set; }

        public int UsersCount { get; set; }

        public string Today { get; set; }

        public List<string> Dates { get; set; } = new();

        public List<int> OrdersPerDay { get; set; } = new();

        public List<int> UsersPerDay { get; set; } = new();
    }
}
