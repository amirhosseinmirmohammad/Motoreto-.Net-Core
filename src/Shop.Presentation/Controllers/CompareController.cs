using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>مقایسه محصولات: تا ۴ کالا کنار هم + نمودار مقایسه قیمت.</summary>
public class CompareController(
    ICompareService compare,
    IGuestCartAccessor guestCart) : Shop.Presentation.Controllers.ShopControllerBase
{
    private const string CookieName = "shop.compare";

    [HttpPost("/api/compare/toggle")]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle([FromBody] CompareRequest request)
    {
        var current = Request.Cookies[CookieName];
        var (added, message) = compare.Toggle(current, request.ProductId, out var newIds);

        Response.Cookies.Append(CookieName, newIds, new CookieOptions
        {
            HttpOnly = false,
            IsEssential = false,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(7)
        });

        var count = newIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        return Json(new { added, message, count });
    }

    [HttpGet("/compare")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var ids = ParseIds(Request.Cookies[CookieName]);
        var cards = await compare.GetCardsAsync([.. ids], ct);

        ViewBag.CompareCount = cards.Count;
        return View(cards);
    }

    public static List<long> ParseIds(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Select(s => long.TryParse(s, out var id) ? id : 0)
                 .Where(id => id > 0)
                 .Distinct()
                 .ToList();
}

public class CompareRequest
{
    public long ProductId { get; set; }
}
