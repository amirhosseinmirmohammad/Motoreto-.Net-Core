using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Services;

/// <summary>
/// انبارداری: تنها راه تغییر موجودی؛ هر گردش در کارت انبار (StockMovement) ثبت می‌شود.
/// تغییر موجودی با UPDATE اتمیک انجام می‌شود تا همزمانی خراب نشود.
/// </summary>
public class InventoryService(ApplicationDbContext db) : IInventoryService
{
    public async Task<(bool Success, string Message)> AdjustAsync(long productId, int delta, StockMovementType type, string? note, Guid? byUserId, string? reference = null, CancellationToken ct = default)
    {
        if (delta == 0)
            return (false, "تغییر موجودی صفر است.");

        var stockAfter = await TryChangeStockAsync(productId, delta, ct);
        if (stockAfter is null)
            return (false, "موجودی نمی‌تواند منفی شود.");

        db.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            Type = type,
            Delta = delta,
            StockAfter = stockAfter.Value,
            Note = note,
            Reference = reference,
            CreatedByUserId = byUserId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var title = type switch
        {
            StockMovementType.Purchase => "ورود کالا",
            StockMovementType.Return => "برگشت از فروش",
            StockMovementType.Adjustment => "اصلاح موجودی",
            _ => "گردش انبار"
        };
        return (true, $"{title}: {Math.Abs(delta)} عدد — مانده: {stockAfter}");
    }

    public async Task<(bool Success, string Message)> SetStockAsync(long productId, int newStock, Guid? byUserId, CancellationToken ct = default)
    {
        if (newStock < 0)
            return (false, "موجودی نمی‌تواند منفی باشد.");

        var current = await db.Products.Where(p => p.Id == productId).Select(p => (int?)p.Stock).FirstOrDefaultAsync(ct);
        if (current is null)
            return (false, "محصول یافت نشد.");

        var delta = newStock - current.Value;
        if (delta == 0)
            return (true, "موجودی بدون تغییر بود.");

        return await AdjustAsync(productId, delta, StockMovementType.Adjustment, "اصلاح دستی موجودی انبار", byUserId, ct: ct);
    }

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(long? productId, StockMovementType? type, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 10, 100);
        page = Math.Max(1, page);

        var query = db.StockMovements.AsNoTracking().AsQueryable();
        if (productId is not null) query = query.Where(m => m.ProductId == productId);
        if (type is not null) query = query.Where(m => m.Type == type);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                ProductName = m.Product.Name,
                TypeTitle = m.Type == StockMovementType.Purchase ? "ورود کالا"
                    : m.Type == StockMovementType.Sale ? "خروج فروش"
                    : m.Type == StockMovementType.Return ? "برگشت از فروش"
                    : "اصلاح موجودی",
                TypeClass = m.Type == StockMovementType.Purchase ? "badge-green"
                    : m.Type == StockMovementType.Sale ? "badge-blue"
                    : m.Type == StockMovementType.Return ? "badge-amber"
                    : "badge-gray",
                Delta = m.Delta,
                StockAfter = m.StockAfter,
                Reference = m.Reference,
                Note = m.Note,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<StockMovementDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<List<StockMovementDto>> GetRecentForProductAsync(long productId, int count, CancellationToken ct = default)
    {
        return await db.StockMovements
            .AsNoTracking()
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Id)
            .Take(count)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                TypeTitle = m.Type == StockMovementType.Purchase ? "ورود کالا"
                    : m.Type == StockMovementType.Sale ? "خروج فروش"
                    : m.Type == StockMovementType.Return ? "برگشت از فروش"
                    : "اصلاح موجودی",
                TypeClass = m.Type == StockMovementType.Purchase ? "badge-green"
                    : m.Type == StockMovementType.Sale ? "badge-blue"
                    : m.Type == StockMovementType.Return ? "badge-amber"
                    : "badge-gray",
                Delta = m.Delta,
                StockAfter = m.StockAfter,
                Reference = m.Reference,
                Note = m.Note,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);
    }

    /// <summary>UPDATE اتمیک؛ مانده جدید یا null (در صورت منفی‌شدن) برمی‌گردد.</summary>
    private async Task<int?> TryChangeStockAsync(long productId, int delta, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE products
               SET stock = stock + {delta}
               WHERE id = {productId} AND stock + {delta} >= 0",
            ct);

        // خواندن مانده جدید
        var stock = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => (int?)p.Stock)
            .FirstOrDefaultAsync(ct);

        return stock;
    }
}
