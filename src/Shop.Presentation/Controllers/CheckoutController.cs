using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>
/// فرایند تسویه: شروع → رزرو موجودی با تایمر → ثبت سفارش → انتقال به درگاه.
/// </summary>
[Authorize]
public class CheckoutController(
    IStockReservationService reservations,
    IOrderService orders,
    ICartService cart,
    IDiscountService discounts,
    ISettingsService settings,
    ApplicationDbContext db,
    IGuestCartAccessor guestCart) : Controller
{
    /// <summary>
    /// ورود به تسویه: رزرو اتمیک موجودی اقلام سبد برای مدت مشخص (قابل تنظیم در ادمین).
    /// GET هم پذیرفته می‌شود تا URL دستی به ۴۰۴ نخورد.
    /// </summary>
    [HttpGet("/checkout/start")]
    public IActionResult StartViaGet() => RedirectToAction(nameof(Index));

    [HttpPost("/checkout/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(CancellationToken ct)
    {
        var result = await reservations.ReserveForCheckoutAsync(CurrentUserId(), ct);

        if (!result.Success)
        {
            TempData["CheckoutError"] = result.Message;
            return RedirectToAction(nameof(CartController.Index), "Cart");
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>صفحه تسویه با شمارش معکوس رزرو.</summary>
    [HttpGet("/checkout")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var active = await reservations.GetActiveAsync(CurrentUserId(), ct);

        if (active is null)
        {
            // بدون رزرو فعال → شروع رزرو
            var started = await reservations.ReserveForCheckoutAsync(CurrentUserId(), ct);
            if (!started.Success)
            {
                TempData["CheckoutError"] = started.Message;
                return RedirectToAction(nameof(CartController.Index), "Cart");
            }
            active = started;
        }

        var config = await settings.GetAsync(ct);
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == CurrentUserId())
            .Select(u => new { u.FirstName, u.LastName, u.Mobile, u.StateId, u.CityId, u.AddressLine, u.PostalCode })
            .FirstAsync(ct);

        ViewBag.States = await db.States.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);
        ViewBag.Cities = user.StateId is null
            ? []
            : await db.Cities.AsNoTracking().Where(c => c.StateId == user.StateId).OrderBy(c => c.Name).ToListAsync(ct);

        ViewBag.Settings = config;
        ViewBag.UserDefaults = user;

        // روش‌های پرداخت پذیرش‌شده توسط همه فروشندگان سبد + تعداد مرسوله‌ها (ارسال چندفروشنده‌ای)
        var uid = CurrentUserId();
        var now = DateTime.UtcNow;
        var resQ = db.StockReservations.AsNoTracking()
            .Where(r => r.UserId == uid && r.Status == Shop.Domain.ReservationStatus.Active && r.ExpiresAt > now);

        var sellerIds = await resQ.Where(r => r.Offer != null).Select(r => r.Offer!.SellerId).Distinct().ToListAsync(ct);
        var hasMarketItems = await resQ.AnyAsync(r => r.Offer == null, ct);
        var sellerPms = await db.Sellers.AsNoTracking()
            .Where(s => sellerIds.Contains(s.Id))
            .Select(s => s.PaymentMethods).ToListAsync(ct);

        ViewBag.AcceptOnline = sellerPms.All(pm => (pm ?? "online").Split(',').Contains("online"));
        ViewBag.AcceptCod = sellerPms.All(pm => (pm ?? "online").Split(',').Contains("cod"));
        ViewBag.ShipmentCount = sellerIds.Count + (hasMarketItems ? 1 : 0);

        return View(active);
    }

    /// <summary>ثبت نهایی سفارش و انتقال به درگاه پرداخت.</summary>
    [HttpPost("/checkout/place")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(CheckoutViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            TempData["CheckoutError"] = string.Join(" ", errors);
            return RedirectToAction(nameof(Index));
        }

        var result = await orders.PlaceOrderAsync(CurrentUserId(), model, ct);

        if (!result.Success)
        {
            TempData["CheckoutError"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // ---- اتریبیوشن افیلیت: کوکی ۳۰ روزه → کمیسیون همکار ----
        var affCode = Request.Cookies["aff"];
        if (!string.IsNullOrWhiteSpace(affCode))
        {
            var aff = await db.Affiliates.FirstOrDefaultAsync(a => a.Code == affCode && a.IsActive, ct);
            var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == result.OrderId, ct);

            // ضدتقلب: خریدِ خودِ همکار کمیسیون ندارد + سود مارکت حفظ شود (کمیسیون همکار ≤ کمیسیون فروشنده به‌صورت پیش‌فرض ۵٪ است)
            if (aff is not null && order is not null && aff.UserId != CurrentUserId() && order.Total > 0)
            {
                order.AffiliateCode = aff.Code;
                aff.OrderCount++;
                aff.OrderAmount += order.Total;
                aff.Earned += order.Total * aff.CommissionPercent / 100;
                await db.SaveChangesAsync(ct);
            }
        }

        if (model.PaymentMethod == (int)PaymentMethod.Online)
        {
            // انتقال به درگاه
            return RedirectToAction(nameof(PaymentController.Pay), "Payment", new { orderId = result.OrderId });
        }

        // پرداخت در محل → مستقیم موفق (بدون درگاه)
        await orders.MarkPaidAsync(result.OrderId!.Value, "COD", ct);
        await reservations.ReleaseUserReservationsAsync(CurrentUserId(), ct);
        return RedirectToAction(nameof(OrderController.Result), "Order", new { id = result.OrderId });
    }

    /// <summary>لیست شهرهای استان (AJAX cascade).</summary>
    [HttpGet("/api/locations/cities/{stateId}")]
    public async Task<IActionResult> Cities(int stateId, CancellationToken ct)
    {
        var cities = await db.Cities
            .AsNoTracking()
            .Where(c => c.StateId == stateId)
            .OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name })
            .ToListAsync(ct);

        return Json(cities);
    }

    /// <summary>بررسی اعتبار کد تخفیف (AJAX).</summary>
    [HttpPost("/api/discount/check")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckDiscount([FromBody] DiscountCheckRequest request, CancellationToken ct)
    {
        var active = await reservations.GetActiveAsync(CurrentUserId(), ct);
        if (active is null)
            return Json(new DiscountValidationResult { Success = false, Message = "سبد خرید شما خالی است." });

        var result = await discounts.ValidateAsync(request.Code, active.Subtotal, ct);
        return Json(result);
    }

    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;
}

public class DiscountCheckRequest
{
    public string Code { get; set; } = default!;
}
