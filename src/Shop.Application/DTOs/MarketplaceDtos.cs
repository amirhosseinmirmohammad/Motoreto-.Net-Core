using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace Shop.Application.DTOs;

/// <summary>یک نقطه از نمودار روند قیمت.</summary>
public class PricePointDto
{
    public DateTime Date { get; set; }
    public long Price { get; set; }
}

/// <summary>پیشنهاد فروش یک فروشنده برای یک کالا (جدول فروشندگان + نمودار قیمت).</summary>
public class SellerOfferDto
{
    public long OfferId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public long SellerId { get; set; }
    public string SellerName { get; set; } = default!;
    public string SellerSlug { get; set; } = default!;
    public long Price { get; set; }
    public long FinalPrice { get; set; }
    public byte DiscountPercent { get; set; }
    public int Available { get; set; }
    public string? Warranty { get; set; }
    public int DeliveryDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsCheapest { get; set; }
}

/// <summary>ردیف کارت انبار.</summary>
public class StockMovementDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string TypeTitle { get; set; } = default!;
    public string TypeClass { get; set; } = default!;
    public int Delta { get; set; }
    public int StockAfter { get; set; }
    public string? Reference { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>ورودی ثبت/ویرایش پیشنهاد فروش (اعتبارسنجی سمت سرور).</summary>
public class SellerOfferInput
{
    [Range(1, long.MaxValue, ErrorMessage = "کالا نامعتبر است.")]
    public long ProductId { get; set; }

    [Range(1000, long.MaxValue, ErrorMessage = "قیمت نامعتبر است.")]
    public long Price { get; set; }

    [Range(0, 90)]
    public byte DiscountPercent { get; set; }

    [Range(0, 100_000)]
    public int Stock { get; set; }

    [StringLength(150)]
    public string? Warranty { get; set; }

    [Range(1, 30)]
    public int DeliveryDays { get; set; } = 3;
}
