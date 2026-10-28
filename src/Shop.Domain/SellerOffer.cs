using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Domain;

/// <summary>
/// پیشنهاد فروش یک فروشنده برای یک کالای کاتالوگ (هر کالا یک‌بار ثبت می‌شود؛
/// هر فروشنده قیمت/موجودی/گارانتی خودش را جداگانه ثبت می‌کند — مدل مارکت‌پلیس).
/// </summary>
public class SellerOffer
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public long SellerId { get; set; }
    public virtual Seller Seller { get; set; } = default!;

    /// <summary>قیمت فروش این فروشنده (ریال).</summary>
    public long Price { get; set; }

    public byte DiscountPercent { get; set; }

    /// <summary>موجودی این فروشنده.</summary>
    public int Stock { get; set; }

    /// <summary>تعداد رزروشده در جریان تسویه (اتمیک).</summary>
    public int ReservedCount { get; set; }

    /// <summary>گارانتی پیشنهادی.</summary>
    public string? Warranty { get; set; }

    /// <summary>حداکثر روز تحویل.</summary>
    public int DeliveryDays { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public long FinalPrice => DiscountPercent > 0 ? Price - Price * DiscountPercent / 100 : Price;

    [NotMapped]
    public int AvailableToSell => Math.Max(0, Stock - ReservedCount);

    [NotMapped]
    public bool IsInStock => AvailableToSell > 0;
}
