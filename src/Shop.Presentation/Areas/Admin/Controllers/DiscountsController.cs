using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Application.ViewModels;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class DiscountsController(IDiscountService discounts) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await discounts.GetAllAsync(ct));

    [HttpGet]
    public IActionResult Create() => View(new Discount());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Discount discount, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(discount);

        await discounts.CreateAsync(discount, ct);
        TempData["Success"] = "کد تخفیف ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var discount = await discounts.GetByIdAsync(id, ct);
        if (discount is null) return NotFound();
        return View(discount);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Discount discount, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(discount);

        await discounts.UpdateAsync(discount, ct);
        TempData["Success"] = "کد تخفیف به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        await discounts.ToggleAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }
}
