using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    public class LayoutController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LayoutController(ApplicationDbContext db)
        {
            _db = db;
        }

        // آخرین ۴ مقاله
        [HttpGet]
        public IActionResult LatestBlogs()
        {
            var blogs = _db.Blogs
                .Where(b => b.IsVisible)
                .OrderByDescending(b => b.CreateDate)
                .Take(4)
                .Select(b => new { b.Id, b.Title, b.SefUrl })
                .ToList();

            return Json(blogs);
        }

        // دسته‌بندی‌های اصلی با زیرمجموعه‌ها
        [HttpGet]
        public IActionResult Categories()
        {
            var cats = _db.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.SubCategories)
                .Select(c => new
                {
                    c.Id,
                    c.PersianName,
                    Subs = c.SubCategories.Select(s => new
                    {
                        s.Id,
                        s.PersianName,
                        Subs = _db.Categories.Where(z => z.ParentId == s.Id)
                                             .Select(z => new { z.Id, z.PersianName })
                    })
                })
                .ToList();

            return Json(cats);
        }

        // تعداد اقلام سبد خرید از کوکی
        [HttpGet]
        public IActionResult CartCount()
        {
            var count = Request.Cookies.Keys.Count(k => k.StartsWith("MotoretoCt_"));
            return Json(new { count });
        }
    }
}
