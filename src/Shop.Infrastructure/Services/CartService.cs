using Shop.Domain;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

/// <summary>
/// سبد خرید دیتابیسی — همه تغییرات اتمیک و مقاوم به همزمانی.
/// هر قلم: کالا + (اختیاری) پیشنهاد فروشنده + (اختیاری) تنوع.
/// </summary>
public class CartService(ApplicationDbContext db, ISettingsService settings) : ICartService
{
    public async Task<CartSummaryDto> GetSummaryAsync(Guid? userId, string? guestToken, CancellationToken ct = default)
    {
        var cartId = await FindCartIdAsync(userId, guestToken, ct);
        if (cartId is null) return new CartSummaryDto();

        return await BuildSummaryAsync(cartId.Value, ct);
    }

    public async Task<CartOperationResult> AddItemAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, int quantity, CancellationToken ct = default)
    {
        var config = await settings.GetAsync(ct);
        quantity = Math.Clamp(quantity, 1, Math.Max(1, config.MaxQuantityPerProduct));

        // منبع موجودی: آفر فروشنده > تنوع > انبار مارکت
        long? srcOfferId = null, srcVariantId = null;
        int available;

        if (offerId is not null)
        {
            var offer = await db.SellerOffers
                .AsNoTracking()
                .Where(o => o.Id == offerId && o.IsActive && o.ProductId == productId
                            && o.Seller.Status == SellerStatus.Approved && o.Seller.IsActive)
                .Select(o => new { o.Id, Available = o.Stock - o.ReservedCount })
                .FirstOrDefaultAsync(ct);

            if (offer is null) return Fail("این پیشنهاد فروش در دسترس نیست.");
            srcOfferId = offer.Id;
            available = offer.Available;
        }
        else if (variantId is not null)
        {
            var variant = await db.ProductVariants
                .AsNoTracking()
                .Where(v => v.Id == variantId && v.IsActive && v.ProductId == productId)
                .Select(v => new { v.Id, Available = v.Stock - v.ReservedCount })
                .FirstOrDefaultAsync(ct);

            if (variant is null) return Fail("این ترکیب ویژگی‌ها در دسترس نیست.");
            srcVariantId = variant.Id;
            available = variant.Available;
        }
        else
        {
            var product = await db.Products
                .AsNoTracking()
                .Where(p => p.Id == productId && p.IsActive)
                .Select(p => new { p.Id, Available = p.Stock - p.ReservedCount })
                .FirstOrDefaultAsync(ct);

            if (product is null)
                return Fail("محصول یافت نشد یا غیرفعال است.");
            available = product.Available;
        }

        if (available < 1)
            return Fail("موجودی این کالا تمام شده است.");

        quantity = Math.Min(quantity, available);
        var cartId = await GetOrCreateCartIdAsync(userId, guestToken, ct);

        return await Transactional.RunAsync(db, async token =>
        {
            var updated = await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE cart_items
                   SET quantity = LEAST(quantity + {quantity}, {config.MaxQuantityPerProduct})
                   WHERE cart_id = {cartId} AND product_id = {productId}
                     AND offer_id IS NOT DISTINCT FROM {srcOfferId}
                     AND variant_id IS NOT DISTINCT FROM {srcVariantId}",
                token);

            if (updated == 0)
            {
                db.CartItems.Add(new Domain.CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    OfferId = srcOfferId,
                    VariantId = srcVariantId,
                    Quantity = quantity
                });
                try
                {
                    await db.SaveChangesAsync(token);
                }
                catch (DbUpdateException)
                {
                    // رقابت: قلم همزمان توسط درخواست دیگر درج شده → آپدیت اتمیک
                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE cart_items
                           SET quantity = LEAST(quantity + {quantity}, {config.MaxQuantityPerProduct})
                           WHERE cart_id = {cartId} AND product_id = {productId}
                             AND offer_id IS NOT DISTINCT FROM {srcOfferId}
                             AND variant_id IS NOT DISTINCT FROM {srcVariantId}",
                        token);
                }
            }

            var cart = await BuildSummaryAsync(cartId, token);
            return new CartOperationResult
            {
                Success = true,
                Message = "به سبد خرید اضافه شد.",
                Cart = cart
            };
        }, ct);
    }

    public async Task<CartOperationResult> UpdateQuantityAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, int quantity, CancellationToken ct = default)
    {
        var config = await settings.GetAsync(ct);
        var cartId = await FindCartIdAsync(userId, guestToken, ct);
        if (cartId is null) return Fail("سبد خرید یافت نشد.");

        if (quantity <= 0)
            return await RemoveItemInternalAsync(cartId.Value, productId, offerId, variantId, ct);

        quantity = Math.Clamp(quantity, 1, Math.Max(1, config.MaxQuantityPerProduct));

        // سقف = موجودی منبع
        var sourceStock = await db.Database
            .SqlQuery<int>($@"
                SELECT COALESCE((
                    CASE
                        WHEN {offerId} IS NOT NULL THEN (SELECT stock - reserved_count FROM seller_offers WHERE id = {offerId.Value})
                        WHEN {variantId} IS NOT NULL THEN (SELECT stock - reserved_count FROM product_variants WHERE id = {variantId.Value})
                        ELSE (SELECT stock - reserved_count FROM products WHERE id = {productId})
                    END
                ), 0) AS ""Value""")
            .FirstOrDefaultAsync(ct);

        quantity = Math.Clamp(quantity, 1, Math.Max(1, Math.Min(config.MaxQuantityPerProduct, Math.Max(sourceStock, 1))));

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE cart_items
               SET quantity = {quantity}
               WHERE cart_id = {cartId} AND product_id = {productId}
                 AND offer_id IS NOT DISTINCT FROM {offerId}
                 AND variant_id IS NOT DISTINCT FROM {variantId}",
            ct);

        var cart = await BuildSummaryAsync(cartId.Value, ct);
        return new CartOperationResult { Success = true, Message = "سبد به‌روزرسانی شد.", Cart = cart };
    }

    public async Task<CartOperationResult> RemoveItemAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, CancellationToken ct = default)
    {
        var cartId = await FindCartIdAsync(userId, guestToken, ct);
        if (cartId is null) return Fail("سبد خرید یافت نشد.");

        return await RemoveItemInternalAsync(cartId.Value, productId, offerId, variantId, ct);
    }

    public async Task ClearAsync(Guid? userId, string? guestToken, CancellationToken ct = default)
    {
        var cartId = await FindCartIdAsync(userId, guestToken, ct);
        if (cartId is null) return;

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM cart_items WHERE cart_id = {cartId.Value}", ct);
    }

    public async Task MergeGuestCartAsync(string guestToken, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(guestToken)) return;

        var config = await settings.GetAsync(ct);

        await Transactional.RunAsync(db, async token =>
        {
            var guestCart = await db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.GuestToken == guestToken && c.UserId == null, token);

            if (guestCart is null || guestCart.Items.Count == 0)
            {
                if (guestCart is not null) db.Carts.Remove(guestCart);
                await db.SaveChangesAsync(token);
                return;
            }

            var userCartId = await GetOrCreateCartIdAsync(userId, null, token);

            foreach (var item in guestCart.Items)
            {
                var updated = await db.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE cart_items
                       SET quantity = LEAST(quantity + {item.Quantity}, {config.MaxQuantityPerProduct})
                       WHERE cart_id = {userCartId} AND product_id = {item.ProductId}
                         AND offer_id IS NOT DISTINCT FROM {item.OfferId}
                         AND variant_id IS NOT DISTINCT FROM {item.VariantId}",
                    token);

                if (updated == 0)
                {
                    item.CartId = userCartId;
                }
            }

            db.Carts.Remove(guestCart);
            await db.SaveChangesAsync(token);
        }, ct);
    }

    /// <summary>پاکسازی سبد کاربر (پس از پرداخت موفق).</summary>
    public async Task ClearByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM cart_items
               WHERE cart_id IN (SELECT id FROM carts WHERE user_id = {userId})",
            ct);
    }

    // ---- private ----

    private async Task<long?> FindCartIdAsync(Guid? userId, string? guestToken, CancellationToken ct)
    {
        if (userId is not null)
        {
            return await db.Carts
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => (long?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        if (!string.IsNullOrWhiteSpace(guestToken))
        {
            return await db.Carts
                .AsNoTracking()
                .Where(c => c.GuestToken == guestToken && c.UserId == null)
                .Select(c => (long?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        return null;
    }

    private async Task<long> GetOrCreateCartIdAsync(Guid? userId, string? guestToken, CancellationToken ct)
    {
        var existing = await FindCartIdAsync(userId, guestToken, ct);
        if (existing is not null) return existing.Value;

        var cart = new Domain.Cart
        {
            UserId = userId,
            GuestToken = userId is null ? guestToken : null,
            CreatedAt = DateTime.UtcNow
        };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);
        return cart.Id;
    }

    private async Task<CartSummaryDto> BuildSummaryAsync(long cartId, CancellationToken ct)
    {
        var items = await db.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cartId)
            .Select(i => new CartItemDto
            {
                ItemId = i.Id,
                ProductId = i.ProductId,
                Name = i.Product.Name,
                Slug = i.Product.Slug,
                Image = i.Variant != null && i.Variant.Image != null ? i.Variant.Image : i.Product.MainImage,
                OfferId = i.OfferId,
                VariantId = i.VariantId,
                SellerName = i.Offer != null ? i.Offer.Seller.StoreName : "مارکت",
                UnitPrice = i.Offer != null ? i.Offer.Price
                    : i.Variant != null ? i.Variant.EffectiveBasePrice
                    : i.Product.UnitPrice,
                DiscountPercent = i.Offer != null ? i.Offer.DiscountPercent
                    : i.Variant != null ? i.Variant.DiscountPercent
                    : i.Product.DiscountPercent,
                FinalPrice = i.Offer != null
                    ? (i.Offer.DiscountPercent > 0 ? i.Offer.Price - i.Offer.Price * i.Offer.DiscountPercent / 100 : i.Offer.Price)
                    : i.Variant != null ? i.Variant.FinalPrice
                    : (i.Product.DiscountPercent > 0 ? i.Product.UnitPrice - i.Product.UnitPrice * i.Product.DiscountPercent / 100 : i.Product.UnitPrice),
                Quantity = i.Quantity,
                Stock = i.Offer != null ? i.Offer.Stock - i.Offer.ReservedCount
                    : i.Variant != null ? i.Variant.Stock - i.Variant.ReservedCount
                    : i.Product.Stock - i.Product.ReservedCount
            })
            .ToListAsync(ct);

        return new CartSummaryDto
        {
            Items = items,
            Subtotal = items.Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = items.Sum(i => i.DiscountAmount),
            TotalCount = items.Sum(i => i.Quantity)
        };
    }

    private async Task<CartOperationResult> RemoveItemInternalAsync(long cartId, long productId, long? offerId, long? variantId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM cart_items
               WHERE cart_id = {cartId} AND product_id = {productId}
                 AND offer_id IS NOT DISTINCT FROM {offerId}
                 AND variant_id IS NOT DISTINCT FROM {variantId}",
            ct);

        var cart = await BuildSummaryAsync(cartId, ct);
        return new CartOperationResult { Success = true, Message = "کالا از سبد حذف شد.", Cart = cart };
    }

    private static CartOperationResult Fail(string message) =>
        new() { Success = false, Message = message };
}
