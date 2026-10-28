namespace Shop.Application.DTOs;

/// <summary>گره منوی دسته‌بندی (درختی).</summary>
public class CategoryNodeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Icon { get; set; }
    public List<CategoryNodeDto> Children { get; set; } = [];
}

/// <summary>قلم سبد خرید.</summary>
public class CartItemDto
{
    public long ItemId { get; set; }
    public long ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Image { get; set; } = default!;

    /// <summary>پیشنهاد فروشنده (null = مارکت).</summary>
    public long? OfferId { get; set; }
    public string SellerName { get; set; } = "مارکت";

    /// <summary>تنوع انتخابی (رنگ/سایز).</summary>
    public long? VariantId { get; set; }
    public string? VariantText { get; set; }

    public long UnitPrice { get; set; }
    public long FinalPrice { get; set; }
    public byte DiscountPercent { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public long LineTotal => FinalPrice * Quantity;
    public long DiscountAmount => (UnitPrice - FinalPrice) * Quantity;
}

/// <summary>سبد خرید + خلاصه مالی.</summary>
public class CartSummaryDto
{
    public List<CartItemDto> Items { get; set; } = [];
    public long Subtotal { get; set; }
    public long DiscountAmount { get; set; }
    public int TotalCount { get; set; }
    public bool IsEmpty => Items.Count == 0;
}

/// <summary>نتیجه اعتبارسنجی کد تخفیف.</summary>
public class DiscountValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? Code { get; set; }
    public long DiscountAmount { get; set; }
}

/// <summary>خلاصه سفارش برای نمایش.</summary>
public class OrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string StatusTitle { get; set; } = default!;
    public string StatusBadgeClass { get; set; } = default!;
    public long Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemsCount { get; set; }
    public string FirstItemImage { get; set; } = default!;
}

/// <summary>جزئیات کامل سفارش.</summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string StatusTitle { get; set; } = default!;
    public string StatusBadgeClass { get; set; } = default!;
    public string PaymentMethodTitle { get; set; } = default!;
    public string ShippingMethodTitle { get; set; } = default!;
    public long Subtotal { get; set; }
    public long DiscountAmount { get; set; }
    public long ShippingCost { get; set; }
    public long TaxAmount { get; set; }
    public long Total { get; set; }
    public string? DiscountCode { get; set; }
    public string ReceiverName { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string StateName { get; set; } = default!;
    public string CityName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string? PostalCode { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Image { get; set; } = default!;
    public long UnitPrice { get; set; }
    public byte DiscountPercent { get; set; }
    public int Quantity { get; set; }
    public long LineTotal { get; set; }
    public string? VariantText { get; set; }
    public string SellerName { get; set; } = "مارکت";
}

public static class OrderTitles
{
    public static string StatusTitle(Domain.Enums.OrderStatus status) => status switch
    {
        Domain.Enums.OrderStatus.AwaitingPayment => "در انتظار پرداخت",
        Domain.Enums.OrderStatus.Paid => "پرداخت شده",
        Domain.Enums.OrderStatus.Processing => "در حال پردازش",
        Domain.Enums.OrderStatus.Shipped => "ارسال شده",
        Domain.Enums.OrderStatus.Delivered => "تحویل شده",
        Domain.Enums.OrderStatus.Canceled => "لغو شده",
        _ => "نامشخص"
    };

    public static string StatusBadgeClass(Domain.Enums.OrderStatus status) => status switch
    {
        Domain.Enums.OrderStatus.AwaitingPayment => "warning",
        Domain.Enums.OrderStatus.Paid => "info",
        Domain.Enums.OrderStatus.Processing => "primary",
        Domain.Enums.OrderStatus.Shipped => "dark",
        Domain.Enums.OrderStatus.Delivered => "success",
        Domain.Enums.OrderStatus.Canceled => "danger",
        _ => "secondary"
    };
}
