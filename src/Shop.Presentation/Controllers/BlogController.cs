using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;

namespace Shop.Presentation;

/// <summary>وبلاگ فروشگاه.</summary>
public class BlogController(IBlogService blogs) : Controller
{
    [HttpGet("/blog")]
    [OutputCache(PolicyName = "BlogCache")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var result = await blogs.GetVisibleAsync(page, 9, ct);
        return View(result);
    }

    [HttpGet("/blog/{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken ct)
    {
        var post = await blogs.GetBySlugAsync(slug, ct);
        if (post is null) return NotFound();

        await blogs.IncrementViewAsync(post.Id, ct);

        ViewBag.Comments = await blogs.GetApprovedCommentsAsync(post.Id, ct);
        ViewBag.Latest = await blogs.GetLatestAsync(4, ct);
        return View(post);
    }

    [HttpPost("/api/blog/comments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment([FromBody] BlogCommentRequest request, CancellationToken ct)
    {
        var (success, message) = await blogs.AddCommentAsync(request.BlogId, request.FullName, request.Text, ct);
        return Json(new { success, message });
    }
}

public class BlogCommentRequest
{
    public int BlogId { get; set; }
    public string FullName { get; set; } = default!;
    public string Text { get; set; } = default!;
}
