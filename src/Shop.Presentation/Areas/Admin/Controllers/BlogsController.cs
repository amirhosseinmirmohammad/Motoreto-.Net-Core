using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;

namespace Shop.Presentation.Areas.Admin.Controllers;

public class BlogsController(IBlogService blogs) : AdminBaseController
{
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await blogs.GetAllForAdminAsync(ct));

    [HttpGet]
    public IActionResult Create() => View(new BlogPostDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostDto post, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(post);

        post.Slug = SlugHelper.Slugify(string.IsNullOrWhiteSpace(post.Slug) ? post.Title : post.Slug);
        post.CoverImage = string.IsNullOrWhiteSpace(post.CoverImage)
            ? $"/images/blog/cover-{DateTime.UtcNow:yyyyMMddHHmmss}.svg"
            : post.CoverImage;

        var userId = CurrentUserId();
        await blogs.CreateAsync(post, userId, ct);
        TempData["Success"] = "مقاله ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var post = await blogs.GetByIdAsync(id, ct);
        if (post is null) return NotFound();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogPostDto post, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(post);

        post.Slug = SlugHelper.Slugify(string.IsNullOrWhiteSpace(post.Slug) ? post.Title : post.Slug);
        await blogs.UpdateAsync(post, ct);
        TempData["Success"] = "مقاله به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await blogs.DeleteAsync(id, ct);
        TempData["Success"] = "مقاله حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : Guid.Empty;
}
