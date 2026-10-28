using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

public class DiscountService(ApplicationDbContext db) : IDiscountService
{
    public async Task<DiscountValidationResult> ValidateAsync(string code, long orderSubtotal, CancellationToken ct = default)
    {
        code = code.Trim().ToUpperInvariant();
        var discount = await db.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Code == code, ct);

        if (discount is null || !discount.IsActive)
            return new DiscountValidationResult { Success = false, Message = "کد تخفیف نامعتبر است." };

        if (discount.ExpireAt is not null && discount.ExpireAt < DateTime.UtcNow)
            return new DiscountValidationResult { Success = false, Message = "کد تخفیف منقضی شده است." };

        if (discount.UsageLimit is not null && discount.UsedCount >= discount.UsageLimit)
            return new DiscountValidationResult { Success = false, Message = "ظرفیت این کد تکمیل شده است." };

        if (orderSubtotal < discount.MinOrderAmount)
            return new DiscountValidationResult
            {
                Success = false,
                Message = $"حداقل مبلغ سفارش: {discount.MinOrderAmount.ToToman()}"
            };

        var amount = orderSubtotal * discount.Percent / 100;
        if (discount.MaxAmount is not null)
            amount = Math.Min(amount, discount.MaxAmount.Value);

        return new DiscountValidationResult
        {
            Success = true,
            Message = $"کد تخفیف اعمال شد: {amount.ToToman()} تخفیف",
            Code = code,
            DiscountAmount = amount
        };
    }

    public async Task RegisterUsageAsync(string code, CancellationToken ct = default)
    {
        await db.Discounts
            .Where(d => d.Code == code.Trim().ToUpperInvariant())
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.UsedCount, d => d.UsedCount + 1), ct);
    }

    public async Task<List<Discount>> GetAllAsync(CancellationToken ct = default)
        => await db.Discounts.AsNoTracking().OrderByDescending(d => d.Id).ToListAsync(ct);

    public async Task<Discount?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Discounts.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task CreateAsync(Discount discount, CancellationToken ct = default)
    {
        discount.Code = discount.Code.Trim().ToUpperInvariant();
        db.Discounts.Add(discount);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Discount discount, CancellationToken ct = default)
    {
        var entity = await db.Discounts.FirstOrDefaultAsync(d => d.Id == discount.Id, ct);
        if (entity is null) return;

        entity.Code = discount.Code.Trim().ToUpperInvariant();
        entity.Title = discount.Title;
        entity.Percent = discount.Percent;
        entity.MaxAmount = discount.MaxAmount;
        entity.MinOrderAmount = discount.MinOrderAmount;
        entity.UsageLimit = discount.UsageLimit;
        entity.ExpireAt = discount.ExpireAt;
        entity.IsActive = discount.IsActive;

        await db.SaveChangesAsync(ct);
    }

    public async Task ToggleAsync(int id, CancellationToken ct = default)
    {
        await db.Discounts.Where(d => d.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.IsActive, d => !d.IsActive), ct);
    }
}

public class CommentService(ApplicationDbContext db) : ICommentService
{
    public async Task<(bool Success, string Message)> AddAsync(Guid userId, long productId, byte rating, string text, CancellationToken ct = default)
    {
        rating = Math.Clamp(rating, (byte)1, (byte)5);

        var exists = await db.Comments
            .AnyAsync(c => c.UserId == userId && c.ProductId == productId, ct);

        if (exists)
            return (false, "شما قبلاً برای این محصول نظر ثبت کرده‌اید.");

        var productExists = await db.Products.AnyAsync(p => p.Id == productId && p.IsActive, ct);
        if (!productExists)
            return (false, "محصول یافت نشد.");

        db.Comments.Add(new Comment
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Text = text.Trim(),
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return (true, "نظر شما ثبت شد و پس از تأیید نمایش داده می‌شود.");
    }

    public async Task<List<ProductCommentDto>> GetApprovedAsync(long productId, CancellationToken ct = default)
    {
        return await db.Comments
            .AsNoTracking()
            .Where(c => c.ProductId == productId && c.IsApproved)
            .OrderByDescending(c => c.Id)
            .Select(c => new ProductCommentDto
            {
                Id = c.Id,
                UserName = c.User.FirstName,
                Rating = c.Rating,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                IsApproved = true
            })
            .ToListAsync(ct);
    }

    public async Task<(double Average, int Count)> GetRatingSummaryAsync(long productId, CancellationToken ct = default)
    {
        var summary = await db.Comments
            .AsNoTracking()
            .Where(c => c.ProductId == productId && c.IsApproved)
            .GroupBy(_ => 1)
            .Select(g => new { Avg = g.Average(c => (double)c.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return summary is null ? (0, 0) : (summary.Avg, summary.Count);
    }

    public async Task<PagedResult<ProductCommentDto>> GetAllForAdminAsync(bool? approved, int page, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 10, 100);
        page = Math.Max(1, page);

        var query = db.Comments.AsNoTracking().AsQueryable();
        if (approved is not null) query = query.Where(c => c.IsApproved == approved);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new ProductCommentDto
            {
                Id = c.Id,
                UserName = c.User.FirstName + " " + c.User.LastName,
                Rating = c.Rating,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                IsApproved = c.IsApproved,
                ProductName = c.Product.Name
            })
            .ToListAsync(ct);

        return new PagedResult<ProductCommentDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task SetApprovedAsync(long commentId, bool approved, CancellationToken ct = default)
    {
        await db.Comments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsApproved, approved), ct);
    }

    public async Task DeleteAsync(long commentId, CancellationToken ct = default)
    {
        await db.Comments.Where(c => c.Id == commentId).ExecuteDeleteAsync(ct);
    }
}

public class FavoriteService(ApplicationDbContext db) : IFavoriteService
{
    public async Task<(bool Added, string Message)> ToggleAsync(Guid userId, long productId, CancellationToken ct = default)
    {
        var existing = await db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, ct);

        if (existing is not null)
        {
            db.Favorites.Remove(existing);
            await db.SaveChangesAsync(ct);
            return (false, "از علاقه‌مندی‌ها حذف شد.");
        }

        var exists = await db.Products.AnyAsync(p => p.Id == productId && p.IsActive, ct);
        if (!exists) return (false, "محصول یافت نشد.");

        db.Favorites.Add(new Favorite { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        return (true, "به علاقه‌مندی‌ها اضافه شد.");
    }

    public async Task<List<ProductCardDto>> GetUserFavoritesAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.Id)
            .Select(f => new ProductCardDto
            {
                Id = f.Product.Id,
                Name = f.Product.Name,
                Slug = f.Product.Slug,
                Image = f.Product.MainImage,
                UnitPrice = f.Product.UnitPrice,
                FinalPrice = f.Product.DiscountPercent > 0
                    ? f.Product.UnitPrice - f.Product.UnitPrice * f.Product.DiscountPercent / 100
                    : f.Product.UnitPrice,
                DiscountPercent = f.Product.DiscountPercent,
                Stock = f.Product.Stock - f.Product.ReservedCount,
                CategoryName = f.Product.Category.Name,
                CategorySlug = f.Product.Category.Slug
            })
            .ToListAsync(ct);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, long productId, CancellationToken ct = default)
        => await db.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId, ct);
}

public class NewsLetterService(ApplicationDbContext db) : INewsLetterService
{
    public async Task<(bool Success, string Message)> SubscribeAsync(string email, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();

        var exists = await db.NewsLetterSubscribers.AnyAsync(n => n.Email == email, ct);
        if (exists)
            return (false, "این ایمیل قبلاً ثبت شده است.");

        db.NewsLetterSubscribers.Add(new NewsLetterSubscriber { Email = email, SubscribedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        return (true, "عضویت شما در خبرنامه ثبت شد.");
    }

    public async Task<List<NewsLetterSubscriber>> GetAllAsync(CancellationToken ct = default)
        => await db.NewsLetterSubscribers.AsNoTracking().OrderByDescending(n => n.Id).ToListAsync(ct);
}

public class SiteMessageService(ApplicationDbContext db) : ISiteMessageService
{
    public async Task AddAsync(string fullName, string email, string subject, string body, CancellationToken ct = default)
    {
        db.SiteMessages.Add(new SiteMessage
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<SiteMessage>> GetAllAsync(CancellationToken ct = default)
        => await db.SiteMessages.AsNoTracking().OrderByDescending(m => m.Id).ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(CancellationToken ct = default)
        => await db.SiteMessages.CountAsync(m => !m.IsRead, ct);

    public async Task MarkReadAsync(long id, CancellationToken ct = default)
    {
        await db.SiteMessages.Where(m => m.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }
}
