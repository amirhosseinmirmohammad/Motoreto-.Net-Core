namespace Shop.Domain;

/// <summary>اعلان درون‌برنامه‌ای (زنگوله هدر) — در همه فرایندها: سفارش، فروشندگی، آگهی، چت، کیف پول، افیلیت.</summary>
public class Notification
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Body { get; set; }

    /// <summary>آیکون Bootstrap Icons (مثل bi-box-seam).</summary>
    public string? Icon { get; set; }

    /// <summary>لینک داخلی مرتبط (مثل /orders یا /panel/ads).</summary>
    public string? Link { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
