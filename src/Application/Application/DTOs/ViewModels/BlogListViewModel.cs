using Domain;

namespace Application.DTOs.ViewModels
{
    public class BlogListViewModel
    {
        /// <summary>
        /// لیست صفحه‌بندی شده مقالات
        /// </summary>
        public X.PagedList.IPagedList<Blog> PagedBlogs { get; set; }   // ✅ از X.PagedList استفاده کن

        /// <summary>
        /// سایدبار (مثل پربازدیدها)
        /// </summary>
        public BlogSidebarViewModel Sidebar { get; set; }
    }
}
