using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>علاقه‌مندی‌ها و تعامل با محصول (API).</summary>
[Authorize]
public class InteractionController(
    IFavoriteService favorites,
    ICommentService comments) : ShopControllerBase
{
    [HttpPost("/api/favorites/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request, CancellationToken ct)
    {
        var (added, message) = await favorites.ToggleAsync(UserId, request.ProductId, ct);
        return Json(new { added, message });
    }

    [HttpPost("/api/comments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment([FromBody] CommentRequest request, CancellationToken ct)
    {
        // فیلتر محتوای غیراخلاقی — قانون پروژه
        var violation = Shop.Shared.ContentFilter.ViolationMessage(request.Text);
        if (violation is not null) return Json(new { success = false, message = violation });

        var (success, message) = await comments.AddAsync(UserId, request.ProductId, request.Rating, request.Text, ct);
        return Json(new { success, message });
    }
}

public class FavoriteRequest
{
    public long ProductId { get; set; }
}

public class CommentRequest
{
    public long ProductId { get; set; }
    public byte Rating { get; set; }
    public string Text { get; set; } = default!;
}
