namespace Shop.Domain.Enums;

/// <summary>وضعیت سفارش در چرخه حیات فروشگاه.</summary>
public enum OrderStatus
{
    /// <summary>در انتظار پرداخت</summary>
    AwaitingPayment = 1,

    /// <summary>پرداخت شده</summary>
    Paid = 2,

    /// <summary>در حال پردازش (بسته‌بندی)</summary>
    Processing = 3,

    /// <summary>ارسال شده</summary>
    Shipped = 4,

    /// <summary>تحویل داده شده</summary>
    Delivered = 5,

    /// <summary>لغو شده</summary>
    Canceled = 6
}

/// <summary>نتیجه یک تراکنش پرداخت.</summary>
public enum PaymentStatus
{
    /// <summary>در انتظار نتیجه درگاه</summary>
    Pending = 1,

    /// <summary>پرداخت موفق</summary>
    Success = 2,

    /// <summary>پرداخت ناموفق / لغو شده</summary>
    Failed = 3
}

/// <summary>روش ارسال سفارش.</summary>
public enum ShippingMethod
{
    /// <summary>پست پیشتاز (اقتصادی)</summary>
    Post = 1,

    /// <summary>ارسال اکسپرس (تیپاکس)</summary>
    Express = 2
}

/// <summary>روش پرداخت سفارش.</summary>
public enum PaymentMethod
{
    /// <summary>پرداخت اینترنتی</summary>
    Online = 1,

    /// <summary>پرداخت در محل (کد به کد)</summary>
    OnDelivery = 2
}
