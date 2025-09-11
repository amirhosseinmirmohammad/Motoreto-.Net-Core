using Application.DTOs.ViewModels;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Controllers
{
    public class ProductController : Controller
    {
        public ProductController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult List(string sort = "newest", string q = "", int page = 1, int pageSize = 24)
        {
            var query = BuildQuery(sort, q);

            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToList();

            ViewBag.Sort = sort;
            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.HasMore = total > page * pageSize;

            return View(items);
        }

        public PartialViewResult ListPartial(string sort = "newest", string q = "", int page = 1, int pageSize = 24)
        {
            var query = BuildQuery(sort, q);

            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToList();

            ViewBag.Page = page;
            ViewBag.HasMore = total > page * pageSize;

            return PartialView("_ProductListPartial", items);
        }

        private IQueryable<Product> BuildQuery(string sort, string q)
        {
            var data = _db.Products.AsQueryable().Where(p => p.Stock >= 0);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                data = data.Where(p =>
                    p.PersianName.Contains(q) ||
                    p.EnglishName.Contains(q) ||
                    p.Description.Contains(q));
            }

            switch ((sort ?? "newest").ToLower())
            {
                case "cheap": data = data.OrderBy(p => p.UnitPrice); break;
                case "expensive": data = data.OrderByDescending(p => p.UnitPrice); break;
                case "bestseller": data = data.OrderByDescending(p => p.Orders.Count()); break;
                case "discount": data = data.OrderByDescending(p => p.DiscountPercent); break;
                default: data = data.OrderByDescending(p => p.CreateDate); break;
            }

            return data;
        }

        // GET: Product/Details/sefUrl
        public IActionResult Details(string sefUrl)
        {
            if (string.IsNullOrEmpty(sefUrl))
                return BadRequest();

            var product = _db.Products
                .Where(p => p.SefUrl == sefUrl)
                .Include(p => p.RelatedProducts)
                .Include(p => p.Category)
                .Include(p => p.Category.Parent)
                .Include(p => p.Images)
                .FirstOrDefault();

            if (product == null)
                return NotFound();

            Response.Cookies.Append($"SeenProduct_{product.Id}", "1", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMonths(1),
                HttpOnly = true
            });

            var vm = new ProductDetailViewModel { product = product };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Search(string search, int page = 1)
        {
            var products = _db.Products.Where(p =>
                p.PersianName.Contains(search) ||
                p.EnglishName.Contains(search) ||
                p.Description.Contains(search));

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                data = products.OrderByDescending(p => p.CreateDate)
                               .ThenByDescending(p => p.PersianName)
                               .Skip((page - 1) * 10)
                               .Take(10)
                               .ToList(),
                TotalItemCount = products.Count()
            };
            return View("All", vm);
        }

        [HttpGet]
        public IActionResult Filter(string title, byte? discount, int? category, int? min, int? max, int page = 1)
        {
            var query = _db.Products.Where(p => p.SiteFirstImage != null);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(p => p.PersianName.Contains(title) || p.Description.Contains(title) || p.EnglishName.Contains(title));

            if (category.HasValue)
                query = query.Where(p => p.CategoryId == category.Value);

            if (discount.HasValue)
                query = query.Where(p => p.DiscountPercent >= discount.Value);

            if (min.HasValue)
                query = query.Where(p => p.UnitPrice >= min.Value);

            if (max.HasValue)
                query = query.Where(p => p.UnitPrice <= max.Value);

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                data = query.OrderByDescending(p => p.CreateDate)
                            .ThenByDescending(p => p.PersianName)
                            .Skip((page - 1) * 12)
                            .Take(12)
                            .ToList(),
                TotalItemCount = query.Count()
            };
            return View("All", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertComment(Comment comment, long productId, string email, string fullName)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    comment.UserId = user.Id;
            }

            comment.Fullname = fullName;
            comment.Email = email;
            comment.ProductId = productId;
            comment.DateTime = DateTime.Now;

            _db.Comments.Add(comment);

            try
            {
                _db.SaveChanges();
                TempData["Success"] = "نظر شما با موفقیت ثبت شد و در انتظار تایید ادمین میباشد .";
            }
            catch
            {
                TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .";
            }

            return RedirectToAction("Details", new { sefUrl = _db.Products.Find(productId)?.SefUrl });
        }

        [Route("product/bycar/{name}")]
        public IActionResult ByCar(string name, int page = 1, int pageSize = 8)
        {
            var baseCategories = _db.Categories.Where(c => c.PersianName == name).Select(c => c.Id).ToList();
            var allCategoryIds = _db.Categories
                .Where(c => baseCategories.Contains(c.Id) || baseCategories.Contains(c.ParentId ?? 0))
                .Select(c => c.Id)
                .ToList();

            var query = _db.Products.Where(p => allCategoryIds.Contains(p.CategoryId));
            var products = query.OrderByDescending(p => p.CreateDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                TotalItemCount = query.Count(),
                data = products
            };

            ViewBag.CarName = name;
            return View("All", vm);
        }

        [HttpGet]
        [Route("Product/LoadMoreProducts")]
        public IActionResult LoadMoreProducts(int page, int pageSize, string q = "", string sort = "new", string filter = "", string name = "")
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                var baseCategories = _db.Categories.Where(c => c.PersianName == name).Select(c => c.Id).ToList();
                var allCategoryIds = _db.Categories
                    .Where(c => baseCategories.Contains(c.Id) || baseCategories.Contains(c.ParentId ?? 0))
                    .Select(c => c.Id)
                    .ToList();
                query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p => p.PersianName.Contains(q) || p.EnglishName.Contains(q) || p.Description.Contains(q));

            switch (filter)
            {
                case "inStock": query = query.Where(p => p.Stock > 0); break;
                case "special": query = query.Where(p => p.IsSpecial); break;
                case "hasImage": query = query.Where(p => !string.IsNullOrEmpty(p.SiteFirstImage)); break;
            }

            switch (sort)
            {
                case "bestsellers": query = query.OrderByDescending(p => p.Orders.Count()); break;
                case "expensive": query = query.OrderByDescending(p => p.UnitPrice); break;
                case "cheap": query = query.OrderBy(p => p.UnitPrice); break;
                case "discount": query = query.OrderByDescending(p => p.DiscountPercent); break;
                default: query = query.OrderByDescending(p => p.CreateDate); break;
            }

            var total = query.Count();
            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.HasMore = total > page * pageSize;
            ViewBag.Page = page;

            if (!products.Any() && page == 1)
                return PartialView("_NoProductsPartial");

            if (!products.Any())
                return Content("");

            return PartialView("_ProductListPartial", products);
        }

        public IActionResult All(string search, int? id, int page = 1)
        {
            var query = _db.Products.AsQueryable();

            if (id.HasValue)
            {
                var categoryIds = _db.Categories.Where(c => c.Id == id || c.ParentId == id).Select(c => c.Id).ToList();
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.PersianName.Contains(search) || p.EnglishName.Contains(search) || p.Description.Contains(search));

            var products = query.ToList();

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                data = products,
                TotalItemCount = products.Count()
            };

            var category = _db.Categories.FirstOrDefault(c => c.Id == id || c.ParentId == id);
            ViewBag.CarName = category?.PersianName;

            return View(vm);
        }

        public IActionResult Special(string search, int page = 1)
        {
            var products = string.IsNullOrEmpty(search)
                ? _db.Products.Where(p => p.IsSpecial).ToList()
                : _db.Products.Where(p => p.PersianName.Contains(search) || p.EnglishName.Contains(search) || p.Description.Contains(search)).ToList();

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                data = products.Where(p => p.IsSpecial).OrderByDescending(p => p.CreateDate).ThenByDescending(p => p.PersianName).Skip((page - 1) * 10).Take(10).ToList(),
                TotalItemCount = products.Count(p => p.IsSpecial)
            };

            return View(vm);
        }


        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
    }
}
