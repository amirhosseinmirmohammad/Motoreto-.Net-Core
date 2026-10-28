namespace Shop.Domain;

/// <summary>
/// Outbox Pattern: رویدادها اول در همین جدول — در همان تراکنش دیتابیسِ تغییرِ کسب‌وکار — ذخیره می‌شوند
/// (تضمین: یا هر دو یا هیچ‌کدام). سپس OutboxDispatcherJob آن‌ها را به کافکا منتشر و PublishedAt را پر می‌کند.
/// خطای بروکر → RetryCount بالا می‌رود و دوره بعد دوباره تلاش می‌شود؛ هیچ رویدادی گم نمی‌شود.
/// </summary>
public class OutboxMessage
{
    public long Id { get; set; }

    /// <summary>نوع رویداد (فعلاً: notification).</summary>
    public string Type { get; set; } = default!;

    /// <summary>JSON payload.</summary>
    public string Payload { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    /// <summary>null = هنوز منتشر نشده.</summary>
    public DateTime? PublishedAt { get; set; }

    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
