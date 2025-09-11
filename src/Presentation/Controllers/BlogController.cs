using Application.DTOs.ViewModels;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .FirstOrDefaultAsync(b => b.Id == id && b.Images.Any());

            if (blog == null) return NotFound();

            blog.Survey++;
            await _db.SaveChangesAsync();                

            ViewBag.comments = await _db.BlogComments
                .Where(c => c.BlogId == id && c.IsApprove)
                .ToListAsync();

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

            var list = await query
                .OrderByDescending(b => b.CreateDate)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(q) && list.Count == 0)
                TempData["NotFound"] = "متاسفانه موردی پیدا نشد .";
            else if (string.IsNullOrWhiteSpace(q) && list.Count == 0)
                TempData["NotFound"] = "هنوز مطلبی در سایت وجود ندارد .";

            var vm = new PagerViewModels<Blog>
            {
                data = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                TotalItemCount = list.Count
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
