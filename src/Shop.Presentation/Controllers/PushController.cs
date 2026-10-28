using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Controllers;

/// <summary>API اشتراک Web Push مرورگر.</summary>
[Route("api/push")]
public class PushController(ApplicationDbContext db, IConfiguration config) : ShopControllerBase
{
    [HttpGet("public-key")]
    public IActionResult PublicKey()
        => Json(new { key = config.GetValue<string>("WebPush:PublicKey", "") ?? "" });

    public sealed record SubscribeRequest(string Endpoint, string P256dh, string Auth);

    [HttpPost("subscribe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest? req, CancellationToken ct)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Endpoint))
            return Json(new { ok = false });

        var uid = UserId;
        var exists = await db.PushSubscriptions.AnyAsync(s => s.UserId == uid && s.Endpoint == req.Endpoint, ct);
        if (!exists)
        {
            db.PushSubscriptions.Add(new PushSubscription
            {
                UserId = uid,
                Endpoint = req.Endpoint,
                P256dh = req.P256dh,
                Auth = req.Auth,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }
        return Json(new { ok = true });
    }

    public sealed record UnsubscribeRequest(string Endpoint);

    [HttpPost("unsubscribe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest? req, CancellationToken ct)
    {
        if (req is null) return Json(new { ok = false });
        await db.PushSubscriptions.Where(s => s.UserId == UserId && s.Endpoint == req.Endpoint)
            .ExecuteDeleteAsync(ct);
        return Json(new { ok = true });
    }
}
