namespace Shop.Infrastructure.Services;

/// <summary>
/// محاسبه ارسال چندفروشنده‌ای: هر فروشنده (و کالاهای خود مارکت) یک مرسوله مستقل است.
/// تعرفه و سقف ارسال رایگان اختصاصی فروشنده (از /seller/settings) روی ارسال پستی اعمال می‌شود؛
/// اکسپرس همیشه با تعرفه مارکت و به تعداد مرسوله‌ها محاسبه می‌شود.
/// </summary>
public static class MarketplaceShipping
{
    /// <summary>هزینه ارسال پستی یک مرسوله — اگر سبد گروه از سقف (اختیاری فروشنده) گذشت، رایگان.</summary>
    public static long PostCost(long groupSubtotal, long? costOverride, long? freeThresholdOverride, long marketPostCost, long marketFreeThreshold)
    {
        var threshold = freeThresholdOverride ?? marketFreeThreshold;
        if (threshold > 0 && groupSubtotal >= threshold) return 0;
        return costOverride ?? marketPostCost;
    }
}

/// <summary>یک مرسوله: گروهی از اقلام یک فروشنده (یا خود مارکت).</summary>
public sealed record ShipmentGroup(long Subtotal, long? CostOverride, long? FreeOverride);
