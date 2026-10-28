using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

public class BlogService(ApplicationDbContext db) : IBlogService
{
    private IQueryable<BlogPostDto> VisiblePosts() => db.Blogs
        .AsNoTracking()
        .Where(b => b.IsVisible)
        .OrderByDescending(b => b.Id)
        .Select(b => new BlogPostDto
        {
            Id = b.Id,
            Title = b.Title,
            Slug = b.Slug,
            ShortDescription = b.ShortDescription,
            Body = b.Body,
            CoverImage = b.CoverImage,
            IsVisible = b.IsVisible,
            ViewCount = b.ViewCount,
            CreatedAt = b.CreatedAt
        });

    public async Task<PagedResult<BlogPostDto>> GetVisibleAsync(int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 6, 24);
        page = Math.Max(1, page);

        var query = db.Blogs.AsNoTracking().Where(b => b.IsVisible).OrderByDescending(b => b.Id);
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new BlogPostDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                CoverImage = b.CoverImage,
                ViewCount = b.ViewCount,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<BlogPostDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<BlogPostDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await VisiblePosts().FirstOrDefaultAsync(b => b.Slug == slug, ct);

    public async Task IncrementViewAsync(int blogId, CancellationToken ct = default)
    {
        await db.Blogs.Where(b => b.Id == blogId)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.ViewCount, b => b.ViewCount + 1), ct);
    }

    public async Task<List<BlogPostDto>> GetLatestAsync(int count, CancellationToken ct = default)
    {
        return await db.Blogs
            .AsNoTracking()
            .Where(b => b.IsVisible)
            .OrderByDescending(b => b.Id)
            .Take(count)
            .Select(b => new BlogPostDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                CoverImage = b.CoverImage,
                ViewCount = b.ViewCount,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<(bool Success, string Message)> AddCommentAsync(int blogId, string fullName, string text, CancellationToken ct = default)
    {
        var exists = await db.Blogs.AnyAsync(b => b.Id == blogId && b.IsVisible, ct);
        if (!exists) return (false, "مقاله یافت نشد.");

        db.BlogComments.Add(new BlogComment
        {
            BlogId = blogId,
            FullName = fullName.Trim(),
            Text = text.Trim(),
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return (true, "نظر شما ثبت شد و پس از تأیید نمایش داده می‌شود.");
    }

    public async Task<List<BlogCommentDto>> GetApprovedCommentsAsync(int blogId, CancellationToken ct = default)
    {
        return await db.BlogComments
            .AsNoTracking()
            .Where(c => c.BlogId == blogId && c.IsApproved)
            .OrderByDescending(c => c.Id)
            .Select(c => new BlogCommentDto
            {
                Id = c.Id,
                BlogId = c.BlogId,
                FullName = c.FullName,
                Text = c.Text,
                IsApproved = true,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<List<BlogPostDto>> GetAllForAdminAsync(CancellationToken ct = default)
        => await db.Blogs.AsNoTracking().OrderByDescending(b => b.Id)
            .Select(b => new BlogPostDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                Body = b.Body,
                CoverImage = b.CoverImage,
                IsVisible = b.IsVisible,
                ViewCount = b.ViewCount,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

    public async Task<BlogPostDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Blogs.AsNoTracking().Where(b => b.Id == id)
            .Select(b => new BlogPostDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                Body = b.Body,
                CoverImage = b.CoverImage,
                IsVisible = b.IsVisible,
                ViewCount = b.ViewCount,
                CreatedAt = b.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

    public async Task CreateAsync(BlogPostDto post, Guid authorId, CancellationToken ct = default)
    {
        db.Blogs.Add(new Blog
        {
            Title = post.Title,
            Slug = post.Slug,
            ShortDescription = post.ShortDescription,
            Body = post.Body,
            CoverImage = post.CoverImage,
            IsVisible = post.IsVisible,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BlogPostDto post, CancellationToken ct = default)
    {
        var entity = await db.Blogs.FirstOrDefaultAsync(b => b.Id == post.Id, ct);
        if (entity is null) return;

        entity.Title = post.Title;
        entity.Slug = post.Slug;
        entity.ShortDescription = post.ShortDescription;
        entity.Body = post.Body;
        entity.CoverImage = post.CoverImage;
        entity.IsVisible = post.IsVisible;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await db.Blogs.Where(b => b.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task SetCommentApprovedAsync(long commentId, bool approved, CancellationToken ct = default)
    {
        await db.BlogComments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsApproved, approved), ct);
    }

    public async Task DeleteCommentAsync(long commentId, CancellationToken ct = default)
    {
        await db.BlogComments.Where(c => c.Id == commentId).ExecuteDeleteAsync(ct);
    }

    public async Task<List<BlogCommentDto>> GetAllCommentsForAdminAsync(bool? approved, CancellationToken ct = default)
    {
        var query = db.BlogComments.AsNoTracking().AsQueryable();
        if (approved is not null) query = query.Where(c => c.IsApproved == approved);

        return await query
            .OrderByDescending(c => c.Id)
            .Select(c => new BlogCommentDto
            {
                Id = c.Id,
                BlogId = c.BlogId,
                BlogTitle = c.Blog.Title,
                FullName = c.FullName,
                Text = c.Text,
                IsApproved = c.IsApproved,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);
    }
}
