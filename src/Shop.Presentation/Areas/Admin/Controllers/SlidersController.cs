using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Domain;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class SlidersController(ISliderService sliders) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await sliders.GetAllAsync(ct));

    [HttpGet]
    public IActionResult Create() => View(new Slider());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Slider slider, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(slider);

        await sliders.CreateAsync(slider, ct);
        TempData["Success"] = "اسلایدر ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var slider = await sliders.GetByIdAsync(id, ct);
        if (slider is null) return NotFound();
        return View(slider);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Slider slider, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(slider);

        await sliders.UpdateAsync(slider, ct);
        TempData["Success"] = "اسلایدر به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        await sliders.ToggleAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sliders.DeleteAsync(id, ct);
        TempData["Success"] = "اسلایدر حذف شد.";
        return RedirectToAction(nameof(Index));
    }
}
