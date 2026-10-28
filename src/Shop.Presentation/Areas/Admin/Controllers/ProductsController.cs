using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class ProductsController(
    ApplicationDbContext db,
    ICategoryService categories,
    IProductService products) : AdminBaseController
{
    public async Task<IActionResult> Index(string? q, int? categoryId, int page = 1, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        const int pageSize = 15;

        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.Name.Contains(q) || p.EnglishName.Contains(q) || p.Code.Contains(q));
        }

        if (categoryId is not null)
        {
            var ids = await categories.GetCategoryWithChildrenIdsAsync(categoryId.Value, ct);
            query = query.Where(p => ids.Contains(p.CategoryId));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.UnitPrice,
                p.DiscountPercent,
                p.Stock,
                p.ReservedCount,
                p.SalesCount,
                p.IsActive,
                p.IsSpecial,
                p.IsAmazing,
                p.MainImage,
                CategoryName = p.Category.Name
            })
            .ToListAsync(ct);

        ViewBag.TotalCount = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Q = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await LoadCategorySelectAsync(ct);

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Categories = await LoadCategorySelectAsync(ct);
        ViewBag.Brands = await LoadBrandSelectAsync(ct);
        ViewBag.AttrRows = new List<(string Name, string Values)>();
        return View(new AdminProductViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await LoadCategorySelectAsync(ct);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(
            string.IsNullOrWhiteSpace(model.Slug) ? SlugHelper.Slugify(model.Name) : SlugHelper.Slugify(model.Slug), null, ct);

        var product = new Product
        {
            Name = model.Name.Trim(),
            EnglishName = model.EnglishName.Trim(),
            Slug = slug,
            Code = string.IsNullOrWhiteSpace(model.Code) ? await GenerateProductCodeAsync(ct) : model.Code.Trim(),
            UnitPrice = model.UnitPrice,
            DiscountPercent = model.DiscountPercent,
            Stock = model.Stock,
            Description = model.Description,
            CategoryId = model.CategoryId,
            BrandId = model.BrandId,
            MainImage = model.MainImage ?? "/images/products/placeholder.svg",
            IsAmazing = model.IsAmazing,
            AmazingEndsAt = model.IsAmazing ? model.AmazingEndsAt ?? DateTime.UtcNow.AddDays(3) : null,
            IsSpecial = model.IsSpecial,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        product.AttributesJson = BuildAttributesJson(model.AttrNames, model.AttrValues);
        await db.SaveChangesAsync(ct);
        await AddGalleryAsync(product.Id, model.GalleryUrls, ct);

        TempData["Success"] = $"محصول «{product.Name}» ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null) return NotFound();

        ViewBag.Categories = await LoadCategorySelectAsync(ct);
        ViewBag.Brands = await LoadBrandSelectAsync(ct);
        ViewBag.AttrRows = ParseAttrRows(product.AttributesJson);
        ViewBag.GalleryUrls = string.Join("\n", product.Images.Where(i => i.SortOrder > 0).Select(i => i.Url));

        return View(new AdminProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            EnglishName = product.EnglishName,
            Slug = product.Slug,
            Code = product.Code,
            UnitPrice = product.UnitPrice,
            DiscountPercent = product.DiscountPercent,
            Stock = product.Stock,
            Description = product.Description,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            MainImage = product.MainImage,
            IsAmazing = product.IsAmazing,
            AmazingEndsAt = product.AmazingEndsAt,
            IsSpecial = product.IsSpecial,
            IsActive = product.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminProductViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await LoadCategorySelectAsync(ct);
            return View(model);
        }

        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == model.Id, ct);
        if (product is null) return NotFound();

        var slug = await EnsureUniqueSlugAsync(
            string.IsNullOrWhiteSpace(model.Slug) ? SlugHelper.Slugify(model.Name) : SlugHelper.Slugify(model.Slug), product.Id, ct);

        product.Name = model.Name.Trim();
        product.EnglishName = model.EnglishName.Trim();
        product.Slug = slug;
        product.Code = model.Code?.Trim() ?? product.Code;
        product.UnitPrice = model.UnitPrice;
        product.DiscountPercent = model.DiscountPercent;
        product.Stock = model.Stock;
        product.Description = model.Description;
        product.CategoryId = model.CategoryId;
        product.BrandId = model.BrandId;
        product.MainImage = model.MainImage ?? product.MainImage;
        product.IsAmazing = model.IsAmazing;
        product.AmazingEndsAt = model.IsAmazing ? model.AmazingEndsAt ?? DateTime.UtcNow.AddDays(3) : null;
        product.IsSpecial = model.IsSpecial;
        product.IsActive = model.IsActive;
        product.AttributesJson = BuildAttributesJson(model.AttrNames, model.AttrValues);
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await db.ProductImages.Where(i => i.ProductId == product.Id).ExecuteDeleteAsync(ct);
        await AddGalleryAsync(product.Id, model.GalleryUrls, ct);

        TempData["Success"] = "محصول به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(long id, CancellationToken ct)
    {
        await db.Products.Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, p => !p.IsActive), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var hasOrders = await db.OrderItems.AnyAsync(i => i.ProductId == id, ct);
        if (hasOrders)
        {
            await db.Products.Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, false), ct);
            TempData["Error"] = "این محصول سفارش دارد؛ غیرفعال شد (تاریخچه حفظ شد).";
        }
        else
        {
            await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(ct);
            TempData["Success"] = "محصول حذف شد.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- private ----

    private async Task<List<SelectListItem>> LoadBrandSelectAsync(CancellationToken ct)
    {
        var brands = await db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync(ct);

        var list = brands.Select(b => new SelectListItem(b.Name, b.Id.ToString())).ToList();
        list.Insert(0, new SelectListItem("بدون برند", ""));
        return list;
    }

    /// <summary>ساخت AttributesJson از ردیف‌های نام/مقدار فرم (مقادیر چندتایی با ویرگول جدا می‌شوند).</summary>
    private static string? BuildAttributesJson(List<string>? names, List<string>? values)
    {
        if (names is null || names.Count == 0) return null;

        var groups = new List<object>();
        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i]?.Trim();
            var value = (i < values?.Count ? values[i] : null)?.Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) continue;

            groups.Add(new
            {
                name,
                values = value.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            });
        }

        return groups.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(groups);
    }

    /// <summary>گروه‌های ویژگی موجود برای نمایش در فرم (مقادیر با ویرگول فارسی).</summary>
    private static List<(string Name, string Values)> ParseAttrRows(string? json)
    {
        var rows = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(json)) return rows;

        try
        {
            var groups = System.Text.Json.JsonSerializer.Deserialize<List<Shop.Application.DTOs.ProductAttributeGroupDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foreach (var g in groups ?? [])
                if (!string.IsNullOrEmpty(g.Name) && (g.Values ?? []).Count > 0)
                    rows.Add((g.Name, string.Join("، ", g.Values)));
        }
        catch { /* JSON خراب — فرم خالی */ }

        return rows;
    }

    private async Task<string> GenerateProductCodeAsync(CancellationToken ct)
    {
        var count = await db.Products.CountAsync(ct);
        return $"SH-{200000 + count + 1}";
    }

    private async Task<List<SelectListItem>> LoadCategorySelectAsync(CancellationToken ct)
    {
        var cats = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.ParentId ?? c.Id).ThenBy(c => c.SortOrder)
            .ToListAsync(ct);

        return cats.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.ParentId is null ? c.Name : $"— {c.Name}"
        }).ToList();
    }

    private async Task<string> EnsureUniqueSlugAsync(string slug, long? excludeId, CancellationToken ct)
    {
        var baseSlug = slug;
        var i = 1;
        while (await db.Products.AnyAsync(p => p.Slug == slug && (excludeId == null || p.Id != excludeId), ct))
        {
            slug = $"{baseSlug}-{++i}";
        }
        return slug;
    }

    private async Task AddGalleryAsync(long productId, string? galleryUrls, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(galleryUrls)) return;

        var urls = galleryUrls
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(8)
            .ToList();

        if (urls.Count == 0) return;

        var images = urls.Select((url, i) => new ProductImage
        {
            ProductId = productId,
            Url = url,
            SortOrder = i
        });

        db.ProductImages.AddRange(images);
        await db.SaveChangesAsync(ct);
    }
}
