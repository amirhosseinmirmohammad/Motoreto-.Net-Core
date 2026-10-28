using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Background;

/// <summary>
/// Worker Pattern: آزادسازی خودکار رزروهای منقضی موجودی.
/// فاصله اجرا از تنظیمات فروشگاه خوانده می‌شود.
/// </summary>
public class ReservationCleanupJob(IServiceScopeFactory scopeFactory, ILogger<ReservationCleanupJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReservationCleanupJob شروع به کار کرد");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var intervalSeconds = await db.SiteSettings
                    .AsNoTracking()
                    .OrderBy(s => s.Id)
                    .Select(s => (int?)s.ReservationCleanupIntervalSeconds)
                    .FirstOrDefaultAsync(stoppingToken) ?? 60;

                var reservationService = scope.ServiceProvider.GetRequiredService<IStockReservationService>();
                await reservationService.ReleaseExpiredAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(intervalSeconds, 15, 3600)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "خطا در پاکسازی رزروهای منقضی");
                await SafeDelay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private static async Task SafeDelay(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (OperationCanceledException) { }
    }
}
