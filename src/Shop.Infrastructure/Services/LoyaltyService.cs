using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Infrastructure.Services;

/// <summary>
/// باشگاه مشتریان: کیف پول، امتیاز، کد معرفی.
/// همه تغییرات کیف پول اتمیک (UPDATE شرطی) و همراه با ثبت گردش است.
/// </summary>
public class LoyaltyService(ApplicationDbContext db) : ILoyaltyService
{
    public async Task<(bool Success, string Message)> ChargeWalletAsync(Guid userId, long amount, WalletTransactionType type, string? description, int? orderId = null, CancellationToken ct = default)
    {
        if (amount == 0) return (false, "مبلغ صفر است.");

        // UPDATE اتمیک با شرط موجودی کافی برای برداشت
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE identity_users
               SET wallet_balance = wallet_balance + {amount}
               WHERE id = {userId} AND wallet_balance + {amount} >= 0",
            ct);

        if (affected == 0)
            return (false, "موجودی کیف پول کافی نیست.");

        var balance = await db.Users.Where(u => u.Id == userId).Select(u => u.WalletBalance).FirstAsync(ct);

        db.WalletTransactions.Add(new WalletTransaction
        {
            UserId = userId,
            Amount = amount,
            Type = type,
            BalanceAfter = balance,
            OrderId = orderId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return (true, amount > 0 ? "کیف پول شارژ شد." : "پرداخت از کیف پول انجام شد.");
    }

    public async Task<(long Balance, long Points)> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.WalletBalance, u.Points })
            .FirstOrDefaultAsync(ct);

        return result is null ? (0, 0) : (result.WalletBalance, result.Points);
    }

    public async Task<List<WalletTransactionDto>> GetTransactionsAsync(Guid userId, int take = 30, CancellationToken ct = default)
    {
        return await db.WalletTransactions
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.Id)
            .Take(take)
            .Select(w => new WalletTransactionDto
            {
                Id = w.Id,
                Amount = w.Amount,
                BalanceAfter = w.BalanceAfter,
                TypeTitle = w.Type == WalletTransactionType.RegistrationGift ? "هدیه ثبت‌نام"
                    : w.Type == WalletTransactionType.ReferralReward ? "پاداش معرفی"
                    : w.Type == WalletTransactionType.OrderRefund ? "بازگشت وجه"
                    : w.Type == WalletTransactionType.OrderPayment ? "پرداخت سفارش"
                    : "شارژ ادمین",
                OrderId = w.OrderId,
                Description = w.Description,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<string> GetOrCreateReferralCodeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return string.Empty;

        if (!string.IsNullOrEmpty(user.ReferralCode)) return user.ReferralCode;

        // کد ۸ کاراکتری یکتا
        string code;
        do
        {
            code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..8]
                .Replace("+", "K").Replace("/", "M").ToUpperInvariant();
        } while (await db.Users.AnyAsync(u => u.ReferralCode == code, ct));

        user.ReferralCode = code;
        await db.SaveChangesAsync(ct);
        return code;
    }

    public async Task<(bool Success, string Message)> ApplyReferralAsync(Guid newUserId, string referralCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(referralCode)) return (false, "کد معرف نامعتبر است.");
        referralCode = referralCode.Trim().ToUpperInvariant();

        var newUser = await db.Users.FirstOrDefaultAsync(u => u.Id == newUserId, ct);
        if (newUser is null || newUser.ReferredByUserId is not null)
            return (false, "امکان ثبت معرف وجود ندارد.");

        var referrer = await db.Users.FirstOrDefaultAsync(u => u.ReferralCode == referralCode && u.Id != newUserId, ct);
        if (referrer is null)
            return (false, "کد معرف یافت نشد.");

        newUser.ReferredByUserId = referrer.Id;
        await db.SaveChangesAsync(ct);
        return (true, $"کد معرف {referrer.FirstName} عزیز ثبت شد. بعد از اولین خرید، پاداش به کیف پول هر دوی شما اضافه می‌شود.");
    }

    /// <summary>پاداش معرفی بعد از اولین خرید موفق معرفی‌شده.</summary>
    public async Task PayReferralRewardAsync(Guid newUserId, long orderId, long rewardAmount, CancellationToken ct = default)
    {
        var newUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == newUserId, ct);
        if (newUser?.ReferredByUserId is null) return;

        // هر کاربر فقط یک بار پاداش معرفی می‌گیرد (اولین خرید)
        var alreadyRewarded = await db.WalletTransactions
            .AnyAsync(w => w.UserId == newUser.ReferredByUserId
                        && w.Type == WalletTransactionType.ReferralReward
                        && w.OrderId == orderId, ct);

        if (alreadyRewarded) return;

        await ChargeWalletAsync(newUser.ReferredByUserId.Value, rewardAmount,
            WalletTransactionType.ReferralReward,
            $"پاداش معرفی — اولین خرید", (int?)null, ct);
    }

    public async Task AddPointsAsync(Guid userId, long orderTotalToman, CancellationToken ct = default)
    {
        // هر ۱۰ هزار تومان = ۱ امتیاز
        var points = orderTotalToman / 10_000;
        if (points <= 0) return;

        await db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Points, u => u.Points + points), ct);
    }
}

/// <summary>پیامک — پیاده‌سازی فعلی لاگ کنسول است؛ برای اتصال سامانه واقعی فقط همین کلاس را عوض کنید.</summary>
public class ConsoleSmsService(ILogger<ConsoleSmsService> logger) : ISmsService
{
    public Task SendAsync(string mobile, string message, CancellationToken ct = default)
    {
        // TODO: اتصال به کاوه‌نگار / ملی‌پیامک / فراز اس‌ام‌اس
        logger.LogInformation("SMS → {Mobile}: {Message}", mobile, message);
        return Task.CompletedTask;
    }
}
