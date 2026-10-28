using Shop.Domain;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Shop.Infrastructure.Services;

/// <summary>مدیریت اسلایدرها.</summary>
public class SliderService(ApplicationDbContext db) : ISliderService
{
    public async Task<List<Slider>> GetActiveAsync(CancellationToken ct = default)
    {
        return await db.Sliders
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<List<Slider>> GetAllAsync(CancellationToken ct = default)
        => await db.Sliders.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(ct);

    public async Task<Slider?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Sliders.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task CreateAsync(Slider slider, CancellationToken ct = default)
    {
        db.Sliders.Add(slider);
        await db.SaveChangesAsync(ct);
        InvalidateCache();
    }

    public async Task UpdateAsync(Slider slider, CancellationToken ct = default)
    {
        var entity = await db.Sliders.FirstOrDefaultAsync(s => s.Id == slider.Id, ct);
        if (entity is null) return;

        entity.Title = slider.Title;
        entity.SubTitle = slider.SubTitle;
        entity.Image = slider.Image;
        entity.Link = slider.Link;
        entity.Theme = slider.Theme;
        entity.SortOrder = slider.SortOrder;
        entity.IsActive = slider.IsActive;

        await db.SaveChangesAsync(ct);
        InvalidateCache();
    }

    public async Task ToggleAsync(int id, CancellationToken ct = default)
    {
        await db.Sliders
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, x => !x.IsActive), ct);
        InvalidateCache();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await db.Sliders.Where(s => s.Id == id).ExecuteDeleteAsync(ct);
        InvalidateCache();
    }

    public void InvalidateCache() { }
}

/// <summary>Decorator Pattern: کش دو لایه (حافظه + Redis) برای اسلایدرهای فعال.</summary>
public class CachedSliderService(ISliderService inner, Microsoft.Extensions.Caching.Hybrid.HybridCache cache) : ISliderService
{
    private const string CacheKey = "sliders-active-v1";

    private static readonly Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(2),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<List<Slider>> GetActiveAsync(CancellationToken ct = default)
        => await cache.GetOrCreateAsync(CacheKey,
            async token => await inner.GetActiveAsync(token),
            CacheOptions, tags: ["sliders"], cancellationToken: ct);

    public Task<List<Slider>> GetAllAsync(CancellationToken ct = default) => inner.GetAllAsync(ct);
    public Task<Slider?> GetByIdAsync(int id, CancellationToken ct = default) => inner.GetByIdAsync(id, ct);
    public Task CreateAsync(Slider slider, CancellationToken ct = default) => inner.CreateAsync(slider, ct);
    public Task UpdateAsync(Slider slider, CancellationToken ct = default) => inner.UpdateAsync(slider, ct);

    public async Task ToggleAsync(int id, CancellationToken ct = default)
    {
        await inner.ToggleAsync(id, ct);
        await cache.RemoveAsync(CacheKey, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await inner.DeleteAsync(id, ct);
        await cache.RemoveAsync(CacheKey, ct);
    }

    public void InvalidateCache()
    {
        cache.RemoveAsync(CacheKey).GetAwaiter().GetResult();
        inner.InvalidateCache();
    }
}
