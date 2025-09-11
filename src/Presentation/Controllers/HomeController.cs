using Application.DTOs.ViewModels;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Controllers
{
    public class HomeController : Controller
    {

        public HomeController(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCity(int stateId)
        {
            var cities = _db.Cities
                .Where(c => c.StateId == stateId)
                .OrderByDescending(c => c.Name)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(cities);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitMessage([Bind("Id,FullName,Body,Email")] SiteMessage model)
        {
            if (ModelState.IsValid)
            {
                model.RegisterDate = DateTime.Now;
                _db.SiteMessages.Add(model);
                _db.SaveChanges();
                TempData["Success"] = "پیام شما با موفقیت در سیستم ثبت گردید";
                return RedirectToAction("Index", "ContactUs");
            }

            TempData["Error"] = "متاسفانه خطایی رخ داده است لطفا اطلاعات خود را بررسی و مجدد تلاش نمایید";
            return RedirectToAction("Index", "ContactUs");
        }

        [HttpGet]
        public IActionResult NewsLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Json(new { text = "لطفا ایمیل خود را وارد نمایید .", status = 0 });

            if (!new EmailAddressAttribute().IsValid(text))
                return Json(new { text = "لطفا ایمیل معتبری را وارد نمایید .", status = 0 });

            var exists = _db.NewsLetters.Any(n => n.Email == text);
            if (exists)
                return Json(new { text = "ایمیل شما قبلا در سیستم ثبت شده است .", status = 0 });

            var newsletter = new NewsLetter { Email = text };
            _db.NewsLetters.Add(newsletter);
            _db.SaveChanges();

            return Json(new { text = "ایمیل شما با موفقیت در سیستم ثبت شد .", status = 1 });
        }

        [HttpPost]
        public IActionResult AddToShoppingCart(int id)
        {
            var cookieKey = $"MotoretoCt_{id}";
            int count = 0;

            if (Request.Cookies.ContainsKey(cookieKey))
            {
                int.TryParse(Request.Cookies[cookieKey], out count);
                count++;
            }
            else
            {
                var pro = _db.Products.Find(id);
                if (pro == null)
                {
                    return Json(new CardViewModel { Success = false });
                }
                count = (int)(pro.Min > 0 ? pro.Min : 1);
            }

            Response.Cookies.Append(cookieKey, count.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddHours(3),
                HttpOnly = false
            });

            return Json(new CardViewModel
            {
                Success = true,
                CountHtml = CartCount(),
                PriceHtml = CartPrice()
            });
        }

        [HttpPost]
        public IActionResult MinusFromShoppingCart(int id)
        {
            var cookieKey = $"MotoretoCt_{id}";
            if (Request.Cookies.ContainsKey(cookieKey))
            {
                int.TryParse(Request.Cookies[cookieKey], out var count);
                var pro = _db.Products.Find(id);
                int minCount = (int)((pro != null && pro.Min > 0) ? pro.Min : 1);

                if (count > minCount)
                {
                    count--;
                    Response.Cookies.Append(cookieKey, count.ToString(), new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddHours(3),
                        HttpOnly = false
                    });

                    return Json(new CardViewModel
                    {
                        Success = true,
                        CountHtml = CartCount(),
                        PriceHtml = CartPrice()
                    });
                }
            }

            return Json(new CardViewModel { Success = false });
        }

        [HttpPost]
        public IActionResult RemoveCart(int id)
        {
            var cookieKey = $"MotoretoCt_{id}";
            if (Request.Cookies.ContainsKey(cookieKey))
            {
                Response.Cookies.Delete(cookieKey);
                return Json(new CardViewModel
                {
                    Success = true,
                    CountHtml = CartCount(),
                    PriceHtml = CartPrice()
                });
            }

            return Json(new CardViewModel { Success = false });
        }

        private string CartCount()
        {
            var count = Request.Cookies.Keys.Count(k => k.StartsWith("MotoretoCt_"));
            return count.ToString();
        }

        private string CartPrice()
        {
            int totalPrice = 0;
            foreach (var cookie in Request.Cookies.Where(c => c.Key.StartsWith("MotoretoCt_")))
            {
                var id = int.Parse(cookie.Key.Substring(11));
                var count = int.Parse(cookie.Value);
                var product = _db.Products.Find(id);
                if (product != null)
                {
                    var price = product.DiscountPercent > 0
                        ? product.UnitPrice - (product.UnitPrice * product.DiscountPercent / 100)
                        : product.UnitPrice;

                    totalPrice += count * price;
                }
            }
            return totalPrice.ToString("N0");
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var basket = new List<BasketViewModel>();
            foreach (var cookie in Request.Cookies.Where(c => c.Key.StartsWith("MotoretoCt_")))
            {
                var id = int.Parse(cookie.Key.Substring(11));
                var pro = _db.Products.Find(id);
                if (pro != null)
                {
                    basket.Add(new BasketViewModel
                    {
                        Product = pro,
                        Count = int.Parse(cookie.Value)
                    });
                }
            }
            return View(basket);
        }

        [HttpPost]
        public IActionResult GetBasket()
        {
            var productlist = new List<Product>();
            foreach (var cookie in Request.Cookies.Where(c => c.Key.StartsWith("MotoretoCt_")))
            {
                var id = int.Parse(cookie.Key.Substring(11));
                var product = _db.Products.Find(id);
                if (product != null)
                {
                    productlist.Add(product);
                }
            }

            return Json(productlist.Select(p => new { Product = p.PersianName, Count = productlist.Count }));
        }

        [HttpGet]
        public IActionResult InsertOrder(byte? PaymentType, byte? Type)
        {
            ViewBag.StateId = _db.States.OrderByDescending(s => s.Name)
                                        .Select(s => new { s.Id, s.Name })
                                        .ToList();

            if (PaymentType == null || PaymentType == 0 || Type == null || Type == 0)
            {
                TempData["error"] = "لطفا روش ارسال و پرداخت را تعیین نمایید .";
                return RedirectToAction("Cart", "Home");
            }

            if (Convert.ToInt32(CartPrice()) == 0)
                return RedirectToAction("Cart");

            var basket = new List<BasketViewModel>();
            foreach (var cookie in Request.Cookies.Where(c => c.Key.StartsWith("MotoretoCt_")))
            {
                var id = int.Parse(cookie.Key.Substring(11));
                var pro = _db.Products.Find(id);
                if (pro != null)
                {
                    basket.Add(new BasketViewModel
                    {
                        Product = pro,
                        Count = int.Parse(cookie.Value)
                    });
                }
            }

            var viewmodel = new SubmitOrderViewModel
            {
                basket = basket,
                Products = basket.Select(b => b.Product).ToList(),
                discount = new List<DiscountViewModel>(),
                FinalPrice = double.Parse(CartPrice())
            };

            ViewBag.Price = CartPrice();
            return View(viewmodel);
        }

        private void ClearCartCookies()
        {
            foreach (var cookie in Request.Cookies.Keys.Where(k => k.StartsWith("MotoretoCt_")))
            {
                Response.Cookies.Delete(cookie);
            }
        }

        [HttpGet]
        public IActionResult Result()
        {
            return View();
        }


        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
    }
}
