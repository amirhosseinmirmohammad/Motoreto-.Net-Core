using Shop.Application.DTOs;
using Shop.Application.ViewModels;
using Shop.Domain;

namespace Shop.Application.Interfaces;

public interface IDiscountService
{
    Task<DiscountValidationResult> ValidateAsync(string code, long orderSubtotal, CancellationToken ct = default);

    /// <summary>بعد از پرداخت موفق، یک مصرف ثبت می‌کند.</summary>
    Task RegisterUsageAsync(string code, CancellationToken ct = default);

    Task<List<Discount>> GetAllAsync(CancellationToken ct = default);

    Task<Discount?> GetByIdAsync(int id, CancellationToken ct = default);

    Task CreateAsync(Discount discount, CancellationToken ct = default);

    Task UpdateAsync(Discount discount, CancellationToken ct = default);

    Task ToggleAsync(int id, CancellationToken ct = default);
}

public interface IOrderService
{
    /// <summary>
    /// ثبت سفارش بر اساس رزروهای فعال کاربر.
    /// اتمیک: سفارش + اقلام + اتصال رزروها در یک تراکنش.
    /// </summary>
    Task<PlaceOrderResult> PlaceOrderAsync(Guid userId, CheckoutViewModel model, CancellationToken ct = default);

    Task<PagedResult<OrderListItemDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    Task<OrderDetailDto?> GetOrderAsync(int orderId, Guid userId, CancellationToken ct = default);

    Task<OrderDetailDto?> GetOrderForAdminAsync(int orderId, CancellationToken ct = default);

    /// <summary>لغو سفارش: آزادی رزرو و در صورت پرداخت، بازگشت موجودی.</summary>
    Task<bool> CancelAsync(int orderId, Guid userId, string reason, CancellationToken ct = default);

    /// <summary>تغییر وضعیت توسط ادمین (با بازگشت موجودی هنگام لغو).</summary>
    Task<bool> UpdateStatusAsync(int orderId, Domain.Enums.OrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// نهایی‌سازی پس از پرداخت موفق (اتمیک): وضعیت Paid، مصرف رزروها، کسر موجودی، ثبت مصرف کد تخفیف.
    /// </summary>
    Task<bool> MarkPaidAsync(int orderId, string gatewayRefId, CancellationToken ct = default);

    /// <summary>ثبت شکست پرداخت.</summary>
    Task MarkPaymentFailedAsync(long paymentId, CancellationToken ct = default);

    Task<PagedResult<OrderListItemDto>> GetFilteredForAdminAsync(string? status, string? q, int page, int pageSize, CancellationToken ct = default);

    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
}

public class PlaceOrderResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public long? Total { get; set; }
}

/// <summary>خلاصه آماری داشبورد ادمین.</summary>
public class AdminDashboardDto
{
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public long TotalSales { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int PendingComments { get; set; }
    public int UnreadMessages { get; set; }
    public List<DailyPointDto> OrdersPerDay { get; set; } = [];
    public List<DailyPointDto> SalesPerDay { get; set; } = [];
    public List<OrderListItemDto> RecentOrders { get; set; } = [];
    public List<LowStockDto> LowStockProducts { get; set; } = [];
}

public class DailyPointDto
{
    public string Label { get; set; } = default!;
    public int Value { get; set; }
    public long Amount { get; set; }
}

public class LowStockDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = default!;
    public int Stock { get; set; }
}
