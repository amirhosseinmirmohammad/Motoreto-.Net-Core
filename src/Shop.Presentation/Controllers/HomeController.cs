using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Presentation.Services;

namespace Shop.Presentation.Controllers;

/// <summary>صفحه اصلی فروشگاه.</summary>
public class HomeController(
    IProductService products,
    ICategoryService categories,
    ISliderService sliders,
    IBlogService blogs,
    ISettingsService settings,
    ICartService cart,
    IGuestCartAccessor guestCart) : ShopControllerBase
{
    // صفحه اصلی «/» به Blazor منتقل شد → Components/Pages/Home.razor (SPA)

    [HttpGet("/404")]
    public IActionResult NotFoundPage() => View("NotFound");

    /// <summary>صفحات خطا (۴۰۴/۵۰۰ و...) — با StatusCodePages ReExecute فراخوانی می‌شود.</summary>
    [HttpGet("/home/status/{code:int}")]
    public IActionResult Status(int code)
    {
        HttpContext.Items["Code"] = code.ToString();
        Response.StatusCode = code;
        return View("Status");
    }

    [HttpGet("/error")]
    public IActionResult Error() => View();

    /// <summary>تعداد اقلام سبد برای badge هدر (AJAX — قابل کش شدن صفحات).</summary>
    [HttpGet("/api/cart/count")]
    [AllowAnonymous]
    public async Task<IActionResult> CartCount(CancellationToken ct)
    {
        var summary = await ResolveCartAsync(ct);
        return Json(new { count = summary.TotalCount });
    }

    /// <summary>
    /// توکن تازه Antiforgery برای صفحات کش‌شده (OutputCache) —
    /// توکن داخل HTML کش ممکن است برای بازدیدکننده جدید معتبر نباشد.
    /// </summary>
    [HttpGet("/api/antiforgery/token")]
    [IgnoreAntiforgeryToken]
    public JsonResult AntiforgeryToken()
    {
        var tokens = HttpContext.RequestServices
            .GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
            .GetAndStoreTokens(HttpContext);

        return Json(new { token = tokens.RequestToken });
    }

    [AllowAnonymous]
    [HttpGet("/maintenance")]
    public async Task<IActionResult> Maintenance(CancellationToken ct)
    {
        var config = await settings.GetAsync(ct);
        return View(config);
    }

    private async Task<CartSummaryDto> ResolveCartAsync(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
            return await cart.GetSummaryAsync(UserId, null, ct);

        var token = guestCart.GetToken();
        return await cart.GetSummaryAsync(null, token, ct);
    }
}

/// <summary>مدل صفحه اصلی.</summary>
public class HomeViewModel
{
    public string StoreName { get; set; } = default!;
    public List<Slider> Sliders { get; set; } = [];
    public List<CategoryNodeDto> Categories { get; set; } = [];
    public List<ProductCardDto> AmazingProducts { get; set; } = [];
    public DateTime? AmazingEndsAt { get; set; }
    public List<ProductCardDto> SpecialProducts { get; set; } = [];
    public List<ProductCardDto> NewestProducts { get; set; } = [];
    public List<ProductCardDto> BestSellers { get; set; } = [];
    public List<BlogPostDto> LatestPosts { get; set; } = [];
}
