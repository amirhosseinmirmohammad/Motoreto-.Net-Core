using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Controllers;

/// <summary>API اعلان‌های درون‌برنامه‌ای (زنگوله هدر).</summary>
[Route("api/notifications")]
public class NotificationsController(ApplicationDbContext db) : ShopControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var uid = UserId;
        var unread = await db.Notifications.CountAsync(n => n.UserId == uid && !n.IsRead, ct);
        var items = await db.Notifications.AsNoTracking()
            .Where(n => n.UserId == uid)
            .OrderByDescending(n => n.Id)
            .Take(12)
            .Select(n => new { n.Id, n.Title, n.Body, n.Icon, n.Link, n.IsRead, n.CreatedAt })
            .ToListAsync(ct);
        return Json(new { unread, items });
    }

    [HttpGet("read/{id:long}")]
    public async Task<IActionResult> Read(long id, CancellationToken ct)
    {
        await db.Notifications.Where(n => n.UserId == UserId && n.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        return Json(new { ok = true });
    }

    [HttpGet("read-all")]
    public async Task<IActionResult> ReadAll(CancellationToken ct)
    {
        await db.Notifications.Where(n => n.UserId == UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        return Json(new { ok = true });
    }
}
