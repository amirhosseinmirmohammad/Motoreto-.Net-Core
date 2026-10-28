using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Presentation.Services;

namespace Shop.Presentation.Controllers;

/// <summary>API سبد خرید (JSON) — پشتیبانی از آفر فروشنده و تنوع محصول.</summary>
public class CartController(
    ICartService cart,
    IGuestCartAccessor guestCart) : Controller
{
    [HttpGet("/cart")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var summary = await ResolveCartAsync(ct);
        return View(summary);
    }

    [HttpPost("/api/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromBody] CartRequest? request, CancellationToken ct)
    {
        if (request is null || request.ProductId < 1)
            return Json(new CartOperationResult { Success = false, Message = "درخواست نامعتبر است." });

        var result = await cart.AddItemAsync(
            await ResolveUserIdOrNullAsync(), GuestTokenForWrite(),
            request.ProductId, request.OfferId, request.VariantId, request.Quantity, ct);
        return Json(result);
    }

    [HttpPost("/api/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromBody] CartRequest? request, CancellationToken ct)
    {
        if (request is null || request.ProductId < 1)
            return Json(new CartOperationResult { Success = false, Message = "درخواست نامعتبر است." });

        var result = await cart.UpdateQuantityAsync(
            await ResolveUserIdOrNullAsync(), GuestTokenForWrite(),
            request.ProductId, request.OfferId, request.VariantId, request.Quantity, ct);
        return Json(result);
    }

    [HttpPost("/api/cart/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove([FromBody] CartRequest? request, CancellationToken ct)
    {
        if (request is null || request.ProductId < 1)
            return Json(new CartOperationResult { Success = false, Message = "درخواست نامعتبر است." });

        var result = await cart.RemoveItemAsync(
            await ResolveUserIdOrNullAsync(), GuestTokenForWrite(),
            request.ProductId, request.OfferId, request.VariantId, ct);
        return Json(result);
    }

    private async Task<Guid?> ResolveUserIdOrNullAsync() =>
        User.Identity?.IsAuthenticated == true
            ? Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null
            : null;

    /// <summary>برای نوشتن مهمان، توکن لازم است — در صورت نبود ساخته می‌شود.</summary>
    private string? GuestTokenForWrite() =>
        User.Identity?.IsAuthenticated == true ? null : guestCart.GetOrCreateToken();

    private async Task<CartSummaryDto> ResolveCartAsync(CancellationToken ct) =>
        await cart.GetSummaryAsync(await ResolveUserIdOrNullAsync(), guestCart.GetToken(), ct);
}

public class CartRequest
{
    public long ProductId { get; set; }

    /// <summary>خرید از فروشنده خاص (مارکت‌پلیس).</summary>
    public long? OfferId { get; set; }

    /// <summary>خرید با ترکیب ویژگی‌های خاص (رنگ/سایز و...)</summary>
    public long? VariantId { get; set; }

    public int Quantity { get; set; } = 1;
}
