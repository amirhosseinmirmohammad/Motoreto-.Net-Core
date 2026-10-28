using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class OrdersController(IOrderService orders) : AdminBaseController
{
    public async Task<IActionResult> Index(string? status, string? q, int page = 1, CancellationToken ct = default)
    {
        var result = await orders.GetFilteredForAdminAsync(status, q, page, 20, ct);

        ViewBag.Status = status;
        ViewBag.Q = q;
        ViewBag.Statuses = Enum.GetValues<OrderStatus>()
            .Select(s => new { Value = s.ToString(), Title = OrderTitles.StatusTitle(s) })
            .ToList();

        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await orders.GetOrderForAdminAsync(id, ct);
        if (order is null) return NotFound();

        ViewBag.Statuses = Enum.GetValues<OrderStatus>().ToList();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, CancellationToken ct)
    {
        var ok = await orders.UpdateStatusAsync(id, status, ct);
        TempData[ok ? "Success" : "Error"] = ok
            ? "وضعیت سفارش تغییر کرد."
            : "تغییر وضعیت ناموفق بود.";

        return RedirectToAction(nameof(Details), new { id });
    }
}
