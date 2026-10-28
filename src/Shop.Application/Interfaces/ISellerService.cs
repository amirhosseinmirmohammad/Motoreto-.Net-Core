using Shop.Application.DTOs;
using Shop.Application.ViewModels;
using Shop.Domain;

namespace Shop.Application.Interfaces;

/// <summary>مدیریت فروشندگان مارکت‌پلیس: ثبت‌نام → تأیید ادمین → ثبت پیشنهاد فروش.</summary>
public interface ISellerService
{
    Task<(bool Success, string Message)> ApplyAsync(Guid userId, SellerApplyViewModel model, CancellationToken ct = default);

    Task<Seller?> GetByUserAsync(Guid userId, CancellationToken ct = default);

    Task UpdateAsync(Seller seller, CancellationToken ct = default);

    Task<Seller?> GetByIdAsync(long id, CancellationToken ct = default);

    Task<List<Seller>> GetAllForAdminAsync(SellerStatus? status, CancellationToken ct = default);

    /// <summary>تأیید فروشگاه — پس از این، پیشنهادهای فروش او فعال می‌شوند.</summary>
    Task ApproveAsync(long sellerId, int commissionPercent, CancellationToken ct = default);

    Task RejectAsync(long sellerId, string reason, CancellationToken ct = default);

    // ---- پیشنهادهای فروش (Offers) ----

    Task<List<SellerOfferDto>> GetMyOffersAsync(Guid userId, CancellationToken ct = default);

    /// <summary>افزودن/به‌روزرسانی پیشنهاد فروش روی کالای موجود کاتالوگ.</summary>
    Task<(bool Success, string Message)> SaveOfferAsync(Guid userId, SellerOfferInput input, CancellationToken ct = default);

    Task<(bool Success, string Message)> ToggleOfferAsync(Guid userId, long offerId, CancellationToken ct = default);

    /// <summary>پیشنهاد کالای جدید توسط فروشنده (تا تأیید ادمین غیرفعال است).</summary>
    Task<(bool Success, string Message, long ProductId)> ProposeProductAsync(Guid userId, AdminProductViewModel model, CancellationToken ct = default);

    /// <summary>پیشنهادهای فعال یک کالا (برای صفحه محصول و نمودار مقایسه فروشندگان).</summary>
    Task<List<SellerOfferDto>> GetProductOffersAsync(long productId, CancellationToken ct = default);
}

public interface ICompareService
{
    (bool Added, string Message) Toggle(string? currentIds, long productId, out string newIds);

    Task<List<ProductCardDto>> GetCardsAsync(long[] productIds, CancellationToken ct = default);
}

/// <summary>تاریخچه قیمت + انبارداری.</summary>
public interface IPriceHistoryService
{
    /// <summary>ثبت نقطه قیمت (هنگام ایجاد/تغییر قیمت).</summary>
    Task RecordAsync(long productId, long finalPrice, CancellationToken ct = default);

    Task<List<PricePointDto>> GetHistoryAsync(long productId, CancellationToken ct = default);

    /// <summary>پیشنهادهای فعال یک کالا مرتب بر اساس قیمت (نمودار فروشندگان).</summary>
    Task<List<SellerOfferDto>> GetSellerPricesAsync(long productId, CancellationToken ct = default);
}

/// <summary>
/// انبارداری: هر تغییر موجودی از این سرویس عبور می‌کند تا کارت انبار (دفتر کل) کامل بماند.
/// </summary>
public interface IInventoryService
{
    /// <summary>ورود کالا / اصلاح / برگشت — تغییر اتمیک + ثبت در کارت انبار.</summary>
    Task<(bool Success, string Message)> AdjustAsync(long productId, int delta, StockMovementType type, string? note, Guid? byUserId, string? reference = null, CancellationToken ct = default);

    /// <summary>ثبت موجودی دقیق (اختلاف به‌صورت Adjustment ثبت می‌شود).</summary>
    Task<(bool Success, string Message)> SetStockAsync(long productId, int newStock, Guid? byUserId, CancellationToken ct = default);

    Task<PagedResult<StockMovementDto>> GetMovementsAsync(long? productId, StockMovementType? type, int page, int pageSize, CancellationToken ct = default);

    Task<List<StockMovementDto>> GetRecentForProductAsync(long productId, int count, CancellationToken ct = default);
}
