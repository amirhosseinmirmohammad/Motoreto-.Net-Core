using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;
using WebPush;
using DomainPush = Shop.Domain.PushSubscription;

namespace Shop.Presentation.Services;

/// <summary>ارسال Web Push به همه اشتراک‌های مرورگری کاربر؛ endpointهای مُرده خودکار پاک می‌شوند.</summary>
public class PushNotificationSender(IConfiguration config, ApplicationDbContext db, ILogger<PushNotificationSender> logger) : IPushSender
{
    private string? PublicKey => config.GetValue<string>("WebPush:PublicKey", "");
    private string? PrivateKey => config.GetValue<string>("WebPush:PrivateKey", "");
    private string Subject => config.GetValue<string>("WebPush:Subject", "mailto:admin@shop.ir");
    public bool Enabled => !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);

    public async Task SendToUserAsync(Guid userId, string title, string? body, string? link, CancellationToken ct = default)
    {
        if (!Enabled) return;

        var subs = await db.PushSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);
        if (subs.Count == 0) return;

        var vapid = new VapidDetails(Subject, PublicKey!, PrivateKey!);
        var payload = System.Text.Json.JsonSerializer.Serialize(new { title, body, link });
        var webPushClient = new WebPushClient();

        foreach (var s in subs)
        {
            try
            {
                await webPushClient.SendNotificationAsync(new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth),
                    payload, vapid);
            }
            catch (WebPushException ex) when (ex.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
            {
                // اشتراک منقضی — حذف
                db.PushSubscriptions.Remove(new DomainPush { Id = s.Id });
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Web Push به {Endpoint} ناموفق بود", s.Endpoint[..60]);
            }
        }
    }
}
