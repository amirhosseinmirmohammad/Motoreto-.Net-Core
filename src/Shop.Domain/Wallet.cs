using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Domain;

/// <summary>نوع گردش کیف پول.</summary>
public enum WalletTransactionType
{
    /// <summary>هدیه ثبت‌نام</summary>
    RegistrationGift = 1,

    /// <summary>پاداش معرفی (به معرف)</summary>
    ReferralReward = 2,

    /// <summary>بازگشت وجه لغو سفارش</summary>
    OrderRefund = 3,

    /// <summary>خرید با کیف پول</summary>
    OrderPayment = 4,

    /// <summary>شارژ دستی توسط ادمین</summary>
    AdminCharge = 5
}

/// <summary>گردش کیف پول کاربر.</summary>
public class WalletTransaction
{
    public long Id { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>مبلغ: مثبت = واریز، منفی = برداشت (ریال).</summary>
    public long Amount { get; set; }

    public WalletTransactionType Type { get; set; }

    /// <summary>مانده پس از این گردش.</summary>
    public long BalanceAfter { get; set; }

    public int? OrderId { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>کد یک‌بارمصرف پیامکی.</summary>
public class OtpCode
{
    public long Id { get; set; }

    /// <summary>شماره موبایل.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(15)]
    public string Mobile { get; set; } = default!;

    [System.ComponentModel.DataAnnotations.StringLength(6)]
    public string Code { get; set; } = default!;

    public bool IsUsed { get; set; }

    /// <summary>تعداد تلاش ناموفق.</summary>
    public int FailedAttempts { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>دقیقه ۲ اعتبار دارد.</summary>
    public DateTime ExpiresAt { get; set; }
}
