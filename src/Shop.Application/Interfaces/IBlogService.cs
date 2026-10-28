using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

public interface IBlogService
{
    Task<PagedResult<BlogPostDto>> GetVisibleAsync(int page, int pageSize, CancellationToken ct = default);

    Task<BlogPostDto?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task IncrementViewAsync(int blogId, CancellationToken ct = default);

    Task<List<BlogPostDto>> GetLatestAsync(int count, CancellationToken ct = default);

    Task<(bool Success, string Message)> AddCommentAsync(int blogId, string fullName, string text, CancellationToken ct = default);

    Task<List<BlogCommentDto>> GetApprovedCommentsAsync(int blogId, CancellationToken ct = default);

    // ادمین
    Task<List<BlogPostDto>> GetAllForAdminAsync(CancellationToken ct = default);

    Task<BlogPostDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task CreateAsync(BlogPostDto post, Guid authorId, CancellationToken ct = default);

    Task UpdateAsync(BlogPostDto post, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task SetCommentApprovedAsync(long commentId, bool approved, CancellationToken ct = default);

    Task DeleteCommentAsync(long commentId, CancellationToken ct = default);

    Task<List<BlogCommentDto>> GetAllCommentsForAdminAsync(bool? approved, CancellationToken ct = default);
}

public class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string CoverImage { get; set; } = default!;
    public bool IsVisible { get; set; } = true;
    public long ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BlogCommentDto
{
    public long Id { get; set; }
    public int BlogId { get; set; }
    public string BlogTitle { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Text { get; set; } = default!;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}
