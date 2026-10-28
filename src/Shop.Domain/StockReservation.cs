namespace Shop.Domain;

/// <summary>وضعیت رزرو موجودی.</summary>
public enum ReservationStatus
{
    /// <summary>فعال (در جریان تسویه)</summary>
    Active = 1,

    /// <summary>مصرف شده (خرید نهایی شد)</summary>
    Consumed = 2,

    /// <summary>آزاد شده (منقضی یا لغو)</summary>
    Released = 3
}

/// <summary>
/// رزرو موجودی محصول در جریان تسویه سفارش (مثل فروشگاه‌های مارکت‌پلیس).
/// بعد از انقضا توسط StockReservationReleaseJob خودکار آزاد می‌شود.
/// </summary>
public class StockReservation
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>سفارش مربوطه (بعد از ثبت سفارش).</summary>
    public int? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    /// <summary>اگر رزرو روی پیشنهاد فروشنده باشد (null = انبار مارکت).</summary>
    public long? OfferId { get; set; }
    public virtual SellerOffer? Offer { get; set; }

    /// <summary>اگر رزرو روی یک تنوع خاص باشد.</summary>
    public long? VariantId { get; set; }
    public virtual ProductVariant? Variant { get; set; }

    /// <summary>تعداد رزروشده.</summary>
    public int Quantity { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Active;

    public DateTime CreatedAt { get; set; }

    /// <summary>مهلت رزرو؛ پس از آن کار باید آزاد شود.</summary>
    public DateTime ExpiresAt { get; set; }

    public DateTime? ReleasedAt { get; set; }
}
