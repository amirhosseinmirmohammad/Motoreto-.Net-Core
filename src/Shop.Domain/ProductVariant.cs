using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop.Domain;

/// <summary>
/// تنوع محصول (SKU): یک ترکیب از ویژگی‌ها (رنگ، سایز و...) با قیمت/موجودی مستقل.
/// ویژگی‌ها و مقادیر مجاز روی خود محصول تعریف می‌شود و هر ترکیب یک ردیف اینجاست.
/// </summary>
public class ProductVariant
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    /// <summary>
    /// ویژگی‌های این تنوع به‌صورت JSON: [{name:"رنگ",value:"آبی"},{name:"سایز",value:"XL"}]
    /// </summary>
    public string AttributesJson { get; set; } = "{}";

    /// <summary>قیمت این تنوع (ریال) — null = قیمت محصول.</summary>
    public long? Price { get; set; }

    public byte DiscountPercent { get; set; }

    public int Stock { get; set; }

    public int ReservedCount { get; set; }

    /// <summary>تصویر اختصاصی تنوع (مثلاً رنگ آبی) — null = تصویر اصلی.</summary>
    public string? Image { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // ---- محاسبه‌ای ----

    [NotMapped]
    public long EffectiveBasePrice => Price ?? Product.UnitPrice;

    [NotMapped]
    public long FinalPrice => DiscountPercent > 0
        ? EffectiveBasePrice - EffectiveBasePrice * DiscountPercent / 100
        : EffectiveBasePrice;

    [NotMapped]
    public int AvailableToSell => Math.Max(0, Stock - ReservedCount);

    [NotMapped]
    public bool IsInStock => AvailableToSell > 0;

    /// <summary>متن خوانا: «آبی / XL».</summary>
    [NotMapped]
    public string AttributesText
    {
        get
        {
            try
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<VariantAttribute>>(AttributesJson)
                            ?? [];
                return string.Join(" / ", items.Select(a => a.Value));
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public sealed class VariantAttribute
    {
        public string Name { get; set; } = default!;
        public string Value { get; set; } = default!;
    }

    /// <summary>ساخت JSON ویژگی‌ها از جفت‌های نام/مقدار.</summary>
    public static string BuildAttributesJson(params (string Name, string Value)[] attributes) =>
        System.Text.Json.JsonSerializer.Serialize(
            attributes.Select(a => new VariantAttribute { Name = a.Name, Value = a.Value }).ToList());
}
