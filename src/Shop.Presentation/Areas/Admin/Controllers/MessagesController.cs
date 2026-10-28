using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>پیام‌های تماس با ما + خبرنامه + نظرات وبلاگ (نگهدارنده‌ها).</summary>
public class MessagesController(
    ISiteMessageService messages,
    INewsLetterService newsLetter,
    IBlogService blogs) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await messages.GetAllAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
    {
        await messages.MarkReadAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/newsletter")]
    public async Task<IActionResult> Newsletter(CancellationToken ct)
        => View(await newsLetter.GetAllAsync(ct));

    [HttpGet("/admin/blogcomments")]
    public async Task<IActionResult> BlogComments(bool? approved, CancellationToken ct)
    {
        ViewBag.Approved = approved;
        return View(await blogs.GetAllCommentsForAdminAsync(approved, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBlogComment(long id, CancellationToken ct)
    {
        await blogs.SetCommentApprovedAsync(id, true, ct);
        return RedirectToAction(nameof(BlogComments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBlogComment(long id, CancellationToken ct)
    {
        await blogs.DeleteCommentAsync(id, ct);
        return RedirectToAction(nameof(BlogComments));
    }
}
