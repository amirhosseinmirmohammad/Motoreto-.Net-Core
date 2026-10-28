using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Common;

/// <summary>
/// اجرای امن عملیات داخل تراکنش با پشتیبانی از Retry Strategy (EnableRetryOnFailure).
/// </summary>
public static class Transactional
{
    public static async Task<T> RunAsync<T>(ApplicationDbContext db, Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await operation(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public static Task RunAsync(ApplicationDbContext db, Func<CancellationToken, Task> operation, CancellationToken ct = default)
        => RunAsync<object?>(db, async token =>
        {
            await operation(token);
            return null;
        }, ct);
}
