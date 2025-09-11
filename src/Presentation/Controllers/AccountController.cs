using Domain;
using Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using System.Data.Entity;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs.ViewModels;

namespace Presentation.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController(
                   ApplicationDbContext db,
                   RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager,
                   UserManager<User> userManager,
                   SignInManager<User> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public bool DeleteImages(int id)
        {
            var list = _db.Images.Find(id);
            if (list != null)
            {
                try
                {
                    _db.Images.Remove(list);
                    _db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public bool DeleteFiles(int id)
        {
            var list = _db.Files.Find(id);
            if (list != null)
            {
                try
                {
                    _db.Files.Remove(list);
                    _db.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }


        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult AdminPanel()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult UserAccount()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RegisterAccount()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LoginAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LoginUser(string PhoneNumber)
        {

            #region Validation
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                TempData["Error"] = "لطفا شماره موبایل خود را وارد نمایید .";
                return RedirectToAction("LoginAccount");
            }

            if (!Regex.Match(PhoneNumber, @"\b\d{4}[\s-.]?\d{3}[\s-.]?\d{4}\b").Success)
            {
                TempData["Error"] = "لطفا شماره موبایل معتبری را وارد نمایید .";
                return RedirectToAction("LoginAccount");
            }

            var users = _db.Users.Where(u => u.Mobile == PhoneNumber);
            if (users != null)
            {
                var inRole = _userManager.IsInRoleAsync(users.FirstOrDefault(), "User");
            }

            if (users.Count() <= 0)
            {
                TempData["Error"] = "شماره شما در سیستم ثبت نشده است ، لطفا ابتدا ثبت نام کنید .";
                return RedirectToAction("LoginAccount");
            }

            if (users.Count() > 0 && users.FirstOrDefault().IsActive == false)
            {
                TempData["Error"] = "حساب کاربری شما غیر فعال شده است لطفا با پشتیبانی تماس حاصل فرمایید .";
                return RedirectToAction("LoginAccount");
            }
            #endregion Validation

            #region SMS
            Domain.Application application = _db.Applications.FirstOrDefault();
            Random random = new Random();
            var randomNumber = random.Next(10000, 99999);
            if (application != null && application.FromNumber != null)
            {

                string smsCode = SendOtpMelipayamak(PhoneNumber);
                if (string.IsNullOrEmpty(smsCode))
                {
                    TempData["Error"] = "ارسال پیامک تایید با خطا مواجه شد.";
                    return RedirectToAction("LoginAccount");
                }

                #region EditUserCode
                users.FirstOrDefault().VerificationCode = Shared.Helpers.FunctionsHelper.Encrypt(smsCode, "Motoreto");
                _db.SaveChanges();
                #endregion EditUserCodde

            }
            else
            {
                TempData["Error"] = "تنظیمات کلی اپلیکیشن هنوز تعیین نشده است .";
                return RedirectToAction("LoginAccount");
            }
            #endregion SMS

            TempData["Success"] = "به حساب کاربری خود خوش آمدید .";
            return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Approve(string PhoneNumber, string ReturnUrl)
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                return Forbid();
            return View();
        }

        public int CartCount()
        {
            // همه کوکی‌های ورودی
            var cookies = Request.Cookies;

            // فقط کوکی‌هایی که با Cart_ شروع میشن
            var cartCookies = cookies.Where(c => c.Key.StartsWith("Cart_")).ToList();

            return cartCookies.Count;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ApproveNumber(string PhoneNumber, string Code, string ReturnUrl)
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                TempData["Error"] = "لطفا شماره موبایل خود را وارد نمایید .";
                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
            }
            if (string.IsNullOrWhiteSpace(Code))
            {
                TempData["Error"] = "لطفا کد را وارد نمایید .";
                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
            }
            if (!Regex.Match(PhoneNumber, @"\b\d{4}[\s-.]?\d{3}[\s-.]?\d{4}\b").Success)
            {
                TempData["Error"] = "لطفا شماره موبایل معتبری را وارد نمایید .";
                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
            }

            var users = _db.Users.Where(u => u.Mobile == PhoneNumber);
            if (users != null)
            {
                var inRole = _userManager.IsInRoleAsync(users.FirstOrDefault(), "User");
            }

            if (users.Count() <= 0)
            {
                TempData["Error"] = "شماره شما در سیستم ثبت نشده است .";
                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
            }
            #endregion Validation

            #region Login
            string UserVerificationCode = Shared.Helpers.FunctionsHelper.Decrypt(users.FirstOrDefault().VerificationCode, "Motoreto");
            if (UserVerificationCode != null)
            {
                if (UserVerificationCode != Code)
                {
                    TempData["Error"] = "کد وارد شده صحیح نمیباشد .";
                    return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
                }
                if (UserVerificationCode == Code)
                {
                    users.FirstOrDefault().PhoneNumberConfirmed = true;
                    _db.SaveChanges();
                    Domain.Application application = _db.Applications.FirstOrDefault();
                    if (application != null && application.VerifyPhoneNumberText != null)
                    {
                        Microsoft.AspNetCore.Html.HtmlString htmlString = new Microsoft.AspNetCore.Html.HtmlString(application.VerifyPhoneNumberText);
                        string htmlResult = Regex.Replace(htmlString.ToString(), @"<[^>]*>", string.Empty).Replace("\r", "").Replace("\n", "").Replace("&nbsp", "");
                        {
                            TempData["Success"] = htmlResult;
                            var result = _signInManager.PasswordSignInAsync(
                                          users.FirstOrDefault().UserName,   // username
                                          PhoneNumber,                       // password (اینجا باید رمز واقعی باشه نه شماره موبایل!)
                                          false,                             // isPersistent
                                          lockoutOnFailure: false
  );

                            if (result.IsCompletedSuccessfully)
                            {
                                if (CartCount() > 0)
                                {
                                    return RedirectToAction("Cart", "Home");
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            else if (result.IsFaulted)
                            {
                                TempData["Error"] = "نام کاربری یا رمز عبور اشتباه است.";
                                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
                            }
                        }
                    }
                    else
                    {
                        TempData["Success"] = "شماره شما با موفقیت تایید شد .";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            else
            {
                TempData["Error"] = "کد کاربر خالی است .";
                return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
            }
            #endregion Login

            TempData["Error"] = "لطفا مجدد تلاش فرمایید .";
            return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResendVerificationCode(string PhoneNumber)
        {

            #region Validation
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                TempData["Error"] = "لطفا شماره موبایل خود را وارد نمایید .";
                return RedirectToAction("LoginAccount");
            }

            if (!Regex.Match(PhoneNumber, @"\b\d{4}[\s-.]?\d{3}[\s-.]?\d{4}\b").Success)
            {
                TempData["Error"] = "لطفا شماره موبایل معتبری را وارد نمایید .";
                return RedirectToAction("LoginAccount");
            }

            var users = _db.Users.Where(u => u.Mobile == PhoneNumber);
            if (users != null)
            {
                var inRole = _userManager.IsInRoleAsync(users.FirstOrDefault(), "User");
            }

            if (users.Count() <= 0)
            {
                TempData["Error"] = "شماره شما در سیستم ثبت نشده است ، لطفا ابتدا ثبت نام کنید .";
                return RedirectToAction("LoginAccount");
            }

            if (users.Count() > 0 && users.FirstOrDefault().IsActive == false)
            {
                TempData["Error"] = "حساب کاربری شما غیر فعال شده است لطفا با پشتیبانی تماس حاصل فرمایید .";
                return RedirectToAction("LoginAccount");
            }
            #endregion Validation

            #region SMS
            Domain.Application application = _db.Applications.FirstOrDefault();
            Random random = new Random();
            var randomNumber = random.Next(10000, 99999);
            if (application != null && application.FromNumber != null)
            {

                string smsCode = SendOtpMelipayamak(PhoneNumber);
                if (string.IsNullOrEmpty(smsCode))
                {
                    TempData["Error"] = "ارسال پیامک تایید با خطا مواجه شد.";
                    return RedirectToAction("LoginAccount");
                }

                #region EditUserCode
                users.FirstOrDefault().VerificationCode = Shared.Helpers.FunctionsHelper.Encrypt(smsCode, "Motoreto");
                _db.SaveChanges();
                #endregion EditUserCodde

            }
            else
            {
                TempData["Error"] = "تنظیمات کلی اپلیکیشن هنوز تعیین نشده است .";
                return RedirectToAction("LoginAccount");
            }
            #endregion SMS
            TempData["Success"] = "به حساب کاربری خود خوش آمدید .";
            return RedirectToAction("Approve", new { PhoneNumber, ReturnUrl = "Account/LoginAccount" });
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            ).Result;   // 👈 چون async نمیخوای

            if (result.Succeeded)
            {
                // نقش‌ها
                var adminRole = _roleManager.FindByNameAsync("Administrator").Result;
                var operatorRole = _roleManager.FindByNameAsync("Operator").Result;

                    var user = _db.Users
                                      .Include(u => u.Roles) // 👈 در Core به این شکله
                                      .FirstOrDefault(u => u.UserName == model.Email);

                    if (user != null)
                    {
                        if (adminRole != null && user.Roles.Any(r => r.RoleId == adminRole.Id))
                            return RedirectToAction("AdminPanel", "Account");

                        if (operatorRole != null && user.Roles.Any(r => r.RoleId == operatorRole.Id))
                            return RedirectToAction("OperatorPanel", "Account");
                    }

                return RedirectToAction("Index", "Home");
            }
            else if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
            }
            else
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = "Admin",
                    LastName = "Admin",
                    RegistrationDate = DateTime.Now
                };

                // ایجاد کاربر
                var result = _userManager.CreateAsync(user, model.Password).Result;

                if (result.Succeeded)
                {
                    // ورود کاربر بدون نیاز به rememberBrowser
                    _signInManager.SignInAsync(user, isPersistent: false).Wait();

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // اگر خطا بود دوباره فرم رو نشون بده
            return View(model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GenerateOrLogin(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return Json(new { success = false, message = "شماره وارد نشده است" });

            phoneNumber = Regex.Replace(phoneNumber, @"[^\d]", "");
            if (!Regex.IsMatch(phoneNumber, @"^09\d{9}$"))
                return Json(new { success = false, message = "شماره موبایل معتبر نیست" });

            var user = _db.Users.FirstOrDefault(u => u.Mobile == phoneNumber);

            // کد یکبارمصرف
            string otpCode = SendOtpMelipayamak(phoneNumber);

            if (string.IsNullOrEmpty(otpCode))
                return Json(new { success = false, message = "ارسال پیامک با خطا مواجه شد" });

            if (user == null)
            {
                // اگر کاربر وجود نداشت بساز
                var security = Guid.NewGuid().ToString("N");
                var hashedPassword = new PasswordHasher<User>().HashPassword(null, "Temp@" + otpCode);

                var defaultCity = _db.Cities.FirstOrDefault();
                var defaultState = _db.States.FirstOrDefault();

                user = new User
                {
                    Mobile = phoneNumber,
                    UserName = phoneNumber,
                    UserScore = 0,
                    AccessCode = "Motoreto_" + otpCode,
                    Credit = 0,
                    IsActive = true,
                    BirthDate = DateTime.Now,
                    CityId = defaultCity?.Id ?? 1,
                    StateId = defaultState?.Id ?? 1,
                    SecurityStamp = security,
                    PasswordHash = hashedPassword,
                    VerificationCode = Shared.Helpers.FunctionsHelper.Encrypt(otpCode, "Motoreto"),
                    PhoneNumberConfirmed = false,
                    RegistrationDate = DateTime.Now,
                    FirstName = "",
                    LastName = ""
                };

                var result = _userManager.CreateAsync(user).Result;
                if (result.Succeeded)
                    _userManager.AddToRoleAsync(user, "User").Wait();
            }
            else
            {
                // اگر بود، فقط کد جدید بده
                user.VerificationCode = Shared.Helpers.FunctionsHelper.Encrypt(otpCode, "Motoreto");
                _db.Users.Update(user);
            }

            _db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ApproveQuick(string phoneNumber, string code)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "شماره یا کد نامعتبر است" });

            phoneNumber = Regex.Replace(phoneNumber, @"[^\d]", "");
            var user = _db.Users.FirstOrDefault(u => u.Mobile == phoneNumber);

            if (user == null)
                return Json(new { success = false, message = "کاربر یافت نشد" });

            var decryptedCode = Shared.Helpers.FunctionsHelper.Decrypt(user.VerificationCode, "Motoreto");
            if (decryptedCode != code)
                return Json(new { success = false, message = "کد اشتباه است" });

            // ✅ تایید شماره
            user.PhoneNumberConfirmed = true;
            _db.Users.Update(user);
            _db.SaveChanges();

            // ✅ ورود کاربر
            _signInManager.SignInAsync(user, isPersistent: false).Wait();

            return Json(new { success = true });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult IsAuthenticated()
        {
            return Json(new { isAuth = User.Identity.IsAuthenticated });
        }

        public static string SendOtpMelipayamak(string phoneNumber)
        {
            try
            {
                Uri apiBaseAddress = new Uri("https://console.melipayamak.com");

                using (HttpClient client = new HttpClient() { BaseAddress = apiBaseAddress })
                {
                    // توجه: باید Microsoft.AspNet.WebApi.Client نصب باشه
                    var result = client.PostAsJsonAsync(
                        "api/send/otp/896d2b4ac27f48c9a42183ce5f482_db6",
                        new { to = phoneNumber }).Result;

                    var response = result.Content.ReadAsStringAsync().Result;

                    // پارس JSON
                    dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                    if (obj != null && obj.code != null)
                    {
                        // یعنی ارسال موفق بوده و code برگشته
                        return obj.code.ToString();
                    }
                    else
                    {
                        return null; // ارسال ناموفق
                    }
                }
            }
            catch (Exception ex)
            {
                // لاگ خطا
                return null;
            }
        }


        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> _roleManager;
    }
}