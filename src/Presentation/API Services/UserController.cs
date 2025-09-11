using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Html;
using System.Data.Entity;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace GladcherryShopping.API_Services
{
    public class UserController : ApiController
    {
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        [AcceptVerbs("GET")]
        [Route("Categories/List")]
        public IHttpActionResult GetCategories()
        {

            #region Validation
            var Categories = _db.Categories.Where(current => current.SmallImage != null && current.ParentId == null).Include(current => current.SubCategories).ToList();
            if (Categories.Count == 0)
                return Ok(new { Status = 0, Text = "هنوز دسته بندی در سیستم وجود ندارد .", Count = 0 });
            #endregion Validation

            #region FindUser
            List<dynamic> Services = new List<dynamic>();
            foreach (var item in Categories)
            {
                Services.Add(new
                {
                    Id = item.Id,
                    Title = item.PersianName,
                    Image = BaseUrl + item.SmallImage,
                    SubCount = item.SubCategories.Count
                });
            }
            return Ok(new { Status = 1, Object = Services, Count = Services.Count });
            #endregion FindUser
        }

        [AcceptVerbs("GET")]
        [Route("SubCategories/List")]
        public IHttpActionResult GetSubCategories(int? ParentId)
        {
            #region Validation
            if (ParentId == null)
                return Ok(new { Status = 0, Text = "لطفا شناسه دسته بندی والد را وارد نمایید .", Count = 0 });
            var Categories = _db.Categories.Where(current => current.SmallImage != null && current.ParentId == ParentId).Include(current => current.SubCategories).ToList();
            if (Categories.Count == 0)
                return Ok(new { Status = 0, Text = "هنوز دسته بندی زیر شاخه ای وجود ندارد .", Count = 0 });
            #endregion Validation

            #region FindUser
            List<dynamic> Services = new List<dynamic>();
            foreach (var item in Categories)
            {
                Services.Add(new
                {
                    Id = item.Id,
                    Title = item.PersianName,
                    Image = BaseUrl + item.SmallImage,
                    SubCount = item.SubCategories.Count
                });
            }
            return Ok(new { Status = 1, Object = Services, Count = Services.Count });
            #endregion FindUser
        }

        [AcceptVerbs("GET")]
        [Route("LatestProducts/List")]
        public IHttpActionResult GetLatestProducts()
        {

            #region Intialize
            var Products = _db.Products.OrderByDescending(current => current.CreateDate).Where(current => current.AppSmallImage != null).Take(8);
            #endregion Intialize

            #region FindUser
            List<dynamic> productList = new List<dynamic>();
            foreach (var item in Products)
            {
                int pricewithdiscount = item.UnitPrice - (item.UnitPrice) * (item.DiscountPercent) / 100;
                if (item.Stock > 0)
                {
                    productList.Add(new
                    {
                        Id = item.Id,
                        Name = item.PersianName,
                        Price = item.UnitPrice,
                        DiscountPrice = pricewithdiscount,
                        Available = true,
                        Unit = "ریال",
                        Image = BaseUrl + item.AppSmallImage ?? ""
                    });
                }
                if (item.Stock == 0)
                {
                    productList.Add(new
                    {
                        Id = item.Id,
                        Name = item.PersianName,
                        Price = item.UnitPrice,
                        DiscountPrice = pricewithdiscount,
                        Available = false,
                        Unit = "ریال",
                        Image = BaseUrl + item.AppSmallImage ?? ""
                    });
                }

            }
            return Ok(new { Status = 1, Object = productList, Count = productList.Count });
            #endregion FindUser
        }

        //Main Api
        [AcceptVerbs("GET")]
        [Route("Categories/GetServices")]
        public IHttpActionResult GetServices(string UserId, string GuId)
        {

            #region Validation
            var Categories = _db.Categories.Where(current => current.SmallImage != null && current.ParentId == null).OrderByDescending(current => current.PersianName).ToList();
            if (Categories.Count == 0)
                return Ok(new { Status = 0, Text = "هنوز دسته بندی اصلی در سیستم وجود ندارد .", Count = 0 });
            #endregion Validation

            #region FindUser
            List<dynamic> Services = new List<dynamic>();
            foreach (var item in Categories)
            {
                Services.Add(new
                {
                    id = item.Id,
                    title = item.PersianName,
                    image = BaseUrl + item.SmallImage ?? "images/pushon_logo.PNG",
                });
            }
            return Ok(new { Status = 1, Text = "", Categories = Services, Count = Services.Count, BasketCount = BasketCount(UserId, GuId) });
            #endregion FindUser
        }

        [AcceptVerbs("GET")]
        [Route("UserInformation/GetUser")]
        public IHttpActionResult UserInformation(string UserId, string GuId)
        {

            #region Validation
            if (string.IsNullOrWhiteSpace(UserId))
                return Ok(new { Status = 0, Text = "لطفا شناسه کاربری را وارد نمایید ." });
            #endregion Validation

            #region FindUser
            User User = _db.Users.Where(current => current.Id == UserId).Include(current => current.City).Include(current => current.State).FirstOrDefault();
            if (User == null)
                return Ok(" کاربری با این شماره یافت نشد . ");
           Domain.Application app = _db.Applications.FirstOrDefault();
            if (User.State != null && User.City != null)
            {
                return Ok(new
                {
                    FirstName = User.FirstName,
                    LastName = User.LastName,
                    ProfileImage = User.ProfileImage,
                    Credit = User.Credit,
                    AccessCode = User.AccessCode,
                    Email = User.Email,
                    IntroCode = User.IntroCode,
                    Phone = User.PhoneNumber,
                    Mobile = User.Mobile,
                    Address = User.AddressLine ?? "",
                    StateId = User.StateId,
                    CityId = User.CityId,
                    GenderId = User.Gender,
                    BithYear = User.BirthDate.Value.Date.Year,
                    BithMonth = User.BirthDate.Value.Date.Month,
                    BithDay = User.BirthDate.Value.Date.Day,
                    Birhtdate = User.BirthDate.Value.Date.Year + "-" + User.BirthDate.Value.Date.Month + "-" + User.BirthDate.Value.Date.Day,
                    PersianBirh = Shared.Helpers.FunctionsHelper.GetPersianDateTime((DateTime)User.BirthDate, false, false),
                    cityName = User.City.Name ?? "",
                    stateName = User.State.Name ?? ""
                });
            }
            else
            {
                return Ok(new
                {
                    FirstName = User.FirstName,
                    LastName = User.LastName,
                    ProfileImage = User.ProfileImage,
                    Credit = User.Credit,
                    AccessCode = User.AccessCode,
                    Email = User.Email,
                    IntroCode = User.IntroCode,
                    Phone = User.PhoneNumber,
                    Mobile = User.Mobile,
                    Address = User.AddressLine,
                    StateId = 1,
                    CityId = 1,
                    GenderId = User.Gender,
                    BithYear = User.BirthDate.Value.Date.Year,
                    BithMonth = User.BirthDate.Value.Date.Month,
                    BithDay = User.BirthDate.Value.Date.Day,
                    Birhtdate = User.BirthDate.Value.Date.Year + "-" + User.BirthDate.Value.Date.Month + "-" + User.BirthDate.Value.Date.Day,
                    PersianBirh = Shared.Helpers.FunctionsHelper.GetPersianDateTime((DateTime)User.BirthDate, false, false),
                    cityName = "",
                    stateName = ""
                });
            }
            #endregion FindUser
        }

        [AcceptVerbs("GET")]
        [Route("Products/Details")]
        public IHttpActionResult ProductDetails(long? ProductId, string UserId, string GuId)
        {

            #region Validation
            if (ProductId == null)
                return Ok(new { Status = 0, Text = "لطفا شناسه محصول را وارد نمایید ." });
            Product product = _db.Products.Where(current => current.Id == ProductId).Include(current => current.Category).Include(current => current.RelatedProducts).Include(current => current.Images).FirstOrDefault();
            if (product == null)
                return Ok(" محصولی با این شماره یافت نشد . ");
            #endregion Validation

            #region FindProduct
            HtmlString htmlString = new HtmlString(product.Description);
            string htmlResult = Regex.Replace(htmlString.ToString(), @"<[^>]*>", string.Empty).Replace("&nbsp;", " ").Replace("&zwnjd", " ").Replace("zwnjd&", " ").Replace("&zwnjd;", " ").Replace("&idquo", " ").Replace("ldquo&", " ").Replace("&ldquo;", " ").Replace(";", "").Replace("&", "").Replace("zwnj", " ").Replace("idquo", " ");
            List<dynamic> ImagesList = new List<dynamic>();
            string EditImage = string.Empty;

            if (product.Images.Count() > 0)
            {
                foreach (var item in product.Images.ToList())
                {
                    EditImage += item.Link.ToString() + ",";
                    ImagesList.Add(new
                    {
                        Id = product.Id,
                        Path = item.Link.ToString() ?? "NULL"
                    });
                }
            }
            else
            {
                ImagesList.Add(new
                {
                    Id = "NULL",
                    Path = "/images/sadr_logo.png",
                });
            }

            List<dynamic> RelatedList = new List<dynamic>();
            foreach (var pr in product.RelatedProducts.ToList())
            {
                RelatedList.Add(new
                {
                    Id = pr.Id.ToString(),
                    PersianName = pr.PersianName,
                    UnitPrice = pr.UnitPrice,
                    PriceWithDiscount = pr.UnitPrice - (pr.UnitPrice) * (pr.DiscountPercent) / 100,
                    Discount = pr.DiscountPercent,
                    Image = BaseUrl + pr.AppSmallImage,
                    Stock = pr.Stock
                });
            }
            return Ok(new
            {
                Status = "1",
                Text = "اطلاعات با موفقیت دریافت شدند .",
                PersianName = product.PersianName,
                EnglishName = product.EnglishName,
                UnitPrice = product.UnitPrice,
                PriceWithDiscount = product.UnitPrice - (product.UnitPrice) * (product.DiscountPercent) / 100,
                Stock = product.Stock,
                DiscountPercent = product.DiscountPercent,
                Description = htmlResult,
                Category = product.Category.PersianName,
                CreateDate = Shared.Helpers.FunctionsHelper.GetPersianDateTime(product.CreateDate, false, false),
                UserImages = ImagesList,
                EditImages = EditImage != "" ? EditImage.Remove(EditImage.Length - 1) : "",
                RelatedCount = product.RelatedProducts.Count(),
                RelatedProduct = RelatedList
            });
            #endregion FindProduct
        }

        [AcceptVerbs("GET")]
        [Route("Server/Connection")]
        public IHttpActionResult Connection(string UserId, string GuId)
        {
            Guid g;
            string guid = string.Empty;
            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(GuId))
            {
                g = Guid.NewGuid();
                guid = g.ToString();
            }
            return Ok(new { status = true, GuId = guid });
        }

        [AcceptVerbs("GET")]
        [Route("Order/MyActivities")]
        public IHttpActionResult GetActivity(string UserId, string GuId)
        {
            #region Validation
            if (string.IsNullOrEmpty(UserId))
                return Ok(new { Status = 0, Text = "لطفا شناسه کاربری را وارد نمایید ." });
            var Orders = _db.Orders.Where(current => current.UserId == UserId).Include(current => current.Products).OrderByDescending(current => current.OrderDate).Take(10).ToList();
            if (Orders.Count == 0)
                return Ok(new { Status = 0, Text = "هنوز سفارشی برای این کاربر وجود ندارد .", OrdersCount = 0 });
            #endregion Validation
            string img = string.Empty;
            #region FindActivities
            List<dynamic> OrdersList = new List<dynamic>();
            foreach (var item in Orders)
            {
                if (item.Products.Count() > 0)
                {
                    Product pro = _db.Products.Find(item.Products.FirstOrDefault().ProductId);
                    if (pro != null)
                    {
                        img = pro.AppSmallImage;
                    }
                }
                OrdersList.Add(new
                {
                    Id = item.Id,
                    DateTime = Shared.Helpers.FunctionsHelper.GetPersianDateTime(item.OrderDate, true, true),
                    Done = item.Done.ToString(),
                    Price = item.TotalPrice ?? "NULL",
                    Cancel = item.IsCanceled.ToString(),
                    ImagePath = "/images/sadr_logo.png",
                    Type = item.getType
                });
            }
            return Ok(new
            {
                Status = 1,
                DoneOrders = Orders.Where(curren => curren.Done == true).Count(),
                CancelOrders = Orders.Where(current => current.IsCanceled == true).Count(),
                AllOrders = Orders.Count(),
                Object = OrdersList,
                OrdersCount = OrdersList.Count
            });
            #endregion FindActivities
        }

        [AcceptVerbs("GET")]
        [Route("Order/Specific")]
        public IHttpActionResult SpecialOrder(int? OrderId, string UserId, string GuId)
        {

            #region Validation
            if (OrderId == null)
                return Ok(new { Status = 0, Text = "لطفا شناسه سفارش را وارد نمایید ." });
            var Orders = _db.Orders.Where(current => current.Id == OrderId).Include(current => current.User).Include(current => current.Transactions).FirstOrDefault();
            if (Orders == null)
                return Ok(new { Status = 0, Text = "سفارش مورد نظر پیدا نشد .", OrdersCount = 0 });
            #endregion Validation

            List<dynamic> OrdersList = new List<dynamic>();

            List<dynamic> ImagesList = new List<dynamic>();

            List<dynamic> Categories = new List<dynamic>();

            string EditImage = string.Empty;

            #region Categories
            var cates = _db.ProductInOrders.Where(current => current.OrderId == OrderId).Include(current => current.Product).ToList();
            foreach (var item in cates)
            {
                Categories.Add(new
                {
                    PersianName = item.Product.PersianName,
                    Count = item.Count
                });
            }
            #endregion Categories

            #region FindUserImages
            var x = _db.ProductInOrders.Where(current => current.OrderId == OrderId).Include(current => current.Product);
            if (x.Count() > 0)
            {
                foreach (var img in x)
                {
                    if (img.Product.AppSmallImage != null)
                    {
                        EditImage += img.Product.AppSmallImage + ",";
                        ImagesList.Add(new
                        {
                            Id = img.ProductId,
                            Path = img.Product.AppSmallImage ?? ""
                        });
                    }
                    else
                    {
                        EditImage += img.Product.AppSmallImage + ",";
                        ImagesList.Add(new
                        {
                            Id = img.ProductId,
                            Path = img.Product.AppSmallImage ?? ""
                        });
                    }
                }
            }
            else
            {
                EditImage += BaseUrl + "/images/sadr_logo.png";
                ImagesList.Add(new
                {
                    Id = "",
                    Path = BaseUrl + "/images/sadr_logo.png"
                });
            }
            #endregion FindUserImages

            string IsPayed = string.Empty;
            string situation = string.Empty;
            string pay = string.Empty;
            string Store = string.Empty;
            if (Orders.Done == true)
                situation = "ثبت موفق";
            else
                situation = "ثبت ناموفق";
            if (Orders.Transactions.Count > 0)
                pay = "پرداخت شده";
            else
                pay = "پرداخت نشده";
            if (Orders.Transactions.Count() > 0)
            {
                IsPayed = "True";
            }
            else
            {
                IsPayed = "False";
            }
            OrdersList.Add(new
            {
                Id = Orders.Id,
                DateTime = Shared.Helpers.FunctionsHelper.GetPersianDateTime(Orders.OrderDate, true, true),
                Category = Categories,
                Done = Orders.Done.ToString(),
                ReceivertName = Orders.getReceiver,
                Type = Orders.getType,
                PaymentType = Orders.getPaymentType,
                InvoiceNumber = Orders.InvoiceNumber ?? "",
                Price = Orders.TotalPrice ?? "",
                UserDescription = Orders.UserOrderDescription,
                FullName = Orders.FullName ?? "",
                Phone = Orders.Phone ?? "",
                UserAddress = Orders.UserAddress ?? "",
                FactorNumber = Orders.FactorNumber,
                Cancel = Orders.IsCanceled.ToString(),
                UserImages = ImagesList,
                EditImages = EditImage != "" ? EditImage.Remove(EditImage.Length - 1) : "",
                UserId = Orders.UserId.ToString() ?? "",
                IsPayed = IsPayed,
                Situation = situation,
                Pay = pay,
                Send = Orders.getSender.ToString()
            });

            #region FindActivities

            return Ok(new { Status = 1, Object = OrdersList, OrdersCount = OrdersList.Count });
            #endregion FindActivities
        }

        void BypassCertificateError()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                delegate (
                    object sender1,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }

        private string GetDate()
        {
            return DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') +
            DateTime.Now.Day.ToString().PadLeft(2, '0');
        }
        private string GetTime()
        {
            return DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0') +
            DateTime.Now.Second.ToString().PadLeft(2, '0');
        }     

        [AcceptVerbs("GET")]
        [Route("Product/GetWithStock")]
        public IHttpActionResult GetWithStock(string q = "")
        {
            try
            {
                var query = _db.Products.Where(p => p.Stock > 0);

                if (!string.IsNullOrEmpty(q))
                {
                    query = query.Where(p => p.PersianName.Contains(q) || p.EnglishName.Contains(q));
                }

                var products = query
                    .OrderBy(p => p.PersianName)
                    .Take(50)
                    .Select(p => new
                    {
                        id = p.SefUrl,         
                        text = p.PersianName
                    })
                    .ToList();

                if (products.Count == 0)
                {
                    return Ok(new
                    {
                        Status = 0,
                        Text = "محصولی یافت نشد."
                    });
                }

                return Ok(new
                {
                    Status = 1,
                    Text = "لیست محصولات دارای موجودی",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Status = 0,
                    Text = "خطایی رخ داده است، لطفا مجدد تلاش کنید.",
                    Error = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("Category/GetSubCategories")]
        public IHttpActionResult GetSubCategories(int categoryId)
        {
            var subs = _db.Categories
                         .Where(c => c.ParentId == categoryId)
                         .OrderBy(c => c.PersianName)
                         .Select(c => new
                         {
                             Id = c.Id,
                             Name = c.PersianName
                         })
                         .ToList();

            if (subs.Count == 0)
                return Ok(new { Status = 0, Text = "زیرشاخه‌ای یافت نشد." });

            return Ok(new { Status = 1, Data = subs });
        }


        private readonly ApplicationDbContext _db;
        const string BaseUrl = "https://motoreto.ir/";
    }
}


