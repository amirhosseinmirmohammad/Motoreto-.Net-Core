using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class CategoriesController(ApplicationDbContext db) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.ParentId ?? c.Id).ThenBy(c => c.SortOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Icon,
                c.ParentId,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                c.SortOrder,
                c.IsActive,
                ProductCount = c.Products.Count
            })
            .ToListAsync(ct);

        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Parents = await LoadParentsAsync(ct);
        return View(new AdminCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCategoryViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Parents = await LoadParentsAsync(ct);
            return View(model);
        }

        var slug = SlugHelper.Slugify(string.IsNullOrWhiteSpace(model.Slug) ? model.Name : model.Slug);
        if (await db.Categories.AnyAsync(c => c.Slug == slug, ct))
            slug = $"{slug}-{Random.Shared.Next(10, 99)}";

        db.Categories.Add(new Category
        {
            Name = model.Name.Trim(),
            Slug = slug,
            Icon = model.Icon,
            ParentId = model.ParentId,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive
        });
        await db.SaveChangesAsync(ct);

        TempData["Success"] = "دسته ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([id], ct);
        if (category is null) return NotFound();

        ViewBag.Parents = await LoadParentsAsync(ct, excludeId: id);
        return View(new AdminCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Icon = category.Icon,
            ParentId = category.ParentId,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminCategoryViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Parents = await LoadParentsAsync(ct, excludeId: model.Id);
            return View(model);
        }

        var category = await db.Categories.FindAsync([model.Id], ct);
        if (category is null) return NotFound();

        if (model.ParentId == model.Id) model.ParentId = null; // جلوگیری از حلقه

        category.Name = model.Name.Trim();
        category.Slug = SlugHelper.Slugify(string.IsNullOrWhiteSpace(model.Slug) ? model.Name : model.Slug);
        category.Icon = model.Icon;
        category.ParentId = model.ParentId;
        category.SortOrder = model.SortOrder;
        category.IsActive = model.IsActive;

        await db.SaveChangesAsync(ct);
        TempData["Success"] = "دسته به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var hasProducts = await db.Products.AnyAsync(p => p.CategoryId == id, ct);
        var hasChildren = await db.Categories.AnyAsync(c => c.ParentId == id, ct);

        if (hasProducts || hasChildren)
        {
            TempData["Error"] = hasProducts
                ? "ابتدا محصولات این دسته را منتقل کنید."
                : "ابتدا زیر‌دسته‌ها را حذف کنید.";
        }
        else
        {
            await db.Categories.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
            TempData["Success"] = "دسته حذف شد.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> LoadParentsAsync(CancellationToken ct, int? excludeId = null)
    {
        var parents = await db.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null && (excludeId == null || c.Id != excludeId))
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

        var items = parents.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "دسته اصلی (بدون والد)" });
        return items;
    }
}
