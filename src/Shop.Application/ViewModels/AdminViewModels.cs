using System.ComponentModel.DataAnnotations;

namespace Shop.Application.ViewModels;

/// <summary>مدیریت محصول در پنل ادمین.</summary>
public class AdminProductViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "نام محصول الزامی است.")]
    [StringLength(200)]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "نام انگلیسی الزامی است.")]
    [StringLength(200)]
    public string EnglishName { get; set; } = default!;

    /// <summary>خالی = خودکار از نام ساخته می‌شود.</summary>
    [StringLength(220)]
    public string? Slug { get; set; }

    [StringLength(50)]
    public string? Code { get; set; }

    [Required(ErrorMessage = "قیمت الزامی است.")]
    [Range(0, long.MaxValue, ErrorMessage = "قیمت نامعتبر است.")]
    public long UnitPrice { get; set; }

    [Range(0, 90, ErrorMessage = "درصد تخفیف باید ۰ تا ۹۰ باشد.")]
    public byte DiscountPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required(ErrorMessage = "توضیحات الزامی است.")]
    public string Description { get; set; } = default!;

    [Required(ErrorMessage = "دسته را انتخاب کنید.")]
    public int CategoryId { get; set; }

    /// <summary>برند محصول (اختیاری).</summary>
    public int? BrandId { get; set; }

    /// <summary>ویژگی‌های داینامیک — نام‌ها (موازی با مقادیر).</summary>
    public List<string>? AttrNames { get; set; }

    /// <summary>ویژگی‌های داینامیک — مقادیر (چندتایی با ویرگول).</summary>
    public List<string>? AttrValues { get; set; }

    /// <summary>عنوان سئو (خالی = نام محصول).</summary>
    public string? MetaTitle { get; set; }

    /// <summary>توضیح متا (~۱۶۰ کاراکتر).</summary>
    public string? MetaDescription { get; set; }

    /// <summary>فروشنده مارکت‌پلیس (خالی = کالای اصلی مارکت).</summary>
    public long? SellerId { get; set; }

    /// <summary>مسیر تصویر (یا آپلود فایل).</summary>
    public string? MainImage { get; set; }

    public bool IsAmazing { get; set; }
    public DateTime? AmazingEndsAt { get; set; }
    public bool IsSpecial { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>آدرس‌های گالری (هر خط یک آدرس) — اختیاری.</summary>
    public string? GalleryUrls { get; set; }
}

/// <summary>مدیریت دسته‌بندی در پنل ادمین.</summary>
public class AdminCategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام دسته الزامی است.")]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(120)]
    public string? Slug { get; set; }

    /// <summary>کلاس آیکون Bootstrap Icons مثل «bi-phone».</summary>
    [StringLength(50)]
    public string? Icon { get; set; }

    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
