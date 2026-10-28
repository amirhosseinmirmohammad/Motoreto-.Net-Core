using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductCardDto>> GetFilteredAsync(ProductFilter filter, CancellationToken ct = default);

    Task<ProductDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task IncrementViewAsync(long productId, CancellationToken ct = default);

    /// <summary>پیشنهاد شگفت‌انگیز (با تایمر).</summary>
    Task<List<ProductCardDto>> GetAmazingAsync(int count, CancellationToken ct = default);

    Task<List<ProductCardDto>> GetSpecialAsync(int count, CancellationToken ct = default);

    Task<List<ProductCardDto>> GetNewestAsync(int count, CancellationToken ct = default);

    Task<List<ProductCardDto>> GetBestSellersAsync(int count, CancellationToken ct = default);

    /// <summary>پربازدیدترین‌ها (بر اساس ViewCount — دیتای متمایز برای سکشن‌های صفحه اصلی).</summary>
    Task<List<ProductCardDto>> GetMostViewedAsync(int count, CancellationToken ct = default);

    Task<List<ProductCardDto>> GetRelatedAsync(long productId, int categoryId, int count, CancellationToken ct = default);

    Task<int> GetActiveCountAsync(CancellationToken ct = default);

    /// <summary>کارت چند محصول با حفظ ترتیب (لیست مقایسه).</summary>
    Task<List<ProductCardDto>> GetCardsByIdsAsync(long[] productIds, CancellationToken ct = default);

    /// <summary>
    /// Facets سایدبار فیلترها (برند + ویژگی‌های داینامیک دسته‌بندی با تعداد).
    /// فیلترهای برند/ویژگیِ انتخاب‌شده در محاسبه لحاظ نمی‌شوند (رفتار مارکت‌پلیس).
    /// </summary>
    Task<ProductFacetsDto?> GetFacetsAsync(ProductFilter filter, CancellationToken ct = default);
}

public interface ICategoryService
{
    /// <summary>درخت دسته‌ها برای منو (کش‌شده).</summary>
    Task<List<CategoryNodeDto>> GetMenuAsync(CancellationToken ct = default);

    Task<(int CategoryId, string Name)?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>شناسه دسته + همه زیر‌دسته‌هایش (برای فیلتر).</summary>
    Task<List<int>> GetCategoryWithChildrenIdsAsync(int categoryId, CancellationToken ct = default);

    void InvalidateCache();
}
