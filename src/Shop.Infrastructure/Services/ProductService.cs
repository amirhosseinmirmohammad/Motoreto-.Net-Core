using Shop.Domain;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.Services;

/// <summary>
/// کوئری‌های محصول با Projection (بدون بارگذاری ستون‌های سنگین مثل Description در لیست‌ها)
/// و AsNoTracking برای حداکثر سرعت.
/// </summary>
public class ProductService(ApplicationDbContext db) : IProductService
{
    // قیمت نهایی قابل نمایش: حداقلِ (قیمت مارکت در صورت موجودی، ارزان‌ترین آفر فعال)
    private IQueryable<Product> ActiveProducts() => db.Products.AsNoTracking().Where(p => p.IsActive);

    private IQueryable<ProductCardDto> Project(IQueryable<Product> query) => query.Select(p => new ProductCardDto
    {
        Id = p.Id,
        Name = p.Name,
        EnglishName = p.EnglishName,
        Slug = p.Slug,
        Image = p.MainImage,
        UnitPrice = p.UnitPrice,
        FinalPrice = p.DiscountPercent > 0 ? p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 : p.UnitPrice,
        DiscountPercent = p.DiscountPercent,
        Stock = p.Stock - p.ReservedCount,
        IsAmazing = p.IsAmazing,
        AmazingEndsAt = p.AmazingEndsAt,
        IsSpecial = p.IsSpecial,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        CategorySlug = p.Category.Slug,
        BrandName = p.Brand != null ? p.Brand.Name : null,
        BrandSlug = p.Brand != null ? p.Brand.Slug : null,
        AttributesJson = p.AttributesJson,
        AverageRating = p.Comments.Any()
            ? p.Comments.Average(c => (double)c.Rating)
            : 0,
        CommentCount = p.Comments.Count,
        SalesCount = p.SalesCount,
        MinOfferPrice = p.Offers.Any(o => o.IsActive && o.Seller.Status == SellerStatus.Approved && o.Seller.IsActive && o.Stock - o.ReservedCount > 0)
            ? p.Offers.Where(o => o.IsActive && o.Seller.Status == SellerStatus.Approved && o.Seller.IsActive && o.Stock - o.ReservedCount > 0)
                .Min(o => o.DiscountPercent > 0 ? o.Price - o.Price * o.DiscountPercent / 100 : o.Price)
            : null,
        OfferCount = p.Offers.Count(o => o.IsActive && o.Seller.Status == SellerStatus.Approved && o.Seller.IsActive && o.Stock - o.ReservedCount > 0)
    });

    private IQueryable<ProductCardDto> ActiveCards() => Project(ActiveProducts());

    public async Task<PagedResult<ProductCardDto>> GetFilteredAsync(ProductFilter filter, CancellationToken ct = default)
    {
        // فیلترها روی Entity (قبل از Projection) تا EF بتواند ترجمه کند
        var entityQuery = await ApplyBaseFilters(ActiveProducts(), filter, ct);

        // برند (فیلتر در SQL)
        if (filter.BrandSlugs is { Count: > 0 })
            entityQuery = entityQuery.Where(p => p.Brand != null && filter.BrandSlugs.Contains(p.Brand!.Slug));

        var query = Project(entityQuery);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 6, 48);
        var sort = filter.Sort?.ToLowerInvariant();

        // ویژگی‌های داینامیک — فیلتر درون‌حافظه‌ای روی AttributesJson (ساختار باز ادمین-تعریف)
        if (filter.Attributes is { Count: > 0 })
        {
            var all = await query.ToListAsync(ct);
            var filtered = all.Where(p => MatchesAttributes(p.AttributesJson, filter.Attributes)).ToList();

            return new PagedResult<ProductCardDto>
            {
                Items = SortQuery(filtered.AsQueryable(), sort).Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = filtered.Count,
                Page = page,
                PageSize = pageSize
            };
        }

        query = SortQuery(query, sort);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<ProductCardDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    /// <inheritdoc />
    public async Task<ProductFacetsDto?> GetFacetsAsync(ProductFilter filter, CancellationToken ct = default)
    {
        // روی نتیجه بدون فیلتر برند/ویژگی (با انتخاب برند، بقیه فیت‌ها ثابت می‌مانند)
        var entityQuery = await ApplyBaseFilters(ActiveProducts(), filter, ct);
        var rows = await Project(entityQuery).Take(2000).ToListAsync(ct); // سقف محاسبه facets

        var facets = new ProductFacetsDto();

        foreach (var g in rows
                     .Where(r => r.BrandSlug is not null)
                     .GroupBy(r => new { r.BrandName, r.BrandSlug })
                     .OrderByDescending(g => g.Count()))
        {
            facets.Brands.Add(new BrandFacetDto { Name = g.Key.BrandName!, Slug = g.Key.BrandSlug!, Count = g.Count() });
        }

        var attrAgg = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.AttributesJson)) continue;
            List<ProductAttributeGroupDto>? groups;
            try { groups = System.Text.Json.JsonSerializer.Deserialize<List<ProductAttributeGroupDto>>(row.AttributesJson, AttrJsonOptions); }
            catch { continue; }
            if (groups is null) continue;

            foreach (var g in groups)
            {
                if (g.Values is null || g.Values.Count == 0) continue;
                if (!attrAgg.TryGetValue(g.Name, out var vals))
                    attrAgg[g.Name] = vals = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var v in g.Values)
                    vals[v] = vals.TryGetValue(v, out var c) ? c + 1 : 1;
            }
        }

        foreach (var (name, vals) in attrAgg.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            facets.Attributes.Add(new AttributeFacetDto
            {
                Name = name,
                Values = [.. vals.OrderByDescending(v => v.Value).Select(v => new AttributeValueFacetDto { Value = v.Key, Count = v.Value })]
            });

        return facets;
    }

    /// <summary>فیلترهای پایه (جستجو/دسته/قیمت/موجود/تخفیف) روی Entity — مشترک بین لیست و Facets.</summary>
    private async Task<IQueryable<Product>> ApplyBaseFilters(IQueryable<Product> query, ProductFilter filter, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var q = filter.Q.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q}%")
                                     || EF.Functions.ILike(p.EnglishName, $"%{q}%"));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var category = await db.Categories
                .AsNoTracking()
                .Where(c => c.Slug == filter.CategorySlug)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(ct);

            if (category is null)
                return query.Where(p => false);

            var ids = await db.Categories
                .AsNoTracking()
                .Where(c => c.Id == category || c.ParentId == category)
                .Select(c => c.Id)
                .ToListAsync(ct);

            // محصولِ چنددسته‌ای: دسته اصلی یا هر دسته junction شده
            query = query.Where(p => ids.Contains(p.CategoryId)
                || db.ProductCategories.Any(pc => pc.ProductId == p.Id && ids.Contains(pc.CategoryId)));
        }

        if (filter.MinPrice is > 0)
            query = query.Where(p => p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 >= filter.MinPrice);

        if (filter.MaxPrice is > 0)
            query = query.Where(p => p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 <= filter.MaxPrice);

        if (filter.OnlyAvailable)
            query = query.Where(p => p.Stock - p.ReservedCount > 0);

        if (filter.OnlyDiscounted)
            query = query.Where(p => p.DiscountPercent > 0);

        return query;
    }

    private static IQueryable<ProductCardDto> SortQuery(IQueryable<ProductCardDto> query, string? sort) =>
        sort switch
        {
            "cheap" => query.OrderBy(p => p.FinalPrice),
            "expensive" => query.OrderByDescending(p => p.FinalPrice),
            "discount" => query.OrderByDescending(p => p.DiscountPercent),
            "bestseller" => query.OrderByDescending(p => p.SalesCount),
            _ => query.OrderByDescending(p => p.Id)
        };

    private static readonly System.Text.Json.JsonSerializerOptions AttrJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>آیا AttributesJson محصول همه فیلترهای ویژگی انتخابی را پاس می‌کند؟</summary>
    private static bool MatchesAttributes(string? json, List<AttributeFilter> filters)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            var groups = System.Text.Json.JsonSerializer.Deserialize<List<ProductAttributeGroupDto>>(json, AttrJsonOptions);
            if (groups is null) return false;

            return filters.All(f => groups.Any(g =>
                g.Name == f.Name && (g.Values ?? []).Intersect(f.Values, StringComparer.Ordinal).Any()));
        }
        catch
        {
            return false;
        }
    }

    public async Task<ProductDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var card = await ActiveCards().FirstOrDefaultAsync(p => p.Slug == slug, ct);
        if (card is null) return null;

        var detail = await db.Products
            .AsNoTracking()
            .Where(p => p.Slug == slug)
            .Select(p => new
            {
                p.Id,
                p.EnglishName,
                p.Code,
                p.Description,
                p.ViewCount,
                p.CategoryId,
                p.AttributesJson,
                p.MetaTitle,
                p.MetaDescription,
                L1Id = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Parent.Id : p.Category.Parent != null ? p.Category.Parent.Id : p.Category.Id,
                L1Name = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Parent.Name : p.Category.Parent != null ? p.Category.Parent.Name : p.Category.Name,
                L1Slug = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Parent.Slug : p.Category.Parent != null ? p.Category.Parent.Slug : p.Category.Slug,
                L2Id = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Id : p.Category.Id,
                L2Name = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Name : p.Category.Name,
                L2Slug = p.Category.Parent != null && p.Category.Parent.Parent != null ? p.Category.Parent.Slug : p.Category.Slug,
                L3Id = p.Category.Id,
                L3Name = p.Category.Name,
                L3Slug = p.Category.Slug,
                Images = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList()
            })
            .FirstAsync(ct);

        return new ProductDetailDto
        {
            Card = card,
            EnglishName = detail.EnglishName,
            Code = detail.Code,
            Description = detail.Description,
            ViewCount = detail.ViewCount,
            AttributesJson = detail.AttributesJson,
            MetaTitle = detail.MetaTitle,
            MetaDescription = detail.MetaDescription,
            BreadCrumbs =
            [
                new BreadCrumbItem { Name = detail.L1Name, Slug = detail.L1Slug },
                new BreadCrumbItem { Name = detail.L2Name, Slug = detail.L2Slug },
                new BreadCrumbItem { Name = detail.L3Name, Slug = detail.L3Slug }
            ],
            CategoryId = detail.CategoryId,
            Images = detail.Images
        };
    }

    public async Task IncrementViewAsync(long productId, CancellationToken ct = default)
    {
        // آپدیت اتمیک بدون بارگذاری entity
        await db.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);
    }

    public async Task<List<ProductCardDto>> GetAmazingAsync(int count, CancellationToken ct = default)
    {
        return await ActiveCards()
            .Where(p => p.IsAmazing && p.Stock > 0 && p.DiscountPercent > 0)
            .OrderByDescending(p => p.DiscountPercent)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<ProductCardDto>> GetSpecialAsync(int count, CancellationToken ct = default)
    {
        return await ActiveCards()
            .Where(p => p.IsSpecial)
            .OrderByDescending(p => p.SalesCount)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<ProductCardDto>> GetNewestAsync(int count, CancellationToken ct = default)
    {
        return await ActiveCards()
            .OrderByDescending(p => p.Id)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<ProductCardDto>> GetMostViewedAsync(int count, CancellationToken ct = default)
    {
        return await ActiveCards()
            .OrderByDescending(p => p.AverageRating)
            .ThenByDescending(p => p.SalesCount)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<ProductCardDto>> GetBestSellersAsync(int count, CancellationToken ct = default)
    {
        return await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.SalesCount)
            .Take(count)
            .Select(p => new ProductCardDto
            {
                Id = p.Id,
                Name = p.Name,
                EnglishName = p.EnglishName,
                Slug = p.Slug,
                Image = p.MainImage,
                UnitPrice = p.UnitPrice,
                FinalPrice = p.DiscountPercent > 0 ? p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 : p.UnitPrice,
                DiscountPercent = p.DiscountPercent,
                Stock = p.Stock - p.ReservedCount,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                SalesCount = p.SalesCount
            })
            .ToListAsync(ct);
    }

    public async Task<List<ProductCardDto>> GetRelatedAsync(long productId, int categoryId, int count, CancellationToken ct = default)
    {
        return await ActiveCards()
            .Where(p => p.Id != productId && p.CategoryId == categoryId && p.Stock > 0)
            .OrderByDescending(p => p.SalesCount)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
        => await db.Products.CountAsync(p => p.IsActive, ct);

    /// <summary>کارت محصولات برای لیست مقایسه (حفظ ترتیب ورودی).</summary>
    public async Task<List<ProductCardDto>> GetCardsByIdsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds.Length == 0) return [];

        var cards = await db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .Select(p => new ProductCardDto
            {
                Id = p.Id,
                Name = p.Name,
                EnglishName = p.EnglishName,
                Slug = p.Slug,
                Image = p.MainImage,
                UnitPrice = p.UnitPrice,
                FinalPrice = p.DiscountPercent > 0 ? p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 : p.UnitPrice,
                DiscountPercent = p.DiscountPercent,
                Stock = p.Stock - p.ReservedCount,
                AttributesJson = p.AttributesJson,
                CategoryName = p.Category.Name,
                CategorySlug = p.Category.Slug,
                AverageRating = p.Comments.Any() ? p.Comments.Average(c => (double)c.Rating) : 0,
                CommentCount = p.Comments.Count,
                OfferCount = p.Offers.Count(o => o.IsActive && o.Seller.Status == SellerStatus.Approved),
                MinOfferPrice = p.Offers.Any(o => o.IsActive && o.Seller.Status == SellerStatus.Approved)
                    ? p.Offers.Where(o => o.IsActive && o.Seller.Status == SellerStatus.Approved)
                        .Min(o => o.DiscountPercent > 0 ? o.Price - o.Price * o.DiscountPercent / 100 : o.Price)
                    : null
            })
            .ToListAsync(ct);

        return productIds
            .Select(id => cards.FirstOrDefault(c => c.Id == id))
            .Where(c => c is not null)
            .Cast<ProductCardDto>()
            .ToList();
    }
}
