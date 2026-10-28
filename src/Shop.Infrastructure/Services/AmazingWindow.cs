using Shop.Shared;

namespace Shop.Infrastructure.Services;

/// <summary>وضعیت پنجره زمانی شگفت‌انگیز در لحظه.</summary>
public sealed record AmazingWindowState(bool Active, DateTime? EndUtc);

/// <summary>
/// زمان‌بندی پیشنهاد شگفت‌انگیز: بازه ساعتی روزانه به وقت ایران (مثل دیجی‌کالا).
/// پایان کمتر از شروع = بازه شبانه (مثلاً ۲۲:۰۰ تا ۰۹:۰۰).
/// زمان‌بندی خاموش = همیشه فعال (رفتار قبلی) و شمارش‌معکوس خالی.
/// </summary>
public static class AmazingWindow
{
    public static AmazingWindowState Resolve(bool enabled, int startMinutes, int endMinutes, DateTime utcNow)
    {
        if (!enabled) return new AmazingWindowState(true, null);

        var iran = PersianDateHelper.ToIranTime(utcNow);
        var nowMin = iran.Hour * 60 + iran.Minute;

        var start = Math.Clamp(startMinutes, 0, 1439);
        var end = Math.Clamp(endMinutes == 0 ? 1440 : endMinutes, 1, 1440);

        var inWindow = start <= end
            ? nowMin >= start && nowMin < end
            : nowMin >= start || nowMin < end; // بازه شبانه (از شروع امشب تا پایان فردا)

        if (!inWindow) return new AmazingWindowState(false, null);

        // پایان بازه: اگر هنوز به ساعت پایان نرسیده‌ایم → امروز، وگرنه → فردا
        var endLocal = nowMin < end ? iran.Date.AddMinutes(end) : iran.Date.AddDays(1).AddMinutes(end);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), PersianDateHelper.IranTimeZone);
        return new AmazingWindowState(true, endUtc);
    }
}
