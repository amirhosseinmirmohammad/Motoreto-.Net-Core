using Microsoft.EntityFrameworkCore;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Background;

/// <summary>
/// jobهای زمان‌بندی‌شدهٔ آگهی‌ها (با Hangfire ساعتی اجرا می‌شوند):
/// انقضای آگهی‌ها، انقضای نردبان/فوری (۷ روز)، حذف ردشده‌های قدیمی.
/// </summary>
public class ClassifiedJobs(ApplicationDbContext db, ILogger<ClassifiedJobs> logger)
{
    public async Task ExpireAndCleanupAsync()
    {
        var now = DateTime.UtcNow;

        // آگهی‌های منقضی → Expired (آرشیو منطقی، حذف نمی‌شوند)
        var expired = await db.ClassifiedAds
            .Where(a => a.Status == ClassifiedAdStatus.Approved && a.ExpiresAt != null && a.ExpiresAt < now)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, ClassifiedAdStatus.Expired));

        // نردبان و فوری بعد از ۷ روز خودکار برداشته می‌شوند
        var unbumped = await db.ClassifiedAds
            .Where(a => a.IsFeatured && a.BumpAt != null && a.BumpAt < now.AddDays(-7))
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsFeatured, false));

        var unurgent = await db.ClassifiedAds
            .Where(a => a.IsUrgent && a.UrgentAt != null && a.UrgentAt < now.AddDays(-7))
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsUrgent, false));

        // ردشده‌های قدیمی‌تر از ۹۰ روز حذف کامل
        var removed = await db.ClassifiedAds
            .Where(a => a.Status == ClassifiedAdStatus.Rejected && a.CreatedAt < now.AddDays(-90))
            .ExecuteDeleteAsync();

        if (expired + unbumped + unurgent + removed > 0)
            logger.LogInformation("ClassifiedJobs: expired={Expired} unbump={Bump} unurgent={Urgent} removed={Removed}",
                expired, unbumped, unurgent, removed);
    }
}
