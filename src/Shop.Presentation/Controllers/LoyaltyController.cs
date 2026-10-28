using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Presentation.Controllers;

namespace Shop.Presentation;

/// <summary>صفحات باشگاه مشتریان: کیف پول، امتیاز، کد معرفی.</summary>
[Authorize]
public class LoyaltyController(ILoyaltyService loyalty) : ShopControllerBase
{
    [HttpGet("/wallet")]
    public async Task<IActionResult> Wallet(CancellationToken ct)
    {
        var (balance, points) = await loyalty.GetSummaryAsync(UserId, ct);
        var transactions = await loyalty.GetTransactionsAsync(UserId, 30, ct);
        var referralCode = await loyalty.GetOrCreateReferralCodeAsync(UserId, ct);

        ViewBag.Balance = balance;
        ViewBag.Points = points;
        ViewBag.ReferralCode = referralCode;
        ViewBag.ReferralLink = $"{Request.Scheme}://{Request.Host}/account/register?ref={referralCode}";

        return View(transactions);
    }
}
