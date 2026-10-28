namespace Shop.Domain;

/// <summary>
/// فروشگاه متصل به سبک تروب: مالک فروشگاه یک feed URL (JSON) می‌دهد و
/// سیستم به‌صورت دوره‌ای کالاهایش را import/همگام می‌کند.
/// قرارداد feed: {"products":[{"external_id":"..","title":"..","price_toman":0,"stock":0,"image_url":"..","category":"..","description":".."}]}
/// </summary>
public class ShopFeed
{
    public int Id { get; set; }

    public string ShopName { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public string FeedUrl { get; set; } = default!;

    /// <summary>فاصله همگام‌سازی خودکار (دقیقه) — صفر = فقط دستی.</summary>
    public int SyncIntervalMinutes { get; set; } = 360;

    public bool IsActive { get; set; } = true;

    public DateTime? LastSyncAt { get; set; }
    public string? LastSyncError { get; set; }
    public int ProductCount { get; set; }
}
