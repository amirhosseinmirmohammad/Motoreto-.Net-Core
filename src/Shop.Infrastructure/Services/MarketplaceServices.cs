using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Domain;
using Shop.Infrastructure.Data;
using Shop.Shared;

namespace Shop.Infrastructure.Services;

/// <summary>چرخه حیات فروشنده مارکت‌پلیس: ثبت‌نام → بررسی ادمین → پیشنهاد فروش روی کالاهای کاتالوگ.</summary>
public class SellerService(ApplicationDbContext db, IEventPublisher events) : ISellerService
{
    public async Task<(bool Success, string Message)> ApplyAsync(Guid userId, SellerApplyViewModel model, CancellationToken ct = default)
    {
        var exists = await db.Sellers.AnyAsync(s => s.OwnerUserId == userId, ct);
        if (exists)
            return (false, "شما قبلاً فروشگاه ثبت کرده‌اید.");

        var slug = SlugHelper.Slugify(model.StoreName);
        if (await db.Sellers.AnyAsync(s => s.Slug == slug, ct))
            slug = $"{slug}-{Random.Shared.Next(100, 999)}";

        db.Sellers.Add(new Seller
        {
            StoreName = model.StoreName.Trim(),
            Slug = slug,
            OwnerUserId = userId,
            Type = (SellerType)model.Type,
            Description = model.Description,
            Address = model.Address,
            Phone = model.Phone,
            Email = model.Email,
            Sheba = model.Sheba,
            FirstName = model.FirstName,
            LastName = model.LastName,
            NationalCode = model.NationalCode,
            CompanyName = model.CompanyName,
            CompanyNationalId = model.CompanyNationalId,
            EconomicCode = model.EconomicCode,
            RegistrationNumber = model.RegistrationNumber,
            CeoName = model.CeoName,
            CeoNationalId = model.CeoNationalId,
            BusinessLicenseNumber = model.BusinessLicenseNumber.Trim(),
            BusinessLicenseFileUrl = model.BusinessLicenseFileUrl,
            NationalCardFileUrl = model.NationalCardFileUrl,
            CompanyAdFileUrl = model.CompanyAdFileUrl,
            Status = SellerStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return (true, "درخواست فروشگاه ثبت شد و پس از بررسی مجوز کسب‌وکار توسط پشتیبانی فعال می‌شود.");
    }

    public async Task<Seller?> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await db.Sellers.FirstOrDefaultAsync(s => s.OwnerUserId == userId, ct);

    public async Task UpdateAsync(Seller model, CancellationToken ct = default)
    {
        var seller = await db.Sellers.FirstOrDefaultAsync(s => s.Id == model.Id, ct);
        if (seller is null || seller.Status == SellerStatus.Approved) return;

        seller.StoreName = model.StoreName.Trim();
        seller.Description = model.Description;
        seller.Address = model.Address;
        seller.Phone = model.Phone;
        seller.Email = model.Email;
        seller.Sheba = model.Sheba;
        seller.FirstName = model.FirstName;
        seller.LastName = model.LastName;
        seller.NationalCode = model.NationalCode;
        seller.CompanyName = model.CompanyName;
        seller.CompanyNationalId = model.CompanyNationalId;
        seller.EconomicCode = model.EconomicCode;
        seller.RegistrationNumber = model.RegistrationNumber;
        seller.CeoName = model.CeoName;
        seller.CeoNationalId = model.CeoNationalId;
        seller.BusinessLicenseNumber = model.BusinessLicenseNumber.Trim();
        seller.BusinessLicenseFileUrl = model.BusinessLicenseFileUrl;
        seller.NationalCardFileUrl = model.NationalCardFileUrl;
        seller.CompanyAdFileUrl = model.CompanyAdFileUrl;
        seller.Status = SellerStatus.PendingApproval;
        seller.RejectReason = null;

        await db.SaveChangesAsync(ct);
    }

    public async Task<Seller?> GetByIdAsync(long id, CancellationToken ct = default)
        => await db.Sellers.Include(s => s.Owner).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<List<Seller>> GetAllForAdminAsync(SellerStatus? status, CancellationToken ct = default)
    {
        var query = db.Sellers.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(s => s.Status == status);

        return await query.OrderByDescending(s => s.Id).ToListAsync(ct);
    }

    public async Task ApproveAsync(long sellerId, int commissionPercent, CancellationToken ct = default)
    {
        await db.Sellers
            .Where(s => s.Id == sellerId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(s => s.Status, SellerStatus.Approved)
                .SetProperty(s => s.CommissionPercent, Math.Clamp(commissionPercent, 0, 50))
                .SetProperty(s => s.ApprovedAt, DateTime.UtcNow)
                .SetProperty(s => s.RejectReason, (string?)null)
                .SetProperty(s => s.IsActive, true), ct);

        // فعال‌سازی پیشنهادهای فروش فروشنده تأییدشده
        await db.SellerOffers
            .Where(o => o.SellerId == sellerId)
            .ExecuteUpdateAsync(u => u.SetProperty(o => o.IsActive, true), ct);

        var owner = await db.Sellers.AsNoTracking()
            .Where(s => s.Id == sellerId).Select(s => (Guid?)s.OwnerUserId).FirstOrDefaultAsync(ct);
        if (owner is not null)
            await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                owner.Value, "🎉 فروشگاه شما تأیید شد!",
                $"کمیسیون {commissionPercent}٪ — از همین حالا می‌توانید آفر ثبت کنید", "bi-shop", "/seller"), ct);
    }

    public async Task RejectAsync(long sellerId, string reason, CancellationToken ct = default)
    {
        await db.Sellers
            .Where(s => s.Id == sellerId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(s => s.Status, SellerStatus.Rejected)
                .SetProperty(s => s.RejectReason, reason), ct);

        var owner = await db.Sellers.AsNoTracking()
            .Where(s => s.Id == sellerId).Select(s => (Guid?)s.OwnerUserId).FirstOrDefaultAsync(ct);
        if (owner is not null)
            await events.PublishNotificationAsync(new Application.Interfaces.NotificationEvent(
                owner.Value, "درخواست فروشندگی شما رد شد",
                $"دلیل: {reason}", "bi-x-circle", "/seller/register"), ct);
    }

    public async Task<List<SellerOfferDto>> GetMyOffersAsync(Guid userId, CancellationToken ct = default)
    {
        var sellerId = await db.Sellers.Where(s => s.OwnerUserId == userId).Select(s => (long?)s.Id).FirstOrDefaultAsync(ct);
        if (sellerId is null) return [];

        return await QueryOffers()
            .Where(o => o.SellerId == sellerId)
            .OrderByDescending(o => o.OfferId)
            .ToListAsync(ct);
    }

    public async Task<(bool Success, string Message)> SaveOfferAsync(Guid userId, SellerOfferInput input, CancellationToken ct = default)
    {
        var seller = await db.Sellers.FirstOrDefaultAsync(s => s.OwnerUserId == userId, ct);
        if (seller is null)
            return (false, "ابتدا فروشگاه خود را ثبت کنید.");
        if (seller.Status != SellerStatus.Approved || !seller.IsActive)
            return (false, "فروشگاه شما هنوز تأیید نشده است.");

        var productExists = await db.Products.AnyAsync(p => p.Id == input.ProductId && p.IsActive, ct);
        if (!productExists)
            return (false, "کالای انتخابی یافت نشد.");

        var offer = await db.SellerOffers.FirstOrDefaultAsync(o => o.ProductId == input.ProductId && o.SellerId == seller.Id, ct);

        if (offer is null)
        {
            offer = new SellerOffer
            {
                ProductId = input.ProductId,
                SellerId = seller.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.SellerOffers.Add(offer);
        }

        offer.Price = input.Price;
        offer.DiscountPercent = input.DiscountPercent;
        offer.Stock = input.Stock + offer.ReservedCount;
        offer.Warranty = input.Warranty;
        offer.DeliveryDays = input.DeliveryDays;
        offer.IsActive = true;
        offer.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return (true, "پیشنهاد فروش ذخیره شد.");
    }

    public async Task<(bool Success, string Message)> ToggleOfferAsync(Guid userId, long offerId, CancellationToken ct = default)
    {
        var offer = await db.SellerOffers
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == offerId && o.Seller.OwnerUserId == userId, ct);

        if (offer is null) return (false, "پیشنهاد یافت نشد.");

        offer.IsActive = !offer.IsActive;
        await db.SaveChangesAsync(ct);
        return (true, offer.IsActive ? "پیشنهاد فعال شد." : "پیشنهاد غیرفعال شد.");
    }

    public async Task<SellerOfferInput?> GetOfferForEditAsync(Guid userId, long offerId, CancellationToken ct = default)
    {
        var offer = await db.SellerOffers.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId && o.Seller.OwnerUserId == userId, ct);
        if (offer is null) return null;

        return new SellerOfferInput
        {
            ProductId = offer.ProductId,
            Price = offer.Price,
            DiscountPercent = offer.DiscountPercent,
            Stock = Math.Max(0, offer.Stock - offer.ReservedCount),
            Warranty = offer.Warranty,
            DeliveryDays = offer.DeliveryDays
        };
    }

    public async Task<(bool Success, string Message)> DeleteOfferAsync(Guid userId, long offerId, CancellationToken ct = default)
    {
        var offer = await db.SellerOffers
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == offerId && o.Seller.OwnerUserId == userId, ct);

        if (offer is null) return (false, "پیشنهاد یافت نشد.");

        // پاک‌سازی آیتم‌های سبد و رزروهای موجودی متصل به این آفر
        await db.CartItems.Where(ci => ci.OfferId == offer.Id).ExecuteDeleteAsync(ct);
        await db.StockReservations.Where(sr => sr.OfferId == offer.Id).ExecuteDeleteAsync(ct);

        db.SellerOffers.Remove(offer);
        await db.SaveChangesAsync(ct);
        return (true, "پیشنهاد فروش حذف شد.");
    }

    public async Task<(bool Success, string Message, long ProductId)> ProposeProductAsync(Guid userId, AdminProductViewModel model, CancellationToken ct = default)
    {
        var seller = await db.Sellers.FirstOrDefaultAsync(s => s.OwnerUserId == userId, ct);
        if (seller is null)
            return (false, "ابتدا فروشگاه خود را ثبت کنید.", 0);
        if (seller.Status != SellerStatus.Approved)
            return (false, "فروشگاه شما هنوز تأیید نشده است.", 0);

        var slug = SlugHelper.Slugify(string.IsNullOrWhiteSpace(model.Slug) ? model.Name : model.Slug);
        var baseSlug = slug;
        var i = 1;
        while (await db.Products.AnyAsync(p => p.Slug == slug, ct))
            slug = $"{baseSlug}-{++i}";

        var product = new Product
        {
            Name = model.Name.Trim(),
            EnglishName = model.EnglishName.Trim(),
            Slug = slug,
            Code = $"MK-{DateTime.UtcNow:yyMMddHHmmss}",
            UnitPrice = model.UnitPrice,
            DiscountPercent = model.DiscountPercent,
            Stock = 0, // موجودی در پیشنهاد فروشنده نگه داشته می‌شود
            Description = model.Description,
            CategoryId = model.CategoryId,
            BrandId = model.BrandId,
            MainImage = string.IsNullOrWhiteSpace(model.MainImage)
                ? "/images/products/real/" + new Random().Next(1, 11) + ".webp"
                : model.MainImage,
            AttributesJson = BuildProposalAttributesJson(model.AttrNames, model.AttrValues),
            IsActive = false, // تا تأیید ادمین
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        db.SellerOffers.Add(new SellerOffer
        {
            ProductId = product.Id,
            SellerId = seller.Id,
            Price = model.UnitPrice,
            DiscountPercent = model.DiscountPercent,
            Stock = model.Stock,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        return (true, "کالای جدید ثبت شد و پس از تأیید کارشناسان به فروش می‌رسد.", product.Id);
    }

    /// <summary>ساخت AttributesJson از ردیف‌های نام/مقدار فرم پیشنهاد فروشنده.</summary>
    private static string? BuildProposalAttributesJson(List<string>? names, List<string>? values)
    {
        if (names is null || names.Count == 0) return null;

        var groups = new List<object>();
        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i]?.Trim();
            var value = (i < values?.Count ? values[i] : null)?.Trim();
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) continue;

            groups.Add(new
            {
                name,
                values = value.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            });
        }

        return groups.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(groups);
    }

    public async Task<List<SellerOfferDto>> GetProductOffersAsync(long productId, CancellationToken ct = default)
    {
        var list = await QueryOffers()
            .Where(o => o.ProductId == productId)
            .OrderBy(o => o.FinalPrice)
            .ToListAsync(ct);

        if (list.Count > 0)
        {
            var cheapest = list.Min(l => l.FinalPrice);
            foreach (var l in list) l.IsCheapest = l.FinalPrice == cheapest;
        }

        return list;
    }

    private IQueryable<SellerOfferDto> QueryOffers() => db.SellerOffers
        .AsNoTracking()
        .Select(o => new SellerOfferDto
        {
            OfferId = o.Id,
            ProductId = o.ProductId,
            ProductName = o.Product.Name,
            SellerId = o.SellerId,
            SellerName = o.Seller.StoreName,
            SellerSlug = o.Seller.Slug,
            Price = o.Price,
            DiscountPercent = o.DiscountPercent,
            FinalPrice = o.DiscountPercent > 0 ? o.Price - o.Price * o.DiscountPercent / 100 : o.Price,
            Available = o.Stock - o.ReservedCount,
            Warranty = o.Warranty,
            DeliveryDays = o.DeliveryDays,
            IsActive = o.IsActive
        });
}

/// <summary>لیست مقایسه (کوکی‌محور، حداکثر ۴ کالا) — بدون state سمت سرور.</summary>
public class CompareService(IProductService products) : ICompareService
{
    public const string CookieName = "shop.compare";
    public const int MaxItems = 4;

    public (bool Added, string Message) Toggle(string? currentIds, long productId, out string newIds)
    {
        var ids = Parse(currentIds);

        if (ids.Contains(productId))
        {
            ids.Remove(productId);
            newIds = string.Join(",", ids);
            return (false, "از لیست مقایسه حذف شد.");
        }

        if (ids.Count >= MaxItems)
        {
            newIds = string.Join(",", ids);
            return (false, $"حداکثر {MaxItems} کالا قابل مقایسه است.");
        }

        ids.Add(productId);
        newIds = string.Join(",", ids);
        return (true, "به لیست مقایسه اضافه شد.");
    }

    public async Task<List<ProductCardDto>> GetCardsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds.Length == 0) return [];
        return await products.GetCardsByIdsAsync(productIds, ct);
    }

    public static List<long> Parse(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                 .Select(s => long.TryParse(s, out var id) ? id : 0)
                 .Where(id => id > 0)
                 .Distinct()
                 .ToList();
}

/// <summary>تاریخچه قیمت کالا + قیمت‌های فروشندگان (برای نمودارها).</summary>
public class PriceHistoryService(ApplicationDbContext db) : IPriceHistoryService
{
    public async Task RecordAsync(long productId, long finalPrice, CancellationToken ct = default)
    {
        db.PriceHistories.Add(new PriceHistory
        {
            ProductId = productId,
            Price = finalPrice,
            RecordedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<PricePointDto>> GetHistoryAsync(long productId, CancellationToken ct = default)
    {
        return await db.PriceHistories
            .AsNoTracking()
            .Where(h => h.ProductId == productId)
            .OrderBy(h => h.RecordedAt)
            .Select(h => new PricePointDto { Date = h.RecordedAt, Price = h.Price })
            .ToListAsync(ct);
    }

    public async Task<List<SellerOfferDto>> GetSellerPricesAsync(long productId, CancellationToken ct = default)
    {
        var list = await db.SellerOffers
            .AsNoTracking()
            .Where(o => o.ProductId == productId && o.IsActive && o.Seller.Status == SellerStatus.Approved && o.Seller.IsActive)
            .OrderBy(o => o.Price - o.Price * o.DiscountPercent / 100)
            .Select(o => new SellerOfferDto
            {
                OfferId = o.Id,
                ProductId = o.ProductId,
                SellerName = o.Seller.StoreName,
                SellerSlug = o.Seller.Slug,
                Price = o.Price,
                DiscountPercent = o.DiscountPercent,
                FinalPrice = o.DiscountPercent > 0 ? o.Price - o.Price * o.DiscountPercent / 100 : o.Price,
                Available = o.Stock - o.ReservedCount,
                DeliveryDays = o.DeliveryDays
            })
            .ToListAsync(ct);

        if (list.Count > 0)
        {
            var cheapest = list.Min(l => l.FinalPrice);
            foreach (var l in list) l.IsCheapest = l.FinalPrice == cheapest;
        }

        return list;
    }
}
