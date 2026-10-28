namespace Shop.Application.Interfaces;

/// <summary>ارسال Web Push به مرورگر کاربر (اگر VAPID تنظیم شده باشد).</summary>
public interface IPushSender
{
    Task SendToUserAsync(Guid userId, string title, string? body, string? link, CancellationToken ct = default);
}
