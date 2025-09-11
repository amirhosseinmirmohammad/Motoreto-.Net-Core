namespace Application.DTOs.ViewModels
{
    public class UserDashboardViewModel
    {
        public int OrdersCount { get; set; }

        public int PaymentsCount { get; set; }

        public int MessagesCount { get; set; }

        public int UserScore { get; set; }

        public decimal Credit { get; set; }

        public string PersianToday { get; set; }

        public List<Domain.Notification> Messages { get; set; } = new();
    }
}
