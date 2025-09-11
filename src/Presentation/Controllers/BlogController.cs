using Application.DTOs.ViewModels;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.EF;

namespace Presentation.Controllers
{
    public class BlogController : Controller
    {
        public BlogController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index(int? id, string sefUrl)
        {
            if (id == null) return BadRequest();

            var blog = await _db.Blogs
                .Include(b => b.Category)
                .Include(b => b.Images)
                .Include(b => b.User)
                .Include(b => b.BlogComments) // برای استفاده در ویو
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return NotFound();

            // افزایش بازدید
            blog.Survey++;
            await _db.SaveChangesAsync();

            // لیست مطالب پربازدید همان دسته (غیر از همین پست)
            var popular = await _db.Blogs
                .Where(b => b.IsVisible && b.CategoryId == blog.CategoryId && b.Id != blog.Id)
                .Include(b => b.Images)
                .OrderByDescending(b => b.Survey)
                .Take(2)
                .ToListAsync();

            ViewBag.PopularBlogs = popular; // ← به ویو بده

            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> All(string q, int page = 1)
        {
            const int pageSize = 4;

            IQueryable<Blog> query = _db.Blogs
                .Where(b => b.Images.Any() && b.IsVisible)
                .Include(b => b.Images);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(b =>
                    b.Title.Contains(q) ||
                    b.ShortDesc.Contains(q) ||
                    b.MetaDesc.Contains(q) ||
                    b.MetaKey.Contains(q) ||
                    b.SefUrl.Contains(q));
            }

            var pagedList = await query
                .OrderByDescending(b => b.CreateDate)
                .ToPagedListAsync(page, pageSize);

            var sidebar = new BlogSidebarViewModel
            {
                PopularBlogs = await _db.Blogs
                    .Where(b => b.IsVisible)
                    .OrderByDescending(b => b.Survey)
                    .Take(4)
                    .ToListAsync()
            };

            var vm = new BlogListViewModel
            {
                PagedBlogs = pagedList,
                Sidebar = sidebar
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment([Bind("Id,FullName,Text,DateTime,IsApprove,BlogId")] BlogComment model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "متاسفانه خطایی رخ داده است لطفا اطلاعات خود را بررسی و مجدد تلاش نمایید";
                return View("Index", model);
            }

            model.DateTime = DateTime.UtcNow; 
            _db.BlogComments.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "نظر شما با موفقیت در سیستم ثبت گردید که پس از تایید مدیریت به نمایش در می آید .";
            return RedirectToAction("Index", "Blog", new { id = model.BlogId });
        }


        private readonly ApplicationDbContext _db;
    }
}
