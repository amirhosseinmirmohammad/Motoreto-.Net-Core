using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Shop.Domain;

/// <summary>کاربر فروشگاه (مبتنی بر ASP.NET Identity).</summary>
public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>شماره موبایل (نمایش و تماس).</summary>
    public string? Mobile { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // ---- آدرس پیش‌فرض ----
    public int? StateId { get; set; }
    public virtual State? State { get; set; }

    public int? CityId { get; set; }
    public virtual City? City { get; set; }

    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }

    // ---- باشگاه مشتریان ----

    /// <summary>موجودی کیف پول (ریال).</summary>
    public long WalletBalance { get; set; }

    /// <summary>امتیاز باشگاه مشتریان (هر ۱۰ هزار تومان خرید = ۱ امتیاز).</summary>
    public long Points { get; set; }

    /// <summary>کد معرف یکتای کاربر.</summary>
    [System.ComponentModel.DataAnnotations.StringLength(12)]
    public string? ReferralCode { get; set; }

    /// <summary>کاربری که این کاربر را معرفی کرده.</summary>
    public Guid? ReferredByUserId { get; set; }

    public virtual List<WalletTransaction> WalletTransactions { get; set; } = [];

    public virtual List<Order> Orders { get; set; } = [];
    public virtual List<Comment> Comments { get; set; } = [];
    public virtual List<Favorite> Favorites { get; set; } = [];
    public virtual List<Blog> Blogs { get; set; } = [];
}
