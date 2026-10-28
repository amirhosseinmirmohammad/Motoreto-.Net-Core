using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class CommentsController(ICommentService comments, IBlogService blogComments) : AdminBaseController
{
    public async Task<IActionResult> Index(bool? approved, int page = 1, CancellationToken ct = default)
    {
        var result = await comments.GetAllForAdminAsync(approved, page, 20, ct);
        ViewBag.Approved = approved;
        ViewBag.Page = result.Page;
        ViewBag.TotalPages = result.TotalPages;
        ViewBag.TotalCount = result.TotalCount;
        return View(result.Items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id, CancellationToken ct)
    {
        await comments.SetApprovedAsync(id, true, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long id, CancellationToken ct)
    {
        await comments.SetApprovedAsync(id, false, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await comments.DeleteAsync(id, ct);
        TempData["Success"] = "نظر حذف شد.";
        return RedirectToAction(nameof(Index));
    }
}
