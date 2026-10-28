using Microsoft.AspNetCore.SignalR;
using Shop.Application.Interfaces;
using Shop.Presentation.Hubs;

namespace Shop.Presentation.Services;

/// <summary>پیاده‌سازی SignalR برای INotificationBroadcaster.</summary>
public class SignalRBroadcaster(IHubContext<AppHub> hub) : INotificationBroadcaster
{
    public async Task PushAsync(Guid userId, string title, string? body, string? icon, string? link, CancellationToken ct = default)
    {
        await hub.Clients.Group($"user-{userId}").SendAsync("notify", new
        {
            title,
            body,
            icon = icon ?? "bi-bell",
            link
        }, ct);
    }

    public async Task PushChatRefreshAsync(long adId, CancellationToken ct = default)
    {
        await hub.Clients.Group($"ad-{adId}").SendAsync("chatRefresh", new { adId }, ct);
    }
}
