using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Domain;

/// <summary>محصول فروشگاه.</summary>
public class Product
{
    public long Id { get; set; }

    /// <summary>نام فارسی محصول.</summary>
    public string Name { get; set; } = default!;

    /// <summary>نام انگلیسی / برند.</summary>
    public string EnglishName { get; set; } = default!;

    /// <summary>اسلاگ یکتا برای URL.</summary>
    public string Slug { get; set; } = default!;

    /// <summary>عنوان سئو (خالی = نام محصول).</summary>
    public string? MetaTitle { get; set; }

    /// <summary>توضیح متا برای موتورهای جستجو و هوش مصنوعی — حداکثر ~۱۶۰ کاراکتر.</summary>
    public string? MetaDescription { get; set; }

    /// <summary>کد کالا.</summary>
    public string Code { get; set; } = default!;

    /// <summary>قیمت واحد به ریال.</summary>
    public long UnitPrice { get; set; }

    /// <summary>درصد تخفیف (۰ تا ۹۰).</summary>
    public byte DiscountPercent { get; set; }

    /// <summary>موجودی انبار (فیزیکی).</summary>
    public int Stock { get; set; }

    /// <summary>تعداد رزروشده در جریان تسویه (اتمیک با SQL). قابل فروش = Stock - ReservedCount.</summary>
    public int ReservedCount { get; set; }

    /// <summary>توضیحات کامل (HTML ساده).</summary>
    public string Description { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = default!;

    /// <summary>پیشنهادهای فروش فروشندگان مارکت‌پلیس برای همین کالا.</summary>
    public virtual List<SellerOffer> Offers { get; set; } = [];

    /// <summary>
    /// ویژگی‌های قابل انتخاب محصول به‌صورت JSON:
    /// [{"name":"رنگ","values":["آبی","قرمز"]},{"name":"سایز","values":["L","XL"]}]
    /// </summary>
    public string? AttributesJson { get; set; }

    /// <summary>برند محصول (ویژگی عمومی — جدای ویژگی‌های دسته‌بندی).</summary>
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public virtual List<ProductVariant> Variants { get; set; } = [];

    /// <summary>تصویر اصلی محصول.</summary>
    public string MainImage { get; set; } = default!;

    /// <summary>آیا در پیشنهاد شگفت‌انگیز نمایش داده شود؟</summary>
    public bool IsAmazing { get; set; }

    /// <summary>پایان مهلت پیشنهاد شگفت‌انگیز.</summary>
    public DateTime? AmazingEndsAt { get; set; }

    /// <summary>آیا محصول انتخاب دبیران / ویژه است؟</summary>
    public bool IsSpecial { get; set; }

    /// <summary>فعال و قابل نمایش در فروشگاه.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>تعداد فروش موفق (برای پرفروش‌ترین‌ها).</summary>
    public int SalesCount { get; set; }

    /// <summary>تعداد بازدید صفحه.</summary>
    public long ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual List<ProductImage> Images { get; set; } = [];
    public virtual List<Comment> Comments { get; set; } = [];
    public virtual List<OrderItem> OrderItems { get; set; } = [];
    public virtual List<CartItem> CartItems { get; set; } = [];
    public virtual List<Favorite> Favorites { get; set; } = [];

    /// <summary>قیمت نهایی پس از تخفیف (ریال). محاسبه‌ای و بدون اثر جانبی.</summary>
    [NotMapped]
    public long FinalPrice => DiscountPercent > 0
        ? UnitPrice - UnitPrice * DiscountPercent / 100
        : UnitPrice;

    /// <summary>مقدار تخفیف به ریال.</summary>
    [NotMapped]
    public long DiscountAmount => UnitPrice - FinalPrice;

    /// <summary>آیا قابل خرید است؟</summary>
    [NotMapped]
    public bool IsInStock => Stock - ReservedCount > 0;

    /// <summary>تعداد قابل فروش الان.</summary>
    [NotMapped]
    public int AvailableToSell => Math.Max(0, Stock - ReservedCount);
}
