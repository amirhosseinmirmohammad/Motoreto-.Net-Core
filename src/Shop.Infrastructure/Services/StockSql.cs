using System.Globalization;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

/// <summary>
/// عملیات اتمیک SQL روی موجودی و رزروها — ضد Race Condition:
/// چند کاربر همزمان هرگز نمی‌توانند بیش از موجودی واقعی رزرو/خرید کنند.
/// </summary>
public static class StockSql
{
    /// <summary>رزرو از انبار مارکت (محصول بدون آفر).</summary>
    public static async Task<bool> TryReserveAsync(ApplicationDbContext db, long productId, int quantity, CancellationToken ct)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE products
               SET reserved_count = reserved_count + {quantity}
               WHERE id = {productId} AND stock - reserved_count >= {quantity}",
            ct);
        return affected > 0;
    }

    /// <summary>رزرو از موجودی یک فروشنده (آفر) — همان گارد اتمیک.</summary>
    public static async Task<bool> TryReserveOfferAsync(ApplicationDbContext db, long offerId, int quantity, CancellationToken ct)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE seller_offers
               SET reserved_count = reserved_count + {quantity}
               WHERE id = {offerId} AND stock - reserved_count >= {quantity}",
            ct);
        return affected > 0;
    }

    /// <summary>کاهش/افزایش اتمیک رزرو یک محصول (انبار مارکت).</summary>
    public static async Task ChangeReservedAsync(ApplicationDbContext db, long productId, int delta, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE products
               SET reserved_count = GREATEST(reserved_count + {delta}, 0)
               WHERE id = {productId}",
            ct);
    }

    /// <summary>کاهش/افزایش اتمیک رزرو یک آفر.</summary>
    public static async Task ChangeOfferReservedAsync(ApplicationDbContext db, long offerId, int delta, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE seller_offers
               SET reserved_count = GREATEST(reserved_count + {delta}, 0)
               WHERE id = {offerId}",
            ct);
    }

    /// <summary>رزرو از موجودی یک تنوع (ترکیب ویژگی‌ها).</summary>
    public static async Task<bool> TryReserveVariantAsync(ApplicationDbContext db, long variantId, int quantity, CancellationToken ct)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE product_variants
               SET reserved_count = reserved_count + {quantity}
               WHERE id = {variantId} AND stock - reserved_count >= {quantity}",
            ct);
        return affected > 0;
    }

    /// <summary>کاهش/افزایش اتمیک رزرو یک تنوع.</summary>
    public static async Task ChangeVariantReservedAsync(ApplicationDbContext db, long variantId, int delta, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE product_variants
               SET reserved_count = GREATEST(reserved_count + {delta}, 0)
               WHERE id = {variantId}",
            ct);
    }

    /// <summary>
    /// مصرف رزروهای سفارش پس از پرداخت موفق:
    /// انبار مارکت + آفر فروشندگان + تنوع‌ها، در یک دستور (کسر stock، فروش sales_count).
    /// </summary>
    public static async Task<bool> ConsumeOrderAsync(ApplicationDbContext db, int orderId, CancellationToken ct)
    {
        // انبار مارکت
        var marketRows = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE products p
               SET stock = p.stock - r.quantity,
                   reserved_count = p.reserved_count - r.quantity,
                   sales_count = p.sales_count + r.quantity
               FROM stock_reservations r
               WHERE r.product_id = p.id
                 AND r.order_id = {orderId}
                 AND r.offer_id IS NULL AND r.variant_id IS NULL
                 AND r.status = 1
                 AND p.stock >= r.quantity",
            ct);

        // آفرهای فروشندگان
        var offerRows = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE seller_offers o
               SET stock = o.stock - r.quantity,
                   reserved_count = o.reserved_count - r.quantity
               FROM stock_reservations r
               WHERE r.offer_id = o.id
                 AND r.order_id = {orderId}
                 AND r.status = 1
                 AND o.stock >= r.quantity",
            ct);

        // تنوع‌ها
        var variantRows = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE product_variants v
               SET stock = v.stock - r.quantity,
                   reserved_count = v.reserved_count - r.quantity
               FROM stock_reservations r
               WHERE r.variant_id = v.id
                 AND r.order_id = {orderId}
                 AND r.status = 1
                 AND v.stock >= r.quantity",
            ct);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE stock_reservations
               SET status = 2
               WHERE order_id = {orderId} AND status = 1",
            ct);

        return marketRows > 0 || offerRows > 0 || variantRows > 0;
    }

    /// <summary>آزادسازی رزروهای منقضی (انبار مارکت + آفرها + تنوع‌ها).</summary>
    public static async Task<int> ReleaseExpiredAsync(ApplicationDbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlAsync(
            $"""
             UPDATE products p
             SET reserved_count = GREATEST(p.reserved_count - r.q, 0)
             FROM (
                 SELECT product_id, SUM(quantity) AS q
                 FROM stock_reservations
                 WHERE status = 1 AND offer_id IS NULL AND variant_id IS NULL AND expires_at <= now()
                 GROUP BY product_id
             ) r
             WHERE r.product_id = p.id
             """,
            ct);

        await db.Database.ExecuteSqlAsync(
            $"""
             UPDATE seller_offers o
             SET reserved_count = GREATEST(o.reserved_count - r.q, 0)
             FROM (
                 SELECT offer_id, SUM(quantity) AS q
                 FROM stock_reservations
                 WHERE status = 1 AND offer_id IS NOT NULL AND expires_at <= now()
                 GROUP BY offer_id
             ) r
             WHERE r.offer_id = o.id
             """,
            ct);

        await db.Database.ExecuteSqlAsync(
            $"""
             UPDATE product_variants v
             SET reserved_count = GREATEST(v.reserved_count - r.q, 0)
             FROM (
                 SELECT variant_id, SUM(quantity) AS q
                 FROM stock_reservations
                 WHERE status = 1 AND variant_id IS NOT NULL AND expires_at <= now()
                 GROUP BY variant_id
             ) r
             WHERE r.variant_id = v.id
             """,
            ct);

        return await db.Database.ExecuteSqlAsync(
            $"""
             UPDATE stock_reservations
             SET status = 3, released_at = now()
             WHERE status = 1 AND expires_at <= now()
             """,
            ct);
    }

    /// <summary>برگشت موجودی پس از لغو سفارش (انبار مارکت + آفرها + تنوع‌ها).</summary>
    public static async Task RestockOrderAsync(ApplicationDbContext db, int orderId, CancellationToken ct)
    {
        // بازگشت به انبار مارکت
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE products p
               SET stock = p.stock + r.quantity,
                   sales_count = GREATEST(p.sales_count - r.quantity, 0)
               FROM stock_reservations r
               WHERE r.product_id = p.id
                 AND r.offer_id IS NULL AND r.variant_id IS NULL
                 AND r.order_id = {orderId}
                 AND r.status = 2",
            ct);

        // بازگشت به آفر فروشنده
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE seller_offers o
               SET stock = o.stock + r.quantity
               FROM stock_reservations r
               WHERE r.offer_id = o.id
                 AND r.order_id = {orderId}
                 AND r.status = 2",
            ct);

        // بازگشت به تنوع
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE product_variants v
               SET stock = v.stock + r.quantity
               FROM stock_reservations r
               WHERE r.variant_id = v.id
                 AND r.order_id = {orderId}
                 AND r.status = 2",
            ct);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE stock_reservations
               SET status = 3, released_at = now()
               WHERE order_id = {orderId} AND status IN (1, 2)",
            ct);
    }

    /// <summary>موجودی قابل فروش یک محصول (اتمیک در لحظه خواندن).</summary>
    public static async Task<int> GetAvailableAsync(ApplicationDbContext db, long productId, CancellationToken ct)
    {
        var result = await db.Database
            .SqlQuery<int>($@"SELECT GREATEST(stock - reserved_count, 0) AS ""Value"" FROM products WHERE id = {productId}")
            .FirstOrDefaultAsync(ct);
        return result;
    }
}
