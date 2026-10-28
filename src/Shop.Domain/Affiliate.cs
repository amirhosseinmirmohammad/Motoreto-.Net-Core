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

    /// <summary>تعداد ثبت‌نام‌های موفق از طریق لینک همکار (مدل CPA).</summary>
    public int RegistrationCount { get; set; }

    /// <summary>تعداد سرنخ‌های ثبت‌شده (مدل CPL).</summary>
    public int LeadCount { get; set; }

    /// <summary>مجموع مبلغ سفارش‌های منسوب (تومان).</summary>
    public long OrderAmount { get; set; }

    /// <summary>مجموع کمیسیون کسب‌شده (تومان).</summary>
    public long Earned { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>مبلغ پرداخت‌شده تاکنون به همکار (تومان).</summary>
    public long PaidOut { get; set; }

    /// <summary>درخواست تسویه ثبت شده؟ (ادمین انجام می‌دهد)</summary>
    public DateTime? PayoutRequestedAt { get; set; }

    /// <summary>موجودی قابل تسویه = کسب‌شده − پرداخت‌شده.</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public long Balance => Math.Max(0, Earned - PaidOut);

    public DateTime CreatedAt { get; set; }
}

/// <summary>سرنخ (Lead) ثبت‌شده از طریق لندینگ همکار — مدل CPL.</summary>
public class AffiliateLead
{
    public long Id { get; set; }

    public int AffiliateId { get; set; }
    public virtual Affiliate Affiliate { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public string Phone { get; set; } = default!;

    /// <summary>تمایل خرید: دسته‌بندی مورد علاقه (اختیاری).</summary>
    public string? Interest { get; set; }

    /// <summary>تماس گرفته شد؟</summary>
    public bool IsContacted { get; set; }

    public DateTime CreatedAt { get; set; }
}
