using Shop.Domain;

namespace Shop.Application.Interfaces;

/// <summary>ارسال زنده اعلان به مرورگر کاربر (SignalR) — مکمل ذخیره در دیتابیس.</summary>
public interface INotificationBroadcaster
{
    Task PushAsync(Guid userId, string title, string? body, string? icon, string? link, CancellationToken ct = default);
    /// <summary>اطلاع به گروه چت یک آگهی که پیام تازه رسیده (صفحه‌های باز، پیام جدید را می‌کشند).</summary>
    Task PushChatRefreshAsync(long adId, CancellationToken ct = default);
}
