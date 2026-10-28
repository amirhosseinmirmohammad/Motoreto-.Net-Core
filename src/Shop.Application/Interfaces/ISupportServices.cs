using Shop.Application.DTOs;
using Shop.Domain;

namespace Shop.Application.Interfaces;

/// <summary>دسترسی کش‌شده به تنظیمات فروشگاه.</summary>
public interface ISettingsService
{
    /// <summary>تنظیمات فعلی (کش‌شده با invalidation خودکار).</summary>
    Task<SiteSettings> GetAsync(CancellationToken ct = default);

    Task UpdateAsync(SiteSettings settings, CancellationToken ct = default);
}

public interface ISliderService
{
    Task<List<Slider>> GetActiveAsync(CancellationToken ct = default);

    Task<List<Slider>> GetAllAsync(CancellationToken ct = default);

    Task<Slider?> GetByIdAsync(int id, CancellationToken ct = default);

    Task CreateAsync(Slider slider, CancellationToken ct = default);

    Task UpdateAsync(Slider slider, CancellationToken ct = default);

    Task ToggleAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    void InvalidateCache();
}

public interface IFavoriteService
{
    /// <summary>تغییر وضعیت علاقه‌مندی؛ true = اضافه شد، false = حذف شد.</summary>
    Task<(bool Added, string Message)> ToggleAsync(Guid userId, long productId, CancellationToken ct = default);

    Task<List<ProductCardDto>> GetUserFavoritesAsync(Guid userId, CancellationToken ct = default);

    Task<bool> IsFavoriteAsync(Guid userId, long productId, CancellationToken ct = default);
}

public interface ICommentService
{
    /// <summary>ثبت نظر (در انتظار تأیید ادمین).</summary>
    Task<(bool Success, string Message)> AddAsync(Guid userId, long productId, byte rating, string text, CancellationToken ct = default);

    /// <summary>نظرهای تأییدشده یک محصول.</summary>
    Task<List<ProductCommentDto>> GetApprovedAsync(long productId, CancellationToken ct = default);

    Task<(double Average, int Count)> GetRatingSummaryAsync(long productId, CancellationToken ct = default);

    Task<PagedResult<ProductCommentDto>> GetAllForAdminAsync(bool? approved, int page, int pageSize, CancellationToken ct = default);

    Task SetApprovedAsync(long commentId, bool approved, CancellationToken ct = default);

    Task DeleteAsync(long commentId, CancellationToken ct = default);
}

public class ProductCommentDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = default!;
    public byte Rating { get; set; }
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
    public string ProductName { get; set; } = default!;
}

public interface INewsLetterService
{
    Task<(bool Success, string Message)> SubscribeAsync(string email, CancellationToken ct = default);

    Task<List<NewsLetterSubscriber>> GetAllAsync(CancellationToken ct = default);
}

public interface ISiteMessageService
{
    Task AddAsync(string fullName, string email, string subject, string body, CancellationToken ct = default);

    Task<List<SiteMessage>> GetAllAsync(CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(CancellationToken ct = default);

    Task MarkReadAsync(long id, CancellationToken ct = default);
}
