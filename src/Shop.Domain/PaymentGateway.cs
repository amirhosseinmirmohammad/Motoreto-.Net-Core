namespace Shop.Domain;

/// <summary>درگاه پرداخت قابل تعریف از پنل ادمین.</summary>
public class PaymentGateway
{
    public int Id { get; set; }

    /// <summary>نام نمایشی (مثلاً زرین‌پال).</summary>
    public string Name { get; set; } = default!;

    /// <summary>نوع درگاه: Zarinpal / Zibal / IDPay / NextPay / Payping / Bale</summary>
    public string Type { get; set; } = default!;

    /// <summary>مرچنت آیدی / کد پذیرندگی.</summary>
    public string MerchantId { get; set; } = default!;

    /// <summary>کلید API / ترمینال.</summary>
    public string? TerminalKey { get; set; }

    /// <summary>کلید امنیتی / Secret.</summary>
    public string? SecretKey { get; set; }

    /// <summary>حالت آزمایشی.</summary>
    public bool IsSandbox { get; set; } = true;

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    /// <summary>درصد کارمزد درگاه (برای محاسبه سود مارکت).</summary>
    public decimal FeePercent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
