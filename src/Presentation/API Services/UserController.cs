using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.APIServices
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("Categories/List")]
        public IActionResult GetCategories()
        {
            var categories = _db.Categories
                .Where(c => c.SmallImage != null && c.ParentId == null)
                .Include(c => c.SubCategories)
                .ToList();

            if (!categories.Any())
                return Ok(new { Status = 0, Text = "هنوز دسته بندی در سیستم وجود ندارد .", Count = 0 });

            var services = categories.Select(item => new
            {
                item.Id,
                Title = item.PersianName,
                Image = BaseUrl + item.SmallImage,
                SubCount = item.SubCategories.Count
            }).ToList();

            return Ok(new { Status = 1, Object = services, Count = services.Count });
        }

        [HttpGet("SubCategories/List")]
        public IActionResult GetSubCategories(int? parentId)
        {
            if (parentId == null)
                return Ok(new { Status = 0, Text = "لطفا شناسه دسته بندی والد را وارد نمایید .", Count = 0 });

            var categories = _db.Categories
                .Where(c => c.SmallImage != null && c.ParentId == parentId)
                .Include(c => c.SubCategories)
                .ToList();

            if (!categories.Any())
                return Ok(new { Status = 0, Text = "هنوز دسته بندی زیر شاخه ای وجود ندارد .", Count = 0 });

            var services = categories.Select(item => new
            {
                item.Id,
                Title = item.PersianName,
                Image = BaseUrl + item.SmallImage,
                SubCount = item.SubCategories.Count
            }).ToList();

            return Ok(new { Status = 1, Object = services, Count = services.Count });
        }

        [HttpGet("LatestProducts/List")]
        public IActionResult GetLatestProducts()
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

            return Ok(new { Status = 1, Object = productList, Count = productList.Count });
        }

        [HttpGet("UserInformation/GetUser")]
        public IActionResult UserInformation(Guid userId)
        {
            if (userId == Guid.Empty)
                return Ok(new { Status = 0, Text = "لطفا شناسه کاربری را وارد نمایید ." });

            var user = _db.Users
                .Include(u => u.City)
                .Include(u => u.State)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return Ok("کاربری با این شماره یافت نشد.");

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

        [HttpGet("Server/Connection")]
        public IActionResult Connection(string userId, string guId)
        {
            string guid = string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guId)
                ? Guid.NewGuid().ToString()
                : guId;

            return Ok(new { status = true, GuId = guid });
        }


        private readonly ApplicationDbContext _db;
        const string BaseUrl = "https://motoreto.ir/";
    }
}
