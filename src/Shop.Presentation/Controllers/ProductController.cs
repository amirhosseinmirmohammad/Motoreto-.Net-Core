using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Shop.Presentation.Services;
using Shop.Shared;
using Shop.Shared;

namespace Shop.Presentation.Controllers;

/// <summary>لیست و جزئیات محصولات.</summary>
public class ProductController(
    IProductService products,
    ICategoryService categories,
    ISettingsService settings,
    ICommentService comments,
    IFavoriteService favorites,
    IGuestCartAccessor guestCart,
    ApplicationDbContext db) : Controller
{
    // لیست محصولات «/products» به Blazor منتقل شد → Components/Pages/ProductList.razor (SPA)

    [HttpGet("/p/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var product = await products.GetBySlugAsync(slug, ct);
        if (product is null) return NotFound();

        // واریانت‌های فعال (SKU) برای انتخاب رنگ/سایز/حافظه و...
        var variants = await db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == product.Card.Id && v.IsActive)
            .OrderBy(v => v.Id)
            .Select(v => new
            {
                v.Id,
                v.AttributesJson,
                Available = v.Stock - v.ReservedCount,
                FinalPrice = v.Price > 0
                    ? (v.DiscountPercent > 0 ? v.Price - v.Price * v.DiscountPercent / 100 : v.Price)
                    : (product.Card.FinalPrice)
            })
            .ToListAsync(ct);

        ViewBag.HasVariants = variants.Count > 0;
        ViewBag.VariantsJson = System.Text.Json.JsonSerializer.Serialize(variants);

        // ---- سئو / AEO / GEO ----
        var siteUrl = $"{Request.Scheme}://{Request.Host}";
        var plainDesc = System.Text.RegularExpressions.Regex.Replace(product.Description ?? "", "<.*?>", " ").Trim();
        if (plainDesc.Length > 160) plainDesc = plainDesc[..157] + "...";
        var card = product.Card;

        ViewData["SeoTitle"] = (string?)product.MetaTitle ?? $"{card.Name} | خرید و قیمت | مارکت";
        ViewData["SeoDescription"] = (string?)product.MetaDescription ?? plainDesc;
        ViewData["SeoUrl"] = $"{siteUrl}/p/{card.Slug}";
        ViewData["SeoImage"] = card.Image.StartsWith("http") ? card.Image : siteUrl + card.Image;

        // Sanitize: توضیحات HTML را برای نمایش امن پاک‌سازی می‌کنیم (ضد XSS)
        var safeDescription = (product.Description ?? "").Sanitize();
        ViewData["SeoJsonLd"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Product",
            ["name"] = card.Name,
            ["description"] = (string?)product.MetaDescription ?? plainDesc,
            ["image"] = card.Image.StartsWith("http") ? card.Image : siteUrl + card.Image,
            ["sku"] = product.Code,
            ["url"] = $"{siteUrl}/p/{card.Slug}",
            ["brand"] = new Dictionary<string, object?> { ["@type"] = "Brand", ["name"] = card.BrandName ?? "مارکت" },
            ["offers"] = new Dictionary<string, object?>
            {
                ["@type"] = "Offer",
                ["url"] = $"{siteUrl}/p/{card.Slug}",
                // قیمت در اسکیمای استاندارد به ریال (ISO 4217: IRR) — نمایش کاربر تومان است
                ["price"] = card.FinalPrice * 10,
                ["priceCurrency"] = "IRR",
                ["availability"] = card.Stock > 0 ? "https://schema.org/InStock" : "https://schema.org/OutOfStock",
                ["itemCondition"] = "https://schema.org/NewCondition"
            },
            ["aggregateRating"] = card.CommentCount > 0
                ? new Dictionary<string, object?> { ["@type"] = "AggregateRating", ["ratingValue"] = Math.Round(card.AverageRating, 1), ["reviewCount"] = card.CommentCount }
                : null
        }.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value!));

        // افزایش بازدید (آپدیت اتمیک سبک — DbContext thread-safe نیست، پس await می‌کنیم)
        await products.IncrementViewAsync(product.Card.Id, ct);

        var config = await settings.GetAsync(ct);
        var (avgRating, commentCount) = await comments.GetRatingSummaryAsync(product.Card.Id, ct);

        var related = await products.GetRelatedAsync(product.Card.Id, product.CategoryId, 12, ct);

        var model = new ProductDetailPageViewModel
        {
            Product = product,
            Related = related,
            Comments = await comments.GetApprovedAsync(product.Card.Id, ct),
            AverageRating = avgRating,
            CommentCount = commentCount,
            IsFavorite = User.Identity?.IsAuthenticated == true &&
                         await favorites.IsFavoriteAsync(CurrentUserId(), product.Card.Id, ct),
            LowStockThreshold = config.LowStockThreshold,
            MaxQuantityPerProduct = config.MaxQuantityPerProduct
        };

        return View(model);
    }

    /// <summary>پیشنهاد جستجو (AJAX).</summary>
    [HttpGet("/api/search/suggest")]
    [AllowAnonymous]
    public async Task<IActionResult> Suggest(string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var result = await products.GetFilteredAsync(new ProductFilter
        {
            Q = q,
            PageSize = 6,
            Page = 1
        }, ct);

        return Json(result.Items.Select(p => new
        {
            name = p.Name,
            url = $"/p/{p.Slug}",
            image = p.Image,
            price = p.FinalPrice
        }));
    }

    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;
}

/// <summary>کوئری ورودی لیست محصولات (model binding از query string).</summary>
public class ProductListQuery
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    public string? Sort { get; set; }
    public long? Min { get; set; }
    public long? Max { get; set; }
    public bool Available { get; set; }
    public bool Discount { get; set; }
    public int Page { get; set; } = 1;
}

public class ProductDetailPageViewModel
{
    public ProductDetailDto Product { get; set; } = default!;
    public List<ProductCardDto> Related { get; set; } = [];
    public List<ProductCommentDto> Comments { get; set; } = [];
    public double AverageRating { get; set; }
    public int CommentCount { get; set; }
    public bool IsFavorite { get; set; }
    public int LowStockThreshold { get; set; }
    public int MaxQuantityPerProduct { get; set; }
}
