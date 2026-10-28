using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>سفارش‌های کاربر + صفحه نتیجه پرداخت.</summary>
[Authorize]
public class OrderController(IOrderService orders, IStockReservationService reservations) : UserControllerBase
{
    [HttpGet("/orders")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await orders.GetUserOrdersAsync(CurrentUserId(), page, 10, ct);
        return View(result);
    }

    [HttpGet("/orders/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await orders.GetOrderAsync(id, CurrentUserId(), ct);
        if (order is null) return NotFound();
        return View(order);
    }

    /// <summary>صفحه نتیجه پرداخت (موفق/ناموفق).</summary>
    [HttpGet("/orders/{id:int}/result")]
    public async Task<IActionResult> Result(int id, bool failed = false, CancellationToken ct = default)
    {
        var order = await orders.GetOrderAsync(id, CurrentUserId(), ct);
        if (order is null) return NotFound();

        ViewBag.Failed = failed;
        return View(order);
    }

    /// <summary>لغو سفارش توسط کاربر (بازگشت خودکار موجودی).</summary>
    [HttpPost("/orders/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason, CancellationToken ct)
    {
        var ok = await orders.CancelAsync(id, CurrentUserId(),
            string.IsNullOrWhiteSpace(reason) ? "لغو توسط کاربر" : reason.Trim(), ct);

        TempData[ok ? "Success" : "Error"] = ok
            ? "سفارش شما لغو شد."
            : "امکان لغو این سفارش وجود ندارد.";

        return RedirectToAction(nameof(Details), new { id });
    }
}

/// <summary>پایه کنترلرهای کاربر عضو (استخراج UserId یکجا — DRY).</summary>
public abstract class UserControllerBase : ShopControllerBase
{
    protected Guid CurrentUserId() => UserId;
}
