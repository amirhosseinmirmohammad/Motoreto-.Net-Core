using Shop.Domain.Enums;

namespace Shop.Domain;

/// <summary>تراکنش پرداخت از طریق درگاه.</summary>
public class Payment
{
    public long Id { get; set; }

    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = default!;

    public long Amount { get; set; }

    /// <summary>نام درگاه پرداخت (مثلاً Fake، Zarinpal، ...).</summary>
    public string Gateway { get; set; } = default!;

    /// <summary>شناسه یکتای تراکنش سمت فروشگاه.</summary>
    public string Authority { get; set; } = default!;

    /// <summary>شناسه پیگیری سمت درگاه.</summary>
    public string? RefId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
