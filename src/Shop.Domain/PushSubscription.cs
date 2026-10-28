namespace Shop.Domain;

/// <summary>اشتراک Web Push مرورگر کاربر (برای اعلان حتی با تب بسته).</summary>
public class PushSubscription
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>آدرس یکتای endpoint سرویس پوش (Chrome/Firefox/...).</summary>
    public string Endpoint { get; set; } = default!;
    public string P256dh { get; set; } = default!;
    public string Auth { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
