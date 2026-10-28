namespace Shop.Domain;

/// <summary>
/// ویژگیِ تعریف‌شده روی یک دسته‌بندی — توسط ادمین مدیریت می‌شود (دست باز).
/// مثال: دسته «موبایل» ← حافظه داخلی، دوربین، سیستم عامل؛ دسته «پوشاک» ← جنس، سایز، طرح.
/// </summary>
public class CategoryAttribute
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    /// <summary>عنوان ویژگی — مثلا «حافظه داخلی».</summary>
    public string Name { get; set; } = default!;

    /// <summary>در فیلتر کنار صفحه محصولات نمایش داده شود؟</summary>
    public bool ShowInFilter { get; set; } = true;

    /// <summary>این ویژگی در ترکیب واریانت‌ها (قیمت/موجودی مستقل) مشارکت کند؟ مثل رنگ و سایز.</summary>
    public bool IsVariantOption { get; set; }

    public int SortOrder { get; set; }
}
