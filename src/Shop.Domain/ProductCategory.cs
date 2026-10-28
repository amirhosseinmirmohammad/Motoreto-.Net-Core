namespace Shop.Domain;

/// <summary>دسته‌بندی‌های چندگانه محصول — یک محصول می‌تواند در چند دسته نمایش داده شود.</summary>
public class ProductCategory
{
    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = default!;
}

/// <summary>دسته‌هایی که فروشنده در آن‌ها فعالیت می‌کند — فقط کالاهای همین دسته‌ها را می‌بیند و آفر می‌زند.</summary>
public class SellerCategory
{
    public long SellerId { get; set; }
    public virtual Seller Seller { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = default!;
}
