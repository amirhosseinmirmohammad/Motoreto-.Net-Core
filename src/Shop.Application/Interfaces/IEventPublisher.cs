namespace Shop.Application.Interfaces;

/// <summary>رویداد اعلان — از Kafka منتشر می‌شود (fallback: درج مستقیم برای dev بدون بروکر). Sms=true → پیامک هم می‌رود.</summary>
public sealed record NotificationEvent(Guid UserId, string Title, string? Body, string? Icon, string? Link, bool Sms = false);

/// <summary>
/// ناشر رویدادها (message broker): publish غیرمسدودکننده در مسیر درخواست؛
/// مصرف توسط NotificationConsumerJob انجام می‌شود تا درخواست‌ها زیر بار نیفتند.
/// </summary>
public interface IEventPublisher
{
    Task PublishNotificationAsync(NotificationEvent evt, CancellationToken ct = default);
}
