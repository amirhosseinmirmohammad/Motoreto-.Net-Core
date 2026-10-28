using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>تنظیمات فروشگاه — هسته ماژولار بودن (هر فروشگاه از همین‌جا شخصی‌سازی می‌شود).</summary>
public class SettingsController(ISettingsService settings) : AdminBaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await settings.GetAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettings model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        await settings.UpdateAsync(model, ct);
        TempData["Success"] = "تنظیمات فروشگاه ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }
}
