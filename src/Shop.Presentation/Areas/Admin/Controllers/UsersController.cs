using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Domain;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class UsersController(
    ApplicationDbContext db,
    UserManager<User> userManager) : AdminBaseController
{
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        const int pageSize = 20;

        var query = db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u => u.Email!.Contains(q) || (u.FirstName + " " + u.LastName).Contains(q) || (u.Mobile ?? "").Contains(q));
        }

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Mobile,
                u.IsActive,
                u.CreatedAt,
                OrdersCount = u.Orders.Count
            })
            .ToListAsync(ct);

        ViewBag.TotalCount = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Q = q;
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);
        TempData["Success"] = user.IsActive ? "کاربر فعال شد." : "کاربر غیرفعال شد.";
        return RedirectToAction(nameof(Index));
    }
}
