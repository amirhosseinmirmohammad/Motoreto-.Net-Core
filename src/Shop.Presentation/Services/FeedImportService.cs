using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Services;

/// <summary>
/// همگام‌سازی فید فروشگاه‌های متصل (مدل تروب): دانلود JSON فید،
/// upsert کالاها با کلید (feed, external_id)، آپدیت قیمت/موجودی، غیرفعال‌سازی غایب‌ها.
/// </summary>
public class FeedImportService(HttpClient http, ApplicationDbContext db, ILogger<FeedImportService> logger)
{
    public async Task<(bool Ok, string Message)> SyncAsync(int feedId, CancellationToken ct = default)
    {
        var feed = await db.ShopFeeds.FirstAsync(f => f.Id == feedId, ct);

        try
        {
            var json = await http.GetStringAsync(feed.FeedUrl, ct);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("products");

            int added = 0, updated = 0;
            var seenIds = new List<long>();

            // دسته پیش‌فرض: اولین دسته برگ
            var fallbackCat = await db.Categories
                .Where(c => c.ParentId != null)
                .OrderBy(c => c.Id)
                .Select(c => (int?)c.Id)
                .FirstAsync(ct);

            foreach (var item in items.EnumerateArray())
            {
                var ext = item.TryGetProperty("external_id", out var e) ? e.GetString() : null;
                var title = item.TryGetProperty("title", out var ti) ? ti.GetString() : null;
                if (string.IsNullOrWhiteSpace(ext) || string.IsNullOrWhiteSpace(title)) continue;

                long price = item.TryGetProperty("price_toman", out var pr) && pr.ValueKind == JsonValueKind.Number ? pr.GetInt64() : 0;
                int stock = item.TryGetProperty("stock", out var st) && st.ValueKind == JsonValueKind.Number ? st.GetInt32() : 0;
                var image = item.TryGetProperty("image_url", out var im) ? im.GetString() : null;
                var catName = item.TryGetProperty("category", out var ca) ? ca.GetString() : null;
                var desc = item.TryGetProperty("description", out var de) ? de.GetString() : null;

                var code = $"F{feed.Id}-{ext}";
                var catId = await ResolveCategoryAsync(catName, fallbackCat!.Value);

                var p = await db.Products.FirstOrDefaultAsync(x => x.SourceFeedId == feed.Id && x.Code == code, ct);
                if (p is null)
                {
                    p = new Product
                    {
                        SourceFeedId = feed.Id,
                        Code = code,
                        Slug = $"feed{feed.Id}-{ext.ToLowerInvariant()}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Products.Add(p);
                    added++;
                }
                else updated++;

                p.Name = title.Trim();
                p.EnglishName = code;
                p.UnitPrice = price;
                p.Stock = Math.Max(0, stock);
                if (!string.IsNullOrWhiteSpace(image)) p.MainImage = image;
                if (!string.IsNullOrWhiteSpace(desc)) p.Description = desc;
                p.CategoryId = catId;
                p.MetaTitle = $"{title.Trim()} | خرید از مارکت";
                seenIds.Add(p.Id);
            }

            await db.SaveChangesAsync(ct);

            // غیرفعال‌سازی کالاهایی که از فید حذف شده‌اند
            var missing = await db.Products
                .Where(x => x.SourceFeedId == feed.Id && x.IsActive && !seenIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), ct);

            feed.LastSyncAt = DateTime.UtcNow;
            feed.LastSyncError = null;
            feed.ProductCount = seenIds.Count;
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Feed {FeedId} synced: +{Added} ~{Updated} -{Missing}", feedId, added, updated, missing);
            return (true, $"همگام‌سازی شد: {added} جدید، {updated} به‌روزرسانی، {missing} غیرفعال");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Feed {FeedId} sync failed", feedId);
            feed.LastSyncError = ex.Message;
            await db.SaveChangesAsync(ct);
            return (false, "خطا در همگام‌سازی: " + ex.Message);
        }
    }

    private async Task<int> ResolveCategoryAsync(string? catName, int fallback)
    {
        if (!string.IsNullOrWhiteSpace(catName))
        {
            var c = await db.Categories
                .Where(x => x.ParentId != null && x.Name == catName.Trim())
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
            if (c is not null) return c.Value;
        }
        return fallback;
    }
}
