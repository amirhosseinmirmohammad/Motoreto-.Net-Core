namespace Application.DTOs.ViewModels
{
    public class PagerViewModels<TEntity> where TEntity : class
    {
        public IEnumerable<TEntity> data { get; set; }

        public int CurrentPage { get; set; }

        public int TotalItemCount { get; set; }
    }
}
