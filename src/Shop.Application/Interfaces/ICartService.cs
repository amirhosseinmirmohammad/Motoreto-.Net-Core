using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

/// <summary>مدیریت سبد خرید دیتابیسی (کاربر عضو یا مهمان با توکن).</summary>
public interface ICartService
{
    Task<CartSummaryDto> GetSummaryAsync(Guid? userId, string? guestToken, CancellationToken ct = default);

    /// <summary>افزودن کالا؛ در مارکت‌پلیس با offerId (فروشنده خاص) و/یا variantId (ترکیب ویژگی‌ها).</summary>
    Task<CartOperationResult> AddItemAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, int quantity, CancellationToken ct = default);

    Task<CartOperationResult> UpdateQuantityAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, int quantity, CancellationToken ct = default);

    Task<CartOperationResult> RemoveItemAsync(Guid? userId, string? guestToken, long productId, long? offerId, long? variantId, CancellationToken ct = default);

    Task ClearAsync(Guid? userId, string? guestToken, CancellationToken ct = default);

    /// <summary>ادغام سبد مهمان در سبد کاربر بعد از ورود.</summary>
    Task MergeGuestCartAsync(string guestToken, Guid userId, CancellationToken ct = default);

    /// <summary>خالی کردن سبد کاربر عضو (پس از پرداخت موفق).</summary>
    Task ClearByUserAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>نتیجه عملیات سبد (برای JSON API).</summary>
public class CartOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public CartSummaryDto? Cart { get; set; }
}
