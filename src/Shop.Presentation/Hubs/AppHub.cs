using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Shop.Presentation.Hubs;

/// <summary>
/// هاب اعلان و چت زنده.
/// - کاربران احرازشده به گروه user-{id} اضافه می‌شوند (دریافت اعلان‌ها)
/// - اگر ?adId= در اتصال باشد، به گروه ad-{adId} هم اضافه می‌شود (چت زنده آگهی)
/// </summary>
[Authorize]
public class AppHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var uid = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(uid))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{uid}");

        var adId = Context.GetHttpContext()?.Request.Query["adId"].ToString();
        if (long.TryParse(adId, out var ad))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ad-{ad}");

        await base.OnConnectedAsync();
    }
}
