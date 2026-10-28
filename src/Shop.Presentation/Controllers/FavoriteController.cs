using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;

namespace Shop.Presentation;

/// <summary>علاقه‌مندی‌های کاربر (صفحه).</summary>
[Authorize]
public class FavoriteController(IFavoriteService favorites) : ShopControllerBase
{
    [HttpGet("/favorites")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await favorites.GetUserFavoritesAsync(UserId, ct);
        return View(items);
    }
}
