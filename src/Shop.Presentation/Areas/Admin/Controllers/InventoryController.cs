using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shop.Domain;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>انبارداری: کارت انبار (دفتر کل گردش موجودی) + ثبت ورود کالا + اصلاح موجودی.</summary>
public class InventoryController(IInventoryService inventory, ApplicationDbContext db) : AdminBaseController
{
    public async Task<IActionResult> Index(long? productId, StockMovementType? type, int page = 1, CancellationToken ct = default)
    {
        var result = await inventory.GetMovementsAsync(productId, type, page, 20, ct);

        ViewBag.ProductId = productId;
        ViewBag.Type = type;
        ViewBag.Types = Enum.GetValues<StockMovementType>()
            .Select(t => new { Value = (int)t, Title = TypeTitle(t) })
            .ToList();
        ViewBag.StockValue = await db.Products.AsNoTracking().SumAsync(p => (long?)p.UnitPrice * p.Stock, ct);

        return View(result);
    }

    [HttpGet]
    public IActionResult Purchase() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(long productId, int quantity, string? note, CancellationToken ct)
    {
        var userId = GetUserId();
        var (ok, message) = await inventory.AdjustAsync(
            productId, Math.Abs(quantity), StockMovementType.Purchase, note, userId, ct: ct);

        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Adjust(long productId, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.Name, p.Stock })
            .FirstOrDefaultAsync(ct);

        if (product is null) return NotFound();

        ViewBag.Product = product;
        ViewBag.Recent = await inventory.GetRecentForProductAsync(productId, 8, ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(long productId, int newStock, string? note, CancellationToken ct)
    {
        var userId = GetUserId();
        var (ok, message) = await inventory.SetStockAsync(productId, newStock, userId, ct);

        TempData[ok ? "Success" : "Error"] = message;
        return ok
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Adjust), new { productId });
    }

    public static string TypeTitle(StockMovementType type) => type switch
    {
        StockMovementType.Purchase => "ورود کالا",
        StockMovementType.Sale => "خروج فروش",
        StockMovementType.Return => "برگشت از فروش",
        StockMovementType.Adjustment => "اصلاح موجودی",
        _ => "نامشخص"
    };

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;
}
