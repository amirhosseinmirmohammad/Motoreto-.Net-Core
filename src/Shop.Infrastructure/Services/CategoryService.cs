using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Shop.Infrastructure.Services;

/// <summary>مدیریت دسته‌بندی‌ها.</summary>
public class CategoryService(ApplicationDbContext db) : ICategoryService
{
    private const string MenuCacheKey = "categories-menu";

    public async Task<List<CategoryNodeDto>> GetMenuAsync(CancellationToken ct = default)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new { c.Id, c.Name, c.Slug, c.Icon, c.ParentId })
            .ToListAsync(ct);

        // درخت ۳ سطحی: ریشه → زیرشاخه → زیرشاخه دوم
        var byParent = categories
            .GroupBy(c => c.ParentId ?? 0)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<CategoryNodeDto> Build(int? parentId) =>
            byParent.TryGetValue(parentId ?? 0, out var list)
                ? list.Select(c => new CategoryNodeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Icon = c.Icon,
                    Children = Build(c.Id)
                }).ToList()
                : [];

        return Build(null);
    }

    public async Task<(int CategoryId, string Name)?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await db.Categories
            .AsNoTracking()
            .Where(c => c.Slug == slug && c.IsActive)
            .Select(c => new ValueTuple<int, string>(c.Id, c.Name))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<int>> GetCategoryWithChildrenIdsAsync(int categoryId, CancellationToken ct = default)
    {
        // نسل اول و دوم زیرشاخه‌ها (درخت ۳ سطحی)
        var level1 = await db.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId || c.ParentId == categoryId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var level2 = await db.Categories
            .AsNoTracking()
            .Where(c => level1.Contains(c.ParentId ?? -1))
            .Select(c => c.Id)
            .ToListAsync(ct);

        return level1.Concat(level2).Distinct().ToList();
    }

    public void InvalidateCache() { /* منو توسط Decorator کش می‌شود */ }
}

/// <summary>
/// Decorator Pattern: کش دو لایه (L1 حافظه + L2 Redis با HybridCache) روی سرویس دسته‌بندی بدون تغییر منطق اصلی (OCP).
/// </summary>
public class CachedCategoryService(ICategoryService inner, Microsoft.Extensions.Caching.Hybrid.HybridCache cache) : ICategoryService
{
    private const string CacheKey = "categories-menu-v1";

    private static readonly Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(12),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<List<CategoryNodeDto>> GetMenuAsync(CancellationToken ct = default)
        => await cache.GetOrCreateAsync(CacheKey,
            async token => await inner.GetMenuAsync(token),
            CacheOptions, tags: ["categories"], cancellationToken: ct);

    public Task<(int CategoryId, string Name)?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => inner.GetBySlugAsync(slug, ct);

    public Task<List<int>> GetCategoryWithChildrenIdsAsync(int categoryId, CancellationToken ct = default)
        => inner.GetCategoryWithChildrenIdsAsync(categoryId, ct);

    public void InvalidateCache()
    {
        cache.RemoveAsync(CacheKey).GetAwaiter().GetResult();
        inner.InvalidateCache();
    }
}
