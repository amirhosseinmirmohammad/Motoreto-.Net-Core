namespace Shop.Domain;

/// <summary>برند (به‌عنوان ویژگی عمومی محصول — فیلتر جدای ویژگی‌های دسته‌بندی).</summary>
public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;

    public List<Product> Products { get; set; } = [];
}

/// <summary>اتصال چند-به-چند برند ↔ دسته‌بندی — یک برند می‌تواند مال چند دسته باشد.</summary>
public class BrandCategory
{
    public int BrandId { get; set; }
    public virtual Brand Brand { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = default!;
}
