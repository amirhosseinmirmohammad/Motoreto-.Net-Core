namespace Shop.Domain;

/// <summary>نوع گردش انبار.</summary>
public enum StockMovementType
{
    /// <summary>ورود کالا (خرید/تولید)</summary>
    Purchase = 1,

    /// <summary>خروج کالا (فروش موفق)</summary>
    Sale = 2,

    /// <summary>برگشت از فروش (لغو سفارش)</summary>
    Return = 3,

    /// <summary>اصلاح موجودی توسط انباردار</summary>
    Adjustment = 4
}

/// <summary>
/// کارت انبار: هر تغییر موجودی یک ردیف اینجاست (دفتر کل انبار).
/// همیشه بعد از تغییر اتمیک موجودی نوشته می‌شود تا تاریخچه کامل باشد.
/// </summary>
public class StockMovement
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public StockMovementType Type { get; set; }

    /// <summary>تغییر موجودی: مثبت = ورود، منفی = خروج.</summary>
    public int Delta { get; set; }

    /// <summary>موجودی پس از این گردش (اسنپ‌شات مانده).</summary>
    public int StockAfter { get; set; }

    /// <summary>ارجاع (مثلاً شماره سفارش).</summary>
    public string? Reference { get; set; }

    public string? Note { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
