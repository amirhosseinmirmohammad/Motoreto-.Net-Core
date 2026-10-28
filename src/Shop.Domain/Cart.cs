namespace Shop.Domain;

/// <summary>سبد خرید (کاربر عضو یا مهمان با توکن).</summary>
public class Cart
{
    public long Id { get; set; }

    /// <summary>شناسه کاربر عضو؛ برای سبد مهمان null است.</summary>
    public Guid? UserId { get; set; }

    /// <summary>توکن یکتای سبد مهمان (کوکی).</summary>
    public string? GuestToken { get; set; }

    public DateTime CreatedAt { get; set; }
    public virtual List<CartItem> Items { get; set; } = [];
}

/// <summary>قلم سبد خرید — در مارکت‌پلیس هر قلم می‌تواند از یک فروشنده مشخص باشد.</summary>
public class CartItem
{
    public long Id { get; set; }

    public long CartId { get; set; }
    public virtual Cart Cart { get; set; } = default!;

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    /// <summary>پیشنهاد فروشنده (null = کالای اصلی مارکت).</summary>
    public long? OfferId { get; set; }
    public virtual SellerOffer? Offer { get; set; }

    /// <summary>تنوع انتخابی (رنگ/سایز و...)</summary>
    public long? VariantId { get; set; }
    public virtual ProductVariant? Variant { get; set; }

    /// <summary>تعداد (حداقل ۱).</summary>
    public int Quantity { get; set; } = 1;
}
