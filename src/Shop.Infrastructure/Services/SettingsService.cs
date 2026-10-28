using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Services;

/// <summary>
/// مالک تنظیمات فروشگاه — کش دو لایه HybridCache (L1 حافظه + L2 Redis) با Invalidation هم‌زمان روی هر دو لایه.
/// </summary>
public class SettingsService(ApplicationDbContext db, HybridCache cache) : ISettingsService
{
    private const string CacheKey = "site-settings";

    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(6),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };

    public async Task<SiteSettings> GetAsync(CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync(
            CacheKey,
            async token => await db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(token)
                           ?? throw new InvalidOperationException("تنظیمات فروشگاه یافت نشد. Seed اجرا نشده است."),
            CacheOptions,
            cancellationToken: ct);
    }

    public async Task UpdateAsync(SiteSettings settings, CancellationToken ct = default)
    {
        var entity = await db.SiteSettings.FirstOrDefaultAsync(s => s.Id == settings.Id, ct);
        if (entity is null) throw new InvalidOperationException("تنظیمات یافت نشد.");

        db.Entry(entity).CurrentValues.SetValues(settings);
        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync(CacheKey, ct);
    }

    public void Invalidate() => cache.RemoveAsync(CacheKey).GetAwaiter().GetResult();
}
