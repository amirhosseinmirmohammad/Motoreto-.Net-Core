namespace Shop.Application.DTOs;

/// <summary>قیمت یک فروشنده برای همان کالا (نمودار مقایسه فروشندگان).</summary>
public class SellerPriceDto
{
    public long ProductId { get; set; }
    public string SellerName { get; set; } = "مارکت";
    public string Slug { get; set; } = default!;
    public long FinalPrice { get; set; }
    public int Stock { get; set; }
    public byte DiscountPercent { get; set; }
    public bool IsCheapest { get; set; }
}
