using System.Globalization;

namespace Shop.Shared;

/// <summary>ابزارهای تاریخ شمسی و اعداد فارسی.</summary>
public static class PersianDateHelper
{
    private static readonly PersianCalendar Pc = new();
    private static readonly CultureInfo Fa = CultureInfo.GetCultureInfo("fa-IR");
    public static readonly TimeZoneInfo IranTimeZone = SafeIranTimeZone();

    private static TimeZoneInfo SafeIranTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran"); }
        catch (TimeZoneNotFoundException)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"); }
            catch { return TimeZoneInfo.Local; }
        }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Local; }
    }

    /// <summary>تبدیل UTC به ساعت ایران.</summary>
    public static DateTime ToIranTime(this DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), IranTimeZone);

    /// <summary>مثال: ۱۴۰۵/۰۸/۰۵</summary>
    public static string ToPersianDate(this DateTime utc)
    {
        var iran = utc.ToIranTime();
        return $"{Pc.GetYear(iran):0000}/{Pc.GetMonth(iran):00}/{Pc.GetDayOfMonth(iran):00}".ToFarsiDigits();
    }

    /// <summary>مثال: ۱۴۰۵/۰۸/۰۵ - ۱۴:۳۰</summary>
    public static string ToPersianDateTime(this DateTime utc)
    {
        var iran = utc.ToIranTime();
        return $"{Pc.GetYear(iran):0000}/{Pc.GetMonth(iran):00}/{Pc.GetDayOfMonth(iran):00} - {iran:HH:mm}".ToFarsiDigits();
    }

    /// <summary>نام ماه شمسی.</summary>
    public static string GetPersianMonthName(DateTime utc)
    {
        var iran = utc.ToIranTime();
        string[] months =
        [
            "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        ];
        return months[Pc.GetMonth(iran) - 1];
    }

    /// <summary>مثال: ۵ آبان ۱۴۰۵</summary>
    public static string ToPersianLongDate(this DateTime utc)
    {
        var iran = utc.ToIranTime();
        return $"{Pc.GetDayOfMonth(iran)} {GetPersianMonthName(utc)} {Pc.GetYear(iran)}".ToFarsiDigits();
    }

    /// <summary>«چند لحظه پیش»، «۳ روز پیش» و...</summary>
    public static string ToRelativeTime(this DateTime utc)
    {
        var iran = utc.ToIranTime();
        var diff = DateTime.Now - iran;
        return (diff.TotalMinutes switch
        {
            < 1 => "لحظاتی پیش",
            < 60 => $"{(int)diff.TotalMinutes} دقیقه پیش",
            _ when diff.TotalHours < 24 => $"{(int)diff.TotalHours} ساعت پیش",
            _ when diff.TotalDays < 30 => $"{(int)diff.TotalDays} روز پیش",
            _ when diff.TotalDays < 365 => $"{(int)(diff.TotalDays / 30)} ماه پیش",
            _ => $"{(int)(diff.TotalDays / 365)} سال پیش"
        }).ToFarsiDigits();
    }

    /// <summary>تبدیل ارقام لاتین به فارسی.</summary>
    public static string ToFarsiDigits(this string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input
            .Replace('0', '۰').Replace('1', '۱').Replace('2', '۲').Replace('3', '۳').Replace('4', '۴')
            .Replace('5', '۵').Replace('6', '۶').Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
    }

    /// <summary>تبدیل ارقام فارسی/عربی به لاتین (برای پارس ورودی).</summary>
    public static string ToLatinDigits(this string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var sb = new System.Text.StringBuilder(input.Length);
        foreach (var ch in input)
        {
            sb.Append(ch switch
            {
                >= '۰' and <= '۹' => (char)(ch - '۰' + '0'),
                >= '٠' and <= '٩' => (char)(ch - '٠' + '0'),
                _ => ch
            });
        }
        return sb.ToString();
    }

    /// <summary>مثال: ۱٬۲۰۰٬۰۰۰</summary>
    public static string ToGroupedNumber(this long value) =>
        value.ToString("N0", Fa);

    /// <summary>مثال: ۱٬۲۰۰٬۰۰۰ تومان</summary>
    public static string ToToman(this long amount)
    {
        // قیمت‌ها در کل سیستم به تومان ذخیره می‌شوند — دیگر تبدیل ریال نداریم
        return $"{amount:N0} تومان".ToFarsiDigits();
    }
}
