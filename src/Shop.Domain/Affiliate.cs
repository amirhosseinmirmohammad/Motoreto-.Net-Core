namespace Shop.Domain;

/// <summary>
/// همکار فروش (افیلیت): کد یکتا برای هر همکار؛ با لینک /aff/{code}، کوکی ۳۰ روزه
/// اتریبیوت می‌شود و درصدی از سفارش‌های منسوب به او کمیسیون ثبت می‌شود.
/// </summary>
public class Affiliate
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>کد یکتای همکار (مثلاً AFF8K2M1).</summary>
    public string Code { get; set; } = default!;

    /// <summary>درصد کمیسیون از مبلغ سفارش‌های منسوب.</summary>
    public int CommissionPercent { get; set; } = 3;

    public int ClickCount { get; set; }
    public int OrderCount { get; set; }

    /// <summary>مجموع مبلغ سفارش‌های منسوب (تومان).</summary>
    public long OrderAmount { get; set; }

    /// <summary>مجموع کمیسیون کسب‌شده (تومان).</summary>
    public long Earned { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
