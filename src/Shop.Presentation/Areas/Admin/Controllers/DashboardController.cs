using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class DashboardController(IOrderService orders) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await orders.GetDashboardAsync(ct);
        return View(model);
    }
}
