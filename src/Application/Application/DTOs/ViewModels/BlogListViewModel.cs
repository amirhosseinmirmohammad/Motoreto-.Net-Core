using Domain;

namespace Application.DTOs.ViewModels
{
    public class BlogListViewModel
    {
        public PagerViewModels<Blog> Pager { get; set; }

        public BlogSidebarViewModel Sidebar { get; set; }
    }
}
