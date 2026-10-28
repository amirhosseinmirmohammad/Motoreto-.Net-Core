namespace Shop.Domain;

/// <summary>گالری تصاویر محصول.</summary>
public class ProductImage
{
    public long Id { get; set; }

    /// <summary>مسیر نسبی تصویر.</summary>
    public string Url { get; set; } = default!;

    /// <summary>متن جایگزین (SEO).</summary>
    public string? Alt { get; set; }

    public int SortOrder { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;
}
