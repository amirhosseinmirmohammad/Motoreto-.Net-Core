using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

/// <summary>خروجی رزرو موفق برای صفحه تسویه.</summary>
public class CheckoutReservationDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;

    /// <summary>زمان انقضای رزرو (UTC) برای شمارش معکوس.</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    public int ReservedMinutes { get; set; }
    public List<ReservedItemDto> Items { get; set; } = [];
    public long Subtotal { get; set; }
    public long ShippingCost { get; set; }
    public long Total { get; set; }
}

public class ReservedItemDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Image { get; set; } = default!;
    public int Quantity { get; set; }
    public long FinalPrice { get; set; }
    public long? OfferId { get; set; }
    public long? VariantId { get; set; }
}

/// <summary>
/// مدیریت رزرو موجودی محصولات (استاندارد مارکت‌پلیس):
/// شروع تسویه → رزرو با تایمر؛ پرداخت موفق → کسر از موجودی؛ انقضا → آزادسازی خودکار.
/// </summary>
public interface IStockReservationService
{
    /// <summary>رزرو (یا تمدید رزرو) اقلام سبد کاربر برای تسویه.</summary>
    Task<CheckoutReservationDto> ReserveForCheckoutAsync(Guid userId, CancellationToken ct = default);

    /// <summary>اطلاعات رزرو فعال کاربر (null = رزروی ندارد).</summary>
    Task<CheckoutReservationDto?> GetActiveAsync(Guid userId, CancellationToken ct = default);

    /// <summary>آزادسازی رزروهای کاربر (خروج از تسویه یا لغو).</summary>
    Task ReleaseUserReservationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// مصرف رزروهای سفارش پس از پرداخت موفق: کسر اتمیک Stock و ReservedCount.
    /// داخل تراکنش صدا زده می‌شود.
    /// </summary>
    Task<bool> ConsumeForOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>آزادسازی همه رزروهای منقضی (job پس‌زمینه). تعداد آزادشده برمی‌گردد.</summary>
    Task<int> ReleaseExpiredAsync(CancellationToken ct = default);
}
