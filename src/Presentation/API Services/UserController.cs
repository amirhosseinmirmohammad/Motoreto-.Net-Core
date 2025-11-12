using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Presentation.APIServices
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserController> _logger;
        const string BaseUrl = "https://motoreto.ir/";

        public UserController(ApplicationDbContext db, ILogger<UserController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("Categories/List")]
        public IActionResult GetCategories()
        {
            _logger.LogInformation("Request:GetCategories started");

            try
            {
                var categories = _db.Categories
                    .Where(c => c.SmallImage != null && c.ParentId == null)
                    .Include(c => c.SubCategories)
                    .ToList();

                if (!categories.Any())
                {
                    _logger.LogInformation("GetCategories: no_categories_found");
                    return Ok(new { Status = 0, Text = "هنوز دسته بندی در سیستم وجود ندارد .", Count = 0 });
                }

                var services = categories.Select(item => new
                {
                    item.Id,
                    Title = item.PersianName,
                    Image = BaseUrl + item.SmallImage,
                    SubCount = item.SubCategories.Count
                }).ToList();

                _logger.LogInformation("GetCategories:success {@meta}", new { Count = services.Count });

                return Ok(new { Status = 1, Object = services, Count = services.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCategories:exception");
                return StatusCode(500, new { Status = 0, Text = "خطای غیرمنتظره رخ داد." });
            }
        }

        [HttpGet("SubCategories/List")]
        public IActionResult GetSubCategories(int? parentId)
        {
            _logger.LogInformation("Request:GetSubCategories {@meta}", new { parentId });

            if (parentId == null)
            {
                _logger.LogWarning("GetSubCategories:missing_parentId");
                return Ok(new { Status = 0, Text = "لطفا شناسه دسته بندی والد را وارد نمایید .", Count = 0 });
            }

            try
            {
                var categories = _db.Categories
                    .Where(c => c.SmallImage != null && c.ParentId == parentId)
                    .Include(c => c.SubCategories)
                    .ToList();

                if (!categories.Any())
                {
                    _logger.LogInformation("GetSubCategories:no_subcategories {@meta}", new { parentId });
                    return Ok(new { Status = 0, Text = "هنوز دسته بندی زیر شاخه ای وجود ندارد .", Count = 0 });
                }

                var services = categories.Select(item => new
                {
                    item.Id,
                    Title = item.PersianName,
                    Image = BaseUrl + item.SmallImage,
                    SubCount = item.SubCategories.Count
                }).ToList();

                _logger.LogInformation("GetSubCategories:success {@meta}", new { parentId, Count = services.Count });

                return Ok(new { Status = 1, Object = services, Count = services.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSubCategories:exception {@meta}", new { parentId });
                return StatusCode(500, new { Status = 0, Text = "خطای غیرمنتظره رخ داد." });
            }
        }

        [HttpGet("LatestProducts/List")]
        public IActionResult GetLatestProducts()
        {
            _logger.LogInformation("Request:GetLatestProducts");

            try
            {
                var products = _db.Products
                    .OrderByDescending(p => p.CreateDate)
                    .Where(p => p.AppSmallImage != null)
                    .Take(8)
                    .ToList();

                var productList = products.Select(item => new
                {
                    item.Id,
                    Name = item.PersianName,
                    Price = item.UnitPrice,
                    DiscountPrice = item.UnitPrice - (item.UnitPrice * item.DiscountPercent / 100),
                    Available = item.Stock > 0,
                    Unit = "ریال",
                    Image = BaseUrl + item.AppSmallImage
                }).ToList();

                _logger.LogInformation("GetLatestProducts:success {@meta}",
                    new { Count = productList.Count });

                return Ok(new { Status = 1, Object = productList, Count = productList.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLatestProducts:exception");
                return StatusCode(500, new { Status = 0, Text = "خطای غیرمنتظره رخ داد." });
            }
        }

        [HttpGet("UserInformation/GetUser")]
        public IActionResult UserInformation(Guid userId)
        {
            _logger.LogInformation("Request:GetUser {@meta}", new { userId });

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetUser:empty_guid");
                return Ok(new { Status = 0, Text = "لطفا شناسه کاربری را وارد نمایید ." });
            }

            try
            {
                var user = _db.Users
                    .Include(u => u.City)
                    .Include(u => u.State)
                    .FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogInformation("GetUser:not_found {@meta}", new { userId });
                    return Ok("کاربری با این شماره یافت نشد.");
                }

                _logger.LogInformation("GetUser:success {@meta}", new { userId });

                return Ok(new
                {
                    user.FirstName,
                    user.LastName,
                    user.ProfileImage,
                    user.Credit,
                    user.AccessCode,
                    user.Email,
                    user.IntroCode,
                    user.PhoneNumber,
                    user.Mobile,
                    user.AddressLine,
                    user.StateId,
                    user.CityId,
                    user.Gender,
                    user.BirthDate,
                    CityName = user.City?.Name ?? "",
                    StateName = user.State?.Name ?? ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUser:exception {@meta}", new { userId });
                return StatusCode(500, new { Status = 0, Text = "خطای غیرمنتظره رخ داد." });
            }
        }

        [HttpGet("Server/Connection")]
        public IActionResult Connection(string userId, string guId)
        {
            _logger.LogInformation("Request:Connection {@meta}", new { userId, guId });

            try
            {
                string guid = string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guId)
                    ? Guid.NewGuid().ToString()
                    : guId;

                _logger.LogInformation("Connection:success {@meta}", new { guidGenerated = string.IsNullOrEmpty(guId) });

                return Ok(new { status = true, GuId = guid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection:exception {@meta}", new { userId, guId });
                return StatusCode(500, new { status = false });
            }
        }

        [HttpGet("/api/home/index")]
        public IActionResult GetHomeIndex()
        {
            _logger.LogInformation("Request:GetHomeIndex");

            try
            {
                var vm = new
                {
                    Sliders = _db.Sliders
                        .Where(s => s.IsApp)
                        .OrderByDescending(s => s.LastDate)
                        .Select(s => new {
                            Image = BaseUrl + s.Image,
                            s.Link
                        }).ToList(),

                    Categories = _db.Categories
                        .Where(c => c.ParentId == null && c.SmallImage != null)
                        .OrderBy(c => c.PersianName == "سایر قطعات" ? 1 : 0)
                        .Select(c => new {
                            c.Id,
                            PersianName = c.PersianName,
                            SmallImage = BaseUrl + c.SmallImage
                        }).ToList(),

                    LatestProducts = _db.Products
                        .Where(p => p.AppSmallImage != null)
                        .OrderByDescending(p => p.CreateDate)
                        .Take(10)
                        .Select(p => new {
                            p.Id,
                            PersianName = p.PersianName,
                            SiteFirstImage = BaseUrl + p.AppSmallImage,
                            p.UnitPrice,
                            p.DiscountPercent,
                            p.SefUrl
                        }).ToList(),

                    Wonder = _db.Products
                        .Where(p => p.IsWonderful && p.Stock > 0 && p.AppLargeImage != null)
                        .OrderByDescending(p => p.CreateDate)
                        .Select(p => new {
                            p.Id,
                            p.PersianName,
                            SiteFirstImage = BaseUrl + p.AppLargeImage,
                            p.UnitPrice,
                            p.DiscountPercent,
                            p.SefUrl
                        }).FirstOrDefault()
                };

                _logger.LogInformation("GetHomeIndex:success {@meta}", new
                {
                    sliders = ((IEnumerable<object>)vm.Sliders).Count(),
                    categories = ((IEnumerable<object>)vm.Categories).Count(),
                    latestProducts = ((IEnumerable<object>)vm.LatestProducts).Count(),
                    wonder = vm.Wonder != null
                });

                return Ok(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHomeIndex:exception");
                return StatusCode(500, new { Status = 0, Text = "خطای غیرمنتظره رخ داد." });
            }
        }
    }
}
