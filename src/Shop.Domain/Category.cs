namespace Shop.Domain;

/// <summary>دسته‌بندی محصولات (درختی، دو سطحی مثل فروشگاه‌های مارکت‌پلیس).</summary>
public class Category
{
    public int Id { get; set; }

    /// <summary>نام فارسی دسته.</summary>
    public string Name { get; set; } = default!;

    /// <summary>اسلاگ یکتا برای SEO.</summary>
    public string Slug { get; set; } = default!;

    /// <summary>کلاس آیکون Bootstrap Icons.</summary>
    public string? Icon { get; set; }

    public int? ParentId { get; set; }
    public virtual Category? Parent { get; set; }
    public virtual List<Category> Children { get; set; } = [];
    public virtual List<Product> Products { get; set; } = [];

    /// <summary>ترتیب نمایش در منو.</summary>
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
