using System.ComponentModel.DataAnnotations.Schema;
using Shop.Domain.Enums;

namespace Shop.Domain;

/// <summary>سفارش ثبت‌شده توسط کاربر.</summary>
public class Order
{
    public int Id { get; set; }

    /// <summary>شماره سفارش قابل نمایش به مشتری.</summary>
    public string OrderNumber { get; set; } = default!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    public OrderStatus Status { get; set; } = OrderStatus.AwaitingPayment;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Post;

    /// <summary>جمع کل اقلام (پیش از تخفیف) به ریال.</summary>
    public long Subtotal { get; set; }

    /// <summary>مجموع تخفیف محصولات + کد تخفیف به ریال.</summary>
    public long DiscountAmount { get; set; }

    /// <summary>مالیت بر ارزش افزوده (ریال).</summary>
    public long TaxAmount { get; set; }

    /// <summary>هزینه ارسال به ریال.</summary>
    public long ShippingCost { get; set; }

    /// <summary>مبلغ نهایی قابل پرداخت به ریال.</summary>
    public long Total { get; set; }

    /// <summary>کد تخفیف اعمال‌شده.</summary>
    public string? DiscountCode { get; set; }

    // ---- اطلاعات گیرنده ----
    public string ReceiverName { get; set; } = default!;
    public string Phone { get; set; } = default!;

    public int StateId { get; set; }
    public virtual State? State { get; set; }

    public int CityId { get; set; }
    public virtual City? City { get; set; }

    public string Address { get; set; } = default!;
    public string? PostalCode { get; set; }
    public string? Note { get; set; }

    /// <summary>توضیح لغو در صورت Canceled شدن.</summary>
    public string? CancelReason { get; set; }

    /// <summary>کد همکار فروش (افیلیت) که این سفارش به او منسوب است.</summary>
    public string? AffiliateCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }

    public virtual List<OrderItem> Items { get; set; } = [];
    public virtual List<Payment> Payments { get; set; } = [];
}

/// <summary>قلم سفارش با اسنپ‌شات قیمت لحظه ثبت.</summary>
public class OrderItem
{
    public long Id { get; set; }

    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = default!;

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    /// <summary>نام محصول هنگام خرید (اسنپ‌شات).</summary>
    public string ProductName { get; set; } = default!;

    /// <summary>اسنپ‌شات تصویر محصول.</summary>
    public string ProductImage { get; set; } = default!;

    /// <summary>قیمت واحد پس از تخفیف (ریال).</summary>
    public long UnitPrice { get; set; }

    public byte DiscountPercent { get; set; }
    public int Quantity { get; set; }

    /// <summary>اسنپ‌شات فروشنده برای تسویه مارکت‌پلیس (null = مارکت).</summary>
    public long? SellerId { get; set; }

    /// <summary>نام فروشنده هنگام خرید (اسنپ‌شات).</summary>
    public string? SellerName { get; set; }

    /// <summary>پیشنهاد فروش (برای تسویه با فروشنده و گزارش مارکت‌پلیس).</summary>
    public long? OfferId { get; set; }

    /// <summary>تنوع انتخابی (رنگ/سایز).</summary>
    public long? VariantId { get; set; }

    /// <summary>متن ویژگی‌های تنوع هنگام خرید (اسنپ‌شات): «آبی / XL».</summary>
    public string? VariantText { get; set; }

    [NotMapped]
    public long LineTotal => UnitPrice * Quantity;
}
