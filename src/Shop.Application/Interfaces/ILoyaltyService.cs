using Shop.Application.DTOs;
using Shop.Domain;

namespace Shop.Application.Interfaces;

/// <summary>باشگاه مشتریان: کیف پول، امتیاز و معرفی.</summary>
public interface ILoyaltyService
{
    /// <summary>شارژ/برداشت اتمیک کیف پول + ثبت گردش.</summary>
    Task<(bool Success, string Message)> ChargeWalletAsync(Guid userId, long amount, WalletTransactionType type, string? description, int? orderId = null, CancellationToken ct = default);

    Task<(long Balance, long Points)> GetSummaryAsync(Guid userId, CancellationToken ct = default);

    Task<List<WalletTransactionDto>> GetTransactionsAsync(Guid userId, int take = 30, CancellationToken ct = default);

    Task<string> GetOrCreateReferralCodeAsync(Guid userId, CancellationToken ct = default);

    Task<(bool Success, string Message)> ApplyReferralAsync(Guid newUserId, string referralCode, CancellationToken ct = default);

    /// <summary>پاداش معرفی پس از اولین خرید معرفی‌شده.</summary>
    Task PayReferralRewardAsync(Guid newUserId, long orderId, long rewardAmount, CancellationToken ct = default);

    Task AddPointsAsync(Guid userId, long orderTotalRial, CancellationToken ct = default);
}

/// <summary>سامانه پیامکی — برای ورود با OTP و اطلاع‌رسانی.</summary>
public interface ISmsService
{
    Task SendAsync(string mobile, string message, CancellationToken ct = default);
}

public class WalletTransactionDto
{
    public long Id { get; set; }
    public long Amount { get; set; }
    public long BalanceAfter { get; set; }
    public string TypeTitle { get; set; } = default!;
    public int? OrderId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
