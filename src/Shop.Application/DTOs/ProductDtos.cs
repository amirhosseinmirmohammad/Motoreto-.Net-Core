namespace Shop.Application.DTOs;

/// <summary>خروجی صفحه‌بندی شده عمومی.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>فیلترهای لیست محصولات.</summary>
public class ProductFilter
{
    public string? Q { get; set; }

    /// <summary>اسلاگ دسته (خود دسته یا زیر‌دسته).</summary>
    public string? CategorySlug { get; set; }

    /// <summary>newest | cheap | expensive | discount | bestseller</summary>
    public string Sort { get; set; } = "newest";

    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; }
    public bool OnlyDiscounted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;

    /// <summary>اسلاگ برندهای انتخاب‌شده (فیلتر برند).</summary>
    public List<string>? BrandSlugs { get; set; }

    /// <summary>فیلتر ویژگی‌های داینامیک دسته‌بندی (تعریف ادمین — ساختار باز).</summary>
    public List<AttributeFilter>? Attributes { get; set; }
}

/// <summary>فیلتر یک ویژگی داینامیک: نام ویژگی + مقادیر انتخابی (OR داخل مقادیر، AND بین ویژگی‌ها).</summary>
public class AttributeFilter
{
    public string Name { get; set; } = default!;
    public List<string> Values { get; set; } = [];
}

/// <summary>کارت محصول در لیست‌ها.</summary>
public class ProductCardDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string EnglishName { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Image { get; set; } = default!;
    public long UnitPrice { get; set; }
    public long FinalPrice { get; set; }
    public byte DiscountPercent { get; set; }
    public int Stock { get; set; }
    public bool IsAmazing { get; set; }
    public DateTime? AmazingEndsAt { get; set; }
    public bool IsSpecial { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string CategorySlug { get; set; } = default!;

    /// <summary>برند (ویژگی عمومی محصول).</summary>
    public string? BrandName { get; set; }
    public string? BrandSlug { get; set; }

    /// <summary>ویژگی‌های محصول — آرایه‌ای از گروه‌ها: [{"name":"برند","values":[..]}] (ساختار باز ادمین-تعریف).</summary>
    public string? AttributesJson { get; set; }

    public double AverageRating { get; set; }
    public int CommentCount { get; set; }
    public int SalesCount { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>ارزان‌ترین پیشنهاد فروشندگان مارکت‌پلیس (null = فقط مارکت).</summary>
    public long? MinOfferPrice { get; set; }

    /// <summary>تعداد فروشندگان فعال این کالا.</summary>
    public int OfferCount { get; set; }
}

// ---------------- Facets سایدبار فیلترها (الگوی مارکت‌پلیس) ----------------

public class ProductFacetsDto
{
    public List<BrandFacetDto> Brands { get; set; } = [];
    public List<AttributeFacetDto> Attributes { get; set; } = [];
}

public class BrandFacetDto
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public int Count { get; set; }
}

public class AttributeFacetDto
{
    /// <summary>نام ویژگی — مثلا «حافظه داخلی» (تعریف ادمین روی دسته‌بندی).</summary>
    public string Name { get; set; } = default!;

    public List<AttributeValueFacetDto> Values { get; set; } = [];
}

public class AttributeValueFacetDto
{
    public string Value { get; set; } = default!;
    public int Count { get; set; }
}

/// <summary>گروه ویژگی محصول — ساختار ذخیره‌سازی: [{"name":"...","values":["..."]}] (کلیدها lowercase).</summary>
public class ProductAttributeGroupDto
{
    public string Name { get; set; } = default!;
    public List<string>? Values { get; set; }
}

// ---------------- جزئیات محصول ----------------

public class ProductDetailDto
{
    public ProductCardDto Card { get; set; } = default!;
    public string EnglishName { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
    public long ViewCount { get; set; }
    public string? AttributesJson { get; set; }
    public List<BreadCrumbItem> BreadCrumbs { get; set; } = [];
    public int CategoryId { get; set; }
    public List<string> Images { get; set; } = [];
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class BreadCrumbItem
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
}
