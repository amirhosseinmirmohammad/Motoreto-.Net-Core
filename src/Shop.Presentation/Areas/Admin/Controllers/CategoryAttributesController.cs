using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>مدیریت ویژگی‌های داینامیک هر دسته‌بندی (نام، نمایش در فیلتر، مشارکت در واریانت‌ها).</summary>
public class CategoryAttributesController(ApplicationDbContext db) : AdminBaseController
{
    public async Task<IActionResult> Index(int? categoryId, CancellationToken ct = default)
    {
        var cats = await db.Categories.AsNoTracking()
            .OrderBy(c => c.ParentId)
            .ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.ParentId })
            .ToListAsync(ct);

        var parents = cats.Where(c => c.ParentId == null).ToDictionary(c => c.Id, c => c.Name);

        var selected = categoryId ?? cats.FirstOrDefault()?.Id ?? 0;

        ViewBag.CategoryOptions = cats
            .Select(c => new
            {
                c.Id,
                Label = c.ParentId is null ? c.Name : $"— {c.Name} ({parents.GetValueOrDefault(c.ParentId.Value, "")})"
            })
            .ToList();

        ViewBag.SelectedId = selected;

        var attrs = await db.CategoryAttributes.AsNoTracking()
            .Where(a => a.CategoryId == selected)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name)
            .ToListAsync(ct);

        return View(attrs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int categoryId, string? name, bool showInFilter = false,
        bool isVariantOption = false, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Ok"] = "نام ویژگی نمی‌تواند خالی باشد.";
            return RedirectToAction(nameof(Index), new { categoryId });
        }

        if (!await db.Categories.AnyAsync(c => c.Id == categoryId, ct))
        {
            TempData["Ok"] = "دسته‌بندی یافت نشد.";
            return RedirectToAction(nameof(Index));
        }

        db.CategoryAttributes.Add(new CategoryAttribute
        {
            CategoryId = categoryId,
            Name = name.Trim(),
            ShowInFilter = showInFilter,
            IsVariantOption = isVariantOption,
            SortOrder = await db.CategoryAttributes.CountAsync(a => a.CategoryId == categoryId, ct)
        });
        await db.SaveChangesAsync(ct);

        TempData["Ok"] = "ویژگی اضافه شد.";
        return RedirectToAction(nameof(Index), new { categoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int categoryId, CancellationToken ct = default)
    {
        var attr = await db.CategoryAttributes.FindAsync(new object[] { id }, ct);
        if (attr is not null)
        {
            db.CategoryAttributes.Remove(attr);
            await db.SaveChangesAsync(ct);
            TempData["Ok"] = "ویژگی حذف شد.";
        }
        return RedirectToAction(nameof(Index), new { categoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, int categoryId, CancellationToken ct = default)
    {
        var attr = await db.CategoryAttributes.FindAsync(new object[] { id }, ct);
        if (attr is not null)
        {
            attr.ShowInFilter = !attr.ShowInFilter;
            await db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Index), new { categoryId });
    }
}
