namespace Shop.Domain;

/// <summary>وضعیت آگهی دست دوم.</summary>
public enum ClassifiedAdStatus
{
    /// <summary>در انتظار تأیید ادمین</summary>
    Pending = 1,
    /// <summary>منتشر شده</summary>
    Approved = 2,
    /// <summary>رد شده</summary>
    Rejected = 3,
    /// <summary>فروخته/مسترد شده</summary>
    Sold = 4,
    /// <summary>منقضی</summary>
    Expired = 5
}

/// <summary>دسته‌بندی آگهی (مستقل از دسته‌های فروشگاه — املاک، وسایل نقلیه، موبایل و...).</summary>
public class ClassifiedCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    /// <summary>آیکون Bootstrap Icons (مثل bi-house).</summary>
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual List<ClassifiedAttribute> Attributes { get; set; } = [];
    public virtual List<ClassifiedAd> Ads { get; set; } = [];
}

/// <summary>ویژگی داینامیک هر دسته آگهی (متراژ، سال ساخت، برند و...).</summary>
public class ClassifiedAttribute
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public virtual ClassifiedCategory Category { get; set; } = default!;

    public string Name { get; set; } = default!;

    /// <summary>text / number / select / bool</summary>
    public string Type { get; set; } = "text";

    /// <summary>گزینه‌های select — جداشده با کاما.</summary>
    public string? Options { get; set; }

    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>آگهی دست دوم (شبیه دیوار): کاربر یا فروشگاه آگهی می‌گذارد، ادمین تأیید می‌کند.</summary>
public class ClassifiedAd
{
    public long Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    /// <summary>قیمت به تومان — null = توافقی، صفر = رایگان.</summary>
    public long? Price { get; set; }

    public int ClassifiedCategoryId { get; set; }
    public virtual ClassifiedCategory Category { get; set; } = default!;

    public int CityId { get; set; }
    public virtual City City { get; set; } = default!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>شماره تماس آگهی (پیش‌فرض از پروفایل).</summary>
    public string Phone { get; set; } = default!;

    public string? MainImage { get; set; }

    /// <summary>ویژگی‌های داینامیک دسته — [{"name":"متراژ","value":"۱۲۰"}]</summary>
    public string? AttributesJson { get; set; }

    public ClassifiedAdStatus Status { get; set; } = ClassifiedAdStatus.Pending;
    public string? RejectReason { get; set; }

    public long ViewCount { get; set; }
    /// <summary>نردبان‌شده (در بالای لیست).</summary>
    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; }
    /// <summary>تاریخ انقضای نمایش (پیش‌فرض +۳۰ روز).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>گالری تصاویر — URLهای جدا شده با کاما (MainImage تصویر اصلی است).</summary>
    public string? ImagesJson { get; set; }

    public virtual List<ClassifiedChatMessage> ChatMessages { get; set; } = [];
}

/// <summary>پیام چت روی یک آگهی (خریدار ↔ آگهی‌دهنده).</summary>
public class ClassifiedChatMessage
{
    public long Id { get; set; }
    public long AdId { get; set; }
    public virtual ClassifiedAd Ad { get; set; } = default!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    public string Text { get; set; } = default!;
    public bool IsFromOwner { get; set; }
    public DateTime CreatedAt { get; set; }
}
