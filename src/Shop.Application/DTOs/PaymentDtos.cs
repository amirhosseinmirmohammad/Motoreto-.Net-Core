namespace Shop.Application.DTOs;

/// <summary>درخواست پرداخت از سمت فروشگاه به درگاه.</summary>
public class PaymentRequest
{
    public int OrderId { get; set; }
    public long Amount { get; set; }
    public string Description { get; set; } = default!;
    public string CallbackUrl { get; set; } = default!;
}

/// <summary>پاسخ درگاه به درخواست پرداخت.</summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;

    /// <summary>شناسه یکتای تراکنش سمت فروشگاه.</summary>
    public string? Authority { get; set; }

    /// <summary>آدرس صفحه پرداخت درگاه (null = خطا).</summary>
    public string? RedirectUrl { get; set; }
}

/// <summary>نتیجه تأیید پرداخت.</summary>
public class PaymentVerificationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? RefId { get; set; }
}

/// <summary>
/// اینترفیس درگاه پرداخت — ماژولار:
/// برای اتصال هر درگاه واقعی (زرین‌پال، آیدی‌پی و...) فقط یک پیاده‌سازی جدید لازم است.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>نام درگاه برای ثبت در تراکنش.</summary>
    string Name { get; }

    Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default);

    Task<PaymentVerificationResult> VerifyAsync(string authority, bool customerApproved, CancellationToken ct = default);
}
