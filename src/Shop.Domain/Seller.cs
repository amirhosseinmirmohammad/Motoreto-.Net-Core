namespace Shop.Domain;

/// <summary>نوع شخصیت فروشنده.</summary>
public enum SellerType
{
    /// <summary>شخص حقیقی</summary>
    Individual = 1,

    /// <summary>شخص حقوقی (شرکت)</summary>
    LegalEntity = 2
}

/// <summary>وضعیت فروشنده در چرخه تأیید ادمین.</summary>
public enum SellerStatus
{
    /// <summary>در انتظار بررسی و تأیید ادمین</summary>
    PendingApproval = 1,

    /// <summary>تأییدشده — مجاز به فروش</summary>
    Approved = 2,

    /// <summary>ردشده</summary>
    Rejected = 3
}

/// <summary>
/// فروشگاه فروشنده در مارکت‌پلیس با اطلاعات کامل هویتی/حقوقی و مدارک کسب‌وکار.
/// تا زمان تأیید ادمین محصولاتش به فروش نمی‌رسد.
/// </summary>
public class Seller
{
    public long Id { get; set; }

    public string StoreName { get; set; } = default!;
    public string Slug { get; set; } = default!;

    public SellerType Type { get; set; } = SellerType.Individual;

    public Guid OwnerUserId { get; set; }
    public virtual User Owner { get; set; } = default!;

    // ---- اطلاعات تماس و فروشگاه ----
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    /// <summary>شماره شبا برای تسویه فروش (IR + ۲۴ رقم).</summary>
    public string? Sheba { get; set; }

    // ---- شخص حقیقی ----
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    /// <summary>کد ملی ۱۰ رقمی (حقیقی یا مدیرعامل حقوقی).</summary>
    [System.ComponentModel.DataAnnotations.StringLength(10)]
    public string? NationalCode { get; set; }

    // ---- شخص حقوقی ----
    public string? CompanyName { get; set; }

    /// <summary>شناسه ملی ۱۱ رقمی شرکت.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(11)]
    public string? CompanyNationalId { get; set; }

    /// <summary>کد اقتصادی.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string? EconomicCode { get; set; }

    /// <summary>شماره ثبت شرکت.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string? RegistrationNumber { get; set; }

    public string? CeoName { get; set; }

    /// <summary>کد ملی مدیرعامل.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(10)]
    public string? CeoNationalId { get; set; }

    // ---- مدارک (آپلود فایل جداگانه) ----

    /// <summary>شماره جواز کسب / شماره ثبت شرکت.</summary>
    public string BusinessLicenseNumber { get; set; } = default!;

    /// <summary>جواز کسب / آگهی ثبت شرکت.</summary>
    public string? BusinessLicenseFileUrl { get; set; }

    /// <summary>کارت ملی (حقیقی یا مدیرعامل).</summary>
    public string? NationalCardFileUrl { get; set; }

    /// <summary>روزنامه رسمی شرکت (اختیاری، حقوقی).</summary>
    public string? CompanyAdFileUrl { get; set; }

    // ---- وضعیت ----
    // ---- ارسال (هر فروشنده برای خودش) ----

    /// <summary>عنوان روش ارسال فروشنده (مثلا «پست پیشتاز» یا «پیک تهران»).</summary>
    [System.ComponentModel.DataAnnotations.StringLength(80)]
    public string? ShippingTitle { get; set; }

    /// <summary>حداقل روز کاری تحویل.</summary>
    public int DeliveryDays { get; set; } = 2;

    /// <summary>حداکثر روز کاری تحویل.</summary>
    public int DeliveryDaysMax { get; set; } = 4;
        // ---- هزینه‌های جانبی هر فروشنده ----

    /// <summary>هزینه ارسال فروشنده (null = پیش‌فرض مارکت).</summary>
    public long? ShippingCostOverride { get; set; }

    /// <summary>سقف ارسال رایگان فروشنده (null = پیش‌فرض مارکت).</summary>
    public long? FreeShippingThresholdOverride { get; set; }

    /// <summary>درصد مالیات فروشنده.</summary>
    public int TaxPercent { get; set; } = 9;

    /// <summary>هزینه خدمات / بسته‌بندی هر سفارش.</summary>
    public long ServiceFee { get; set; }

    public SellerStatus Status { get; set; } = SellerStatus.PendingApproval;
    public string? RejectReason { get; set; }
    public int CommissionPercent { get; set; } = 5;
    public bool IsActive { get; set; } = true;

    /// <summary>روش‌های پرداخت پذیرفته فروشگاه: online,cash,cod (جداشده با کاما).</summary>
    public string? PaymentMethods { get; set; }

    /// <summary>موقعیت جغرافیایی فروشگاه (نمایش روی نقشه).</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public virtual List<SellerOffer> Offers { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string OwnerFullName => Type == SellerType.LegalEntity
        ? CompanyName ?? StoreName
        : $"{FirstName} {LastName}".Trim();
}
