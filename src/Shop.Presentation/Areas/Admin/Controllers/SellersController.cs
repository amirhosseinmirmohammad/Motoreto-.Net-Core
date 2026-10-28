using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Shop.Domain;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>بررسی و تأیید فروشندگان مارکت‌پلیس (مجوز کسب‌وکار).</summary>
public class SellersController(ISellerService sellers, ApplicationDbContext db) : AdminBaseController
{
    public async Task<IActionResult> Index(SellerStatus? status, CancellationToken ct)
    {
        ViewBag.Status = status;
        ViewBag.Statuses = Enum.GetValues<SellerStatus>()
            .Select(s => new { Value = (int)s, Title = StatusTitle(s) })
            .ToList();

        return View(await sellers.GetAllForAdminAsync(status, ct));
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var seller = await sellers.GetByIdAsync(id, ct);
        if (seller is null) return NotFound();

        ViewBag.Offers = await db.SellerOffers.Include(o => o.Seller).AsNoTracking().Where(o => o.SellerId == id).OrderByDescending(o => o.Id).Select(o => new { o.Id, o.Price, o.DiscountPercent, o.Stock, o.IsActive, ProductName = o.Product.Name, SellerName = o.Seller.StoreName }).ToListAsync(ct);
        return View(seller);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id, int commissionPercent, CancellationToken ct)
    {
        await sellers.ApproveAsync(id, commissionPercent, ct);
        TempData["Success"] = "فروشگاه تأیید شد و محصولاتش فعال است.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long id, string reason, CancellationToken ct)
    {
        await sellers.RejectAsync(id, string.IsNullOrWhiteSpace(reason) ? "مدارک ناقص است." : reason.Trim(), ct);
        TempData["Success"] = "درخواست فروشنده رد شد.";
        return RedirectToAction(nameof(Index));
    }

    public static string StatusTitle(SellerStatus status) => status switch
    {
        SellerStatus.PendingApproval => "در انتظار بررسی",
        SellerStatus.Approved => "تأییدشده",
        SellerStatus.Rejected => "ردشده",
        _ => "نامشخص"
    };
}
