using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Domain;
using Shop.Infrastructure.Data;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>ثبت‌نام، ورود و خروج (با ادغام سبد مهمان).</summary>
public class AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ICartService cart,
    IGuestCartAccessor guestCart,
    ApplicationDbContext db) : Controller
{
    [HttpGet("/account/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/account/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        ViewBag.ReturnUrl = model.ReturnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "حساب شما موقتاً قفل شده است. چند دقیقه بعد تلاش کنید."
                : "ایمیل یا رمز عبور اشتباه است.");
            return View(model);
        }

        // ادغام سبد مهمان در سبد کاربر
        var token = guestCart.GetToken();
        if (!string.IsNullOrEmpty(token))
            await cart.MergeGuestCartAsync(token, user.Id, ct);

        if (Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        // هدایت هر نقش به پنل اختصاصی خودش (ادمین → /manage ، فروشنده → /seller ، کاربر → /panel)
        if (await userManager.IsInRoleAsync(user, "Administrator"))
            return Redirect("/manage");

        var hasShop = await db.Sellers.AnyAsync(s => s.OwnerUserId == user.Id && s.Status == SellerStatus.Approved, ct);
        if (hasShop)
            return Redirect("/seller");

        return Redirect("/panel");
    }

    [HttpGet("/account/register")]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost("/account/register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Mobile = model.Mobile,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code, error.Description));

            return View(model);
        }

        await userManager.AddToRoleAsync(user, "User");
        await signInManager.SignInAsync(user, isPersistent: false);

        var guestToken = guestCart.GetToken();
        if (!string.IsNullOrEmpty(guestToken))
            await cart.MergeGuestCartAsync(guestToken, user.Id, ct);

        // ---- CPA: پاداش ثبت‌نام از طریق لینک همکار فروش ----
        var affCode = Request.Cookies["aff"];
        if (!string.IsNullOrWhiteSpace(affCode))
        {
            var aff = await db.Affiliates.FirstOrDefaultAsync(a => a.Code == affCode && a.IsActive, ct);
            if (aff is not null)
            {
                const int cpaBonus = 20000; // پاداش هر ثبت‌نام موفق (تومان)
                aff.RegistrationCount++;
                aff.Earned += cpaBonus;
                await db.SaveChangesAsync(ct);
            }
        }

        TempData["Success"] = $"خوش آمدید {user.FirstName} جان! 🎉";
        return Redirect("/panel");
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/account/denied")]
    public IActionResult Denied() => View();

    // ---- پنل کاربر → به Blazor منتقل شد (Components/Account/Pages) ----

    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;

    private static string TranslateIdentityError(string code, string fallback) => code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "این ایمیل قبلاً ثبت شده است.",
        "PasswordTooShort" => "رمز عبور خیلی کوتاه است.",
        "InvalidEmail" => "ایمیل معتبر نیست.",
        _ => fallback
    };
}
