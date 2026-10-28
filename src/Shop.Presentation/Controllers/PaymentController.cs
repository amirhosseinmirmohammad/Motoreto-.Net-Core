using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;

namespace Shop.Presentation;

/// <summary>
/// جریان پرداخت: ایجاد تراکنش → صفحه درگاه → بازگشت و تأیید اتمیک.
/// درگاه فعال از تنظیمات خوانده می‌شود (Factory + Keyed DI).
/// </summary>
[Authorize]
public class PaymentController(
    IPaymentGatewayFactory gatewayFactory,
    IOrderService orders,
    ApplicationDbContext db,
    IStockReservationService reservations) : Controller
{
    /// <summary>ایجاد پرداخت و انتقال به درگاه.</summary>
    [HttpGet("/payment/pay/{orderId:int}")]
    public async Task<IActionResult> Pay(int orderId, CancellationToken ct)
    {
        var order = await orders.GetOrderAsync(orderId, CurrentUserId(), ct);
        if (order is null) return NotFound();
        if (order.StatusTitle != OrderTitles.StatusTitle(Domain.Enums.OrderStatus.AwaitingPayment))
            return RedirectToAction(nameof(OrderController.Result), "Order", new { id = orderId });

        var gateway = await gatewayFactory.GetActiveAsync(ct);

        var payment = await db.Payments
            .Where(p => p.OrderId == orderId && p.Status == Domain.Enums.PaymentStatus.Pending)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            payment = new Domain.Payment
            {
                OrderId = orderId,
                Amount = order.Total,
                Gateway = gateway.Name,
                Authority = Guid.NewGuid().ToString("N"),
                Status = Domain.Enums.PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);
        }

        var request = new PaymentRequest
        {
            OrderId = orderId,
            Amount = order.Total,
            Description = $"پرداخت سفارش {order.OrderNumber}",
            CallbackUrl = Url.ActionLink(nameof(Callback), "Payment")!
        };

        var result = await gateway.CreatePaymentAsync(request, ct);

        if (!result.Success || result.RedirectUrl is null)
        {
            TempData["PaymentError"] = result.Message;
            return RedirectToAction(nameof(OrderController.Result), "Order", new { id = orderId });
        }

        // Authority دریافتی از درگاه باید روی تراکنش ذخیره شود (جریان استاندارد درگاه‌ها)
        if (!string.IsNullOrEmpty(result.Authority))
        {
            payment.Authority = result.Authority;
            await db.SaveChangesAsync(ct);
        }

        return Redirect(result.RedirectUrl);
    }

    /// <summary>صفحه درگاه شبیه‌سازی‌شده (شبیه صفحه بانک).</summary>
    [HttpGet("/payment/gateway/{authority}")]
    public async Task<IActionResult> Gateway(string authority, long amount, int order, CancellationToken ct)
    {
        var payment = await db.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Authority == authority, ct);

        if (payment is null || payment.Status != Domain.Enums.PaymentStatus.Pending)
            return RedirectToAction(nameof(OrderController.Index), "Order");

        ViewBag.Amount = amount;
        ViewBag.OrderId = order;
        ViewBag.Authority = authority;
        ViewBag.OrderNumber = payment.Order.OrderNumber;
        ViewBag.UserName = payment.Order.ReceiverName;

        return View();
    }

    /// <summary>نتیجه پرداخت از درگاه (تأیید اتمیک سفارش).</summary>
    [HttpPost("/payment/callback")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Callback(string authority, string action, CancellationToken ct)
    {
        var approved = action == "pay";

        var payment = await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Authority == authority, ct);

        if (payment is null)
            return RedirectToAction(nameof(OrderController.Index), "Order");

        var gateway = await gatewayFactory.GetActiveAsync(ct);
        var verification = await gateway.VerifyAsync(authority, approved, ct);

        if (verification.Success)
        {
            var paid = await orders.MarkPaidAsync(payment.OrderId, verification.RefId ?? "UNKNOWN", ct);
            await reservations.ReleaseUserReservationsAsync(CurrentUserId(), ct);

            if (paid)
                return RedirectToAction(nameof(OrderController.Result), "Order", new { id = payment.OrderId });
        }
        else
        {
            await orders.MarkPaymentFailedAsync(payment.Id, ct);
        }

        return RedirectToAction(nameof(OrderController.Result), "Order", new { id = payment.OrderId, failed = true });
    }

    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;
}
