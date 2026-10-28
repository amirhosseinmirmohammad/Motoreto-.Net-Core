namespace Shop.Domain;

/// <summary>
/// یک نقطه از تاریخچه قیمت محصول (قیمت نهایی پس از تخفیف، ریال).
/// هنگام تغییر قیمت (ایجاد/ویرایش) ثبت می‌شود و نمودار روند قیمت را می‌سازد.
/// </summary>
public class PriceHistory
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    /// <summary>قیمت نهایی به ریال.</summary>
    public long Price { get; set; }

    public DateTime RecordedAt { get; set; }
}
