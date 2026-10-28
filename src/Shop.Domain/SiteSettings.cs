namespace Shop.Domain;

/// <summary>
/// تنظیمات فروشگاه (رکورد سینگلتون Id=1).
/// ماژولار: با تغییر این رکورد، هر فروشگاهی بدون تغییر کد شخصی‌سازی می‌شود.
/// </summary>
public class SiteSettings
{
    public int Id { get; set; }

    /// <summary>نام فروشگاه.</summary>
    public string StoreName { get; set; } = default!;

    /// <summary>شعار / توضیح کوتاه زیر لوگو.</summary>
    public string? Tagline { get; set; }

    public string? LogoUrl { get; set; }

    /// <summary>رنگ اصلی قالب (hex مثل #ef4056).</summary>
    public string PrimaryColor { get; set; } = "#ef4056";

    public string SupportPhone { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Address { get; set; }

    public string? InstagramUrl { get; set; }
    public string? TelegramUrl { get; set; }

    /// <summary>متن صفحه درباره ما.</summary>
    public string? AboutUs { get; set; }

    /// <summary>متن صفحه تماس با ما.</summary>
    public string? ContactUs { get; set; }

    // ---- قوانین ارسال (ریال) ----
    /// <summary>هزینه ثابت ارسال عادی.</summary>
    public long ShippingPostCost { get; set; } = 490_000;

    /// <summary>هزینه ارسال اکسپرس.</summary>
    public long ShippingExpressCost { get; set; } = 990_000;

    /// <summary>سقف خرید رایگان ارسال عادی (۰ = غیرفعال).</summary>
    public long FreeShippingThreshold { get; set; } = 50_000_000;

    // ---- تنظیمات رزرو موجودی و سبد (قابل تغییر از پنل ادمین) ----
    /// <summary>مدت رزرو موجودی در جریان تسویه (دقیقه).</summary>
    public int ReservationMinutes { get; set; } = 20;

    /// <summary>حداکثر تعداد قابل خرید هر محصول در یک سفارش.</summary>
    public int MaxQuantityPerProduct { get; set; } = 5;

    /// <summary>آستانه هشدار «تنها X عدد باقی مانده».</summary>
    public int LowStockThreshold { get; set; } = 5;

    /// <summary>فاصله اجرای پاکسازی رزروهای منقضی (ثانیه).</summary>
    public int ReservationCleanupIntervalSeconds { get; set; } = 60;

    /// <summary>درصد مالیات بر ارزش افزوده (پیش‌فرض ۹٪ قانون ایران — قابل تغییر از پنل).</summary>
    public int TaxPercent { get; set; } = 9;

    // ---- زمان‌بندی پیشنهاد شگفت‌انگیز ----

    /// <summary>زمان‌بندی شگفت‌انگیز فعال باشد؟ خاموش = آیتم‌های شگفت‌انگیز همیشه نمایش داده می‌شوند.</summary>
    public bool AmazingScheduleEnabled { get; set; }

    /// <summary>ساعت شروع بازه شگفت‌انگیز (دقیقه از نیمه‌شب به وقت ایران — ۵۴۰ = ۰۹:۰۰).</summary>
    public int AmazingStartMinutes { get; set; } = 540;

    /// <summary>ساعت پایان بازه شگفت‌انگیز (دقیقه از نیمه‌شب به وقت ایران — ۱۳۲۰ = ۲۲:۰۰). کمتر از شروع = بازه شبانه.</summary>
    public int AmazingEndMinutes { get; set; } = 1320;

    /// <summary>نام درگاه پرداخت فعال (Fake / Zarinpal / ...).</summary>
    public string PaymentGateway { get; set; } = "Fake";

    /// <summary>کلید مرچنت درگاه (زرین‌پال و...).</summary>
    public string? PaymentMerchantId { get; set; }

    /// <summary>حالت آزمایشی درگاه (Sandbox).</summary>
    public bool PaymentSandbox { get; set; } = true;

    /// <summary>آدرس فرانت‌اند (برای CORS وقتی API روی سابدامین جدا است).</summary>
    public string? FrontendUrl { get; set; }

    /// <summary>دامین‌های اضافه مجاز برای CORS (جداشده با کاما).</summary>
    public string? AdditionalCorsOrigins { get; set; }

    /// <summary>آیا فروشگاه در دسترس باشد؟ (حالت تعمیر و نگهداری)</summary>
    public bool IsMaintenanceMode { get; set; }
}

/// <summary>مشترک خبرنامه.</summary>
public class NewsLetterSubscriber
{
    public int Id { get; set; }

    /// <summary>ایمیل یکتا.</summary>
    public string Email { get; set; } = default!;

    public DateTime SubscribedAt { get; set; }
}

/// <summary>پیام ارسالی از فرم تماس با ما.</summary>
public class SiteMessage
{
    public long Id { get; set; }

    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>علاقه‌مندی (لیست علاقه‌مندی کاربر).</summary>
public class Favorite
{
    public long Id { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
