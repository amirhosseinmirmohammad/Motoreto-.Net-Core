using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shop.Presentation.Services;

/// <summary>بررسی سلامت اتصال دیتابیس — بدون پکیج اضافه.</summary>
public class DbHealthCheck(ApplicationDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("دیتابیس در دسترس است.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("اتصال دیتابیس برقرار نیست.", ex);
        }
    }
}
