namespace Shop.Domain;

/// <summary>کد تخفیف سبد خرید.</summary>
public class Discount
{
    public int Id { get; set; }

    /// <summary>کد یکتا (بزرگ‌شده).</summary>
    public string Code { get; set; } = default!;

    public string Title { get; set; } = default!;

    /// <summary>درصد تخفیف (۱ تا ۱۰۰).</summary>
    public byte Percent { get; set; }

    /// <summary>حداکثر مبلغ تخفیف به ریال (null = بی‌نهایت).</summary>
    public long? MaxAmount { get; set; }

    /// <summary>حداقل مبلغ سفارش برای استفاده (ریال).</summary>
    public long MinOrderAmount { get; set; }

    /// <summary>سقف تعداد استفاده کلی (null = بی‌نهایت).</summary>
    public int? UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public DateTime? ExpireAt { get; set; }

    public bool IsActive { get; set; } = true;
}
