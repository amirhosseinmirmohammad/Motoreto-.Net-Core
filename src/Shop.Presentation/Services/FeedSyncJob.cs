using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Services;

/// <summary>
/// همگام‌سازی دوره‌ای فید فروشگاه‌های متصل (مدل تروب):
/// هر ۵ دقیقه فیدهایی که SyncIntervalMinutes > 0 دارند و موعدشان رسیده sync می‌شوند.
/// (الگوی ReservationCleanupJob — سرویس‌های scoped از scope جدید گرفته می‌شوند.)
/// </summary>
public class FeedSyncJob(IServiceScopeFactory scopeFactory, ILogger<FeedSyncJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FeedSyncJob شروع به کار کرد");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var importer = scope.ServiceProvider.GetRequiredService<FeedImportService>();

                var now = DateTime.UtcNow;
                var dueFeedIds = await db.ShopFeeds.AsNoTracking()
                    .Where(f => f.IsActive
                                && f.SyncIntervalMinutes > 0
                                && (f.LastSyncAt == null || now >= f.LastSyncAt.Value.AddMinutes(f.SyncIntervalMinutes)))
                    .OrderBy(f => f.LastSyncAt)
                    .Select(f => f.Id)
                    .ToListAsync(stoppingToken);

                foreach (var feedId in dueFeedIds)
                {
                    var (ok, msg) = await importer.SyncAsync(feedId, stoppingToken);
                    logger.LogInformation("Feed {FeedId} sync دوره‌ای: {Result}", feedId, msg);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطا در همگام‌سازی دوره‌ای فیدها");
                await SafeDelay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (OperationCanceledException) { }
    }
}
