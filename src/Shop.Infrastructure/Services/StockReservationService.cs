using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

/// <summary>
/// رزرو موجودی در جریان تسویه (الگوی مارکت‌پلیس):
/// شروع تسویه → رزرو اتمیک با تایمر → پرداخت موفق → مصرف؛ انقضا → آزادسازی توسط job.
/// </summary>
public class StockReservationService(
    ApplicationDbContext db,
    ISettingsService settings,
    ILogger<StockReservationService> logger) : IStockReservationService
{
    public async Task<CheckoutReservationDto> ReserveForCheckoutAsync(Guid userId, CancellationToken ct = default)
    {
        var config = await settings.GetAsync(ct);
        var expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(5, config.ReservationMinutes));

        // اقلام سبد کاربر (با قیمت بر اساس منبع: آفر / تنوع / مارکت)
        var cartItems = await db.CartItems
            .AsNoTracking()
            .Where(i => i.Cart!.UserId == userId)
            .Select(i => new
            {
                i.ProductId,
                i.OfferId,
                i.VariantId,
                i.Quantity,
                i.Product.Name,
                i.Product.MainImage,
                Available = i.Offer != null ? i.Offer.Stock - i.Offer.ReservedCount
                    : i.Variant != null ? i.Variant.Stock - i.Variant.ReservedCount
                    : i.Product.Stock - i.Product.ReservedCount,
                FinalPrice = i.Offer != null
                    ? (i.Offer.DiscountPercent > 0 ? i.Offer.Price - i.Offer.Price * i.Offer.DiscountPercent / 100 : i.Offer.Price)
                    : i.Variant != null ? i.Variant.FinalPrice
                    : (i.Product.DiscountPercent > 0 ? i.Product.UnitPrice - i.Product.UnitPrice * i.Product.DiscountPercent / 100 : i.Product.UnitPrice)
            })
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            return new CheckoutReservationDto { Success = false, Message = "سبد خرید شما خالی است." };

        return await Transactional.RunAsync(db, async token =>
        {
            // رزروهای منقضی کاربر را اول آزاد کن
            await ReleaseUserExpiredAsync(db, userId, token);

            var reserved = new List<ReservedItemDto>();

            foreach (var item in cartItems)
            {
                var qty = Math.Clamp(item.Quantity, 1, Math.Max(1, config.MaxQuantityPerProduct));
                long? reservationOfferId = null, reservationVariantId = null;

                if (item.OfferId is not null)
                {
                    // ---- رزرو از آفر فروشنده ----
                    var alreadyReserved = await db.StockReservations
                        .Where(r => r.UserId == userId && r.OfferId == item.OfferId && r.Status == Domain.ReservationStatus.Active)
                        .SumAsync(r => (int?)r.Quantity, token) ?? 0;

                    var needed = qty - alreadyReserved;
                    if (needed > 0)
                    {
                        var ok = await StockSql.TryReserveOfferAsync(db, item.OfferId.Value, needed, token);
                        if (!ok)
                        {
                            return new CheckoutReservationDto
                            {
                                Success = false,
                                Message = $"موجودی «{item.Name}» نزد فروشنده کافی نیست. لطفاً سبد خرید را ویرایش کنید."
                            };
                        }
                    }

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE stock_reservations
                           SET quantity = {qty}, expires_at = {expiresAt}
                           WHERE user_id = {userId} AND offer_id = {item.OfferId} AND status = 1",
                        token);

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"INSERT INTO stock_reservations (product_id, user_id, offer_id, quantity, status, created_at, expires_at)
                           SELECT {item.ProductId}, {userId}, {item.OfferId}, {qty}, 1, {DateTime.UtcNow}, {expiresAt}
                           WHERE NOT EXISTS (
                               SELECT 1 FROM stock_reservations
                               WHERE user_id = {userId} AND offer_id = {item.OfferId} AND status = 1)",
                        token);

                    reservationOfferId = item.OfferId;
                }
                else if (item.VariantId is not null)
                {
                    // ---- رزرو از تنوع ----
                    var alreadyReserved = await db.StockReservations
                        .Where(r => r.UserId == userId && r.VariantId == item.VariantId && r.Status == Domain.ReservationStatus.Active)
                        .SumAsync(r => (int?)r.Quantity, token) ?? 0;

                    var needed = qty - alreadyReserved;
                    if (needed > 0)
                    {
                        var ok = await StockSql.TryReserveVariantAsync(db, item.VariantId.Value, needed, token);
                        if (!ok)
                        {
                            return new CheckoutReservationDto
                            {
                                Success = false,
                                Message = $"موجودی «{item.Name}» در ترکیب انتخابی کافی نیست."
                            };
                        }
                    }

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE stock_reservations
                           SET quantity = {qty}, expires_at = {expiresAt}
                           WHERE user_id = {userId} AND variant_id = {item.VariantId} AND status = 1",
                        token);

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"INSERT INTO stock_reservations (product_id, user_id, variant_id, quantity, status, created_at, expires_at)
                           SELECT {item.ProductId}, {userId}, {item.VariantId}, {qty}, 1, {DateTime.UtcNow}, {expiresAt}
                           WHERE NOT EXISTS (
                               SELECT 1 FROM stock_reservations
                               WHERE user_id = {userId} AND variant_id = {item.VariantId} AND status = 1)",
                        token);

                    reservationVariantId = item.VariantId;
                }
                else
                {
                    // ---- رزرو از انبار مارکت ----
                    var alreadyReserved = await db.StockReservations
                        .Where(r => r.UserId == userId && r.ProductId == item.ProductId && r.OfferId == null && r.VariantId == null && r.Status == Domain.ReservationStatus.Active)
                        .SumAsync(r => (int?)r.Quantity, token) ?? 0;

                    var needed = qty - alreadyReserved;
                    if (needed > 0)
                    {
                        var ok = await StockSql.TryReserveAsync(db, item.ProductId, needed, token);
                        if (!ok)
                        {
                            return new CheckoutReservationDto
                            {
                                Success = false,
                                Message = $"موجودی «{item.Name}» کافی نیست. لطفاً سبد خرید را ویرایش کنید."
                            };
                        }
                    }

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE stock_reservations
                           SET quantity = {qty}, expires_at = {expiresAt}
                           WHERE user_id = {userId} AND product_id = {item.ProductId} AND offer_id IS NULL AND variant_id IS NULL AND status = 1",
                        token);

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"INSERT INTO stock_reservations (product_id, user_id, quantity, status, created_at, expires_at)
                           SELECT {item.ProductId}, {userId}, {qty}, 1, {DateTime.UtcNow}, {expiresAt}
                           WHERE NOT EXISTS (
                               SELECT 1 FROM stock_reservations
                               WHERE user_id = {userId} AND product_id = {item.ProductId} AND offer_id IS NULL AND variant_id IS NULL AND status = 1)",
                        token);
                }

                reserved.Add(new ReservedItemDto
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Image = item.MainImage,
                    Quantity = qty,
                    FinalPrice = item.FinalPrice,
                    OfferId = reservationOfferId,
                    VariantId = reservationVariantId
                });
            }

            var subtotal = reserved.Sum(r => r.FinalPrice * r.Quantity);
            var shipping = subtotal >= config.FreeShippingThreshold && config.FreeShippingThreshold > 0
                ? 0
                : config.ShippingPostCost;

            return new CheckoutReservationDto
            {
                Success = true,
                ExpiresAtUtc = expiresAt,
                ReservedMinutes = Math.Max(5, config.ReservationMinutes),
                Items = reserved,
                Subtotal = subtotal,
                ShippingCost = shipping,
                Total = subtotal + shipping
            };
        }, ct);
    }

    public async Task<CheckoutReservationDto?> GetActiveAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var reservations = await db.StockReservations
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.Status == Domain.ReservationStatus.Active && r.ExpiresAt > now)
            .Select(r => new
            {
                r.ProductId,
                r.Quantity,
                r.ExpiresAt,
                r.Product.Name,
                r.Product.MainImage,
                FinalPrice = r.Product.DiscountPercent > 0
                    ? r.Product.UnitPrice - r.Product.UnitPrice * r.Product.DiscountPercent / 100
                    : r.Product.UnitPrice
            })
            .ToListAsync(ct);

        if (reservations.Count == 0) return null;

        var config = await settings.GetAsync(ct);
        var subtotal = reservations.Sum(r => r.FinalPrice * r.Quantity);
        var shipping = subtotal >= config.FreeShippingThreshold && config.FreeShippingThreshold > 0
            ? 0
            : config.ShippingPostCost;

        return new CheckoutReservationDto
        {
            Success = true,
            ExpiresAtUtc = reservations.Min(r => r.ExpiresAt),
            ReservedMinutes = Math.Max(5, config.ReservationMinutes),
            Items = reservations.Select(r => new ReservedItemDto
            {
                ProductId = r.ProductId,
                Name = r.Name,
                Image = r.MainImage,
                Quantity = r.Quantity,
                FinalPrice = r.FinalPrice
            }).ToList(),
            Subtotal = subtotal,
            ShippingCost = shipping,
            Total = subtotal + shipping
        };
    }

    public async Task ReleaseUserReservationsAsync(Guid userId, CancellationToken ct = default)
    {
        await Transactional.RunAsync(db, async token =>
        {
            await ReleaseUserExpiredAsync(db, userId, token);

            var actives = await db.StockReservations
                .Where(r => r.UserId == userId && r.Status == Domain.ReservationStatus.Active)
                .ToListAsync(token);

            foreach (var group in actives.Where(r => r.OfferId is null && r.VariantId is null).GroupBy(r => r.ProductId))
            {
                await StockSql.ChangeReservedAsync(db, group.Key, -group.Sum(r => r.Quantity), token);
            }

            foreach (var group in actives.Where(r => r.OfferId is not null).GroupBy(r => r.OfferId))
            {
                await StockSql.ChangeOfferReservedAsync(db, group.Key!.Value, -group.Sum(r => r.Quantity), token);
            }

            foreach (var group in actives.Where(r => r.VariantId is not null).GroupBy(r => r.VariantId))
            {
                await StockSql.ChangeVariantReservedAsync(db, group.Key!.Value, -group.Sum(r => r.Quantity), token);
            }

            await db.StockReservations
                .Where(r => r.UserId == userId && r.Status == Domain.ReservationStatus.Active)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Status, Domain.ReservationStatus.Released)
                    .SetProperty(r => r.ReleasedAt, DateTime.UtcNow), token);
        }, ct);
    }

    public Task<bool> ConsumeForOrderAsync(int orderId, CancellationToken ct = default)
        => StockSql.ConsumeOrderAsync(db, orderId, ct);

    public async Task<int> ReleaseExpiredAsync(CancellationToken ct = default)
    {
        var released = await Transactional.RunAsync(db,
            token => StockSql.ReleaseExpiredAsync(db, token), ct);

        if (released > 0)
            logger.LogInformation("رزرو منقضی: {Count} رزرو آزاد شد", released);

        return released;
    }

    // ---- private ----

    /// <summary>آزادسازی رزروهای منقضیِ یک کاربر خاص (قبل از رزرو مجدد).</summary>
    private static async Task ReleaseUserExpiredAsync(ApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var expired = await db.StockReservations
            .Where(r => r.UserId == userId && r.Status == Domain.ReservationStatus.Active && r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var group in expired.GroupBy(r => r.ProductId))
        {
            await StockSql.ChangeReservedAsync(db, group.Key, -group.Sum(r => r.Quantity), ct);
        }

        foreach (var group in expired.Where(r => r.OfferId is not null).GroupBy(r => r.OfferId))
        {
            await StockSql.ChangeOfferReservedAsync(db, group.Key!.Value, -group.Sum(r => r.Quantity), ct);
        }

        foreach (var group in expired.Where(r => r.VariantId is not null).GroupBy(r => r.VariantId))
        {
            await StockSql.ChangeVariantReservedAsync(db, group.Key!.Value, -group.Sum(r => r.Quantity), ct);
        }

        if (expired.Count > 0)
        {
            await db.StockReservations
                .Where(r => r.UserId == userId && r.Status == Domain.ReservationStatus.Active && r.ExpiresAt <= DateTime.UtcNow)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Status, Domain.ReservationStatus.Released)
                    .SetProperty(r => r.ReleasedAt, DateTime.UtcNow), ct);
        }
    }
}
