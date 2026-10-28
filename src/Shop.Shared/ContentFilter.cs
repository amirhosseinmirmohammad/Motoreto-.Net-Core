using System.Text.RegularExpressions;

namespace Shop.Shared;

/// <summary>
/// فیلتر محتوای غیراخلاقی — قانون پروژه: هیچ محتوای جنسی/پورن نباید در هیچ بخشی منتشر شود.
/// لایه اول: بلاک‌کردن واژه‌های ممنوعه در ورودی‌های عمومی (آگهی، چت، نظرات).
/// لایه دوم: تأیید دستی ادمین (آگهی‌ها Pending→Approved می‌مانند).
/// </summary>
public static partial class ContentFilter
{
    [GeneratedRegex(@"[\u200c\s\.\-_!*@#\$%\^&\(\)\[\]{}،,؛;:'""؟?\\/]")]
    private static partial Regex Separator();

    private static readonly string[] BannedWords =
    [
        // فارسی
        "سکس", "سکسی", "پورن", "پورنو", "هرزه", "جنده", "فاحشه", "روسپی",
        "کون", "کوس", "جق", "حشری", "شهوتی", "لاچی", "لاچین", "کسده",
        "فیلم سوپر", "سوپر ايرانی", "سوپر ایرانی", "سایت سکس", "دختر همراهی",
        "همراهی شبانه", "همدمی", "مشروط", "نیمه مشروط", "ارباب و برده",
        // لاتین / رایج در آگهی‌های متخلفانه
        "sex", "sexy", "porn", "porno", "xxx", "escort", "esc0rt",
        "nude", "nudes", "hooker", "prostitute", "erotic", "horny",
        "cam show", "webcam", "onlyfans", "nudes4sale"
    ];

    /// <summary>واژه‌های ممنوعه را با حذف نویسه‌های جداکننده می‌یابد (مثل سـکـس یا s.e.x).</summary>
    public static bool IsClean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        var normalized = Separator().Replace(text, string.Empty).ToLowerInvariant();
        return !BannedWords.Any(w => normalized.Contains(w.Replace(" ", "").ToLowerInvariant()));
    }

    /// <summary>اگر متن تمیز نبود، پیام خطای مناسب برمی‌گرداند؛ وگرنه null.</summary>
    public static string? ViolationMessage(string? text) => IsClean(text)
        ? null
        : "محتوای شما حاوی واژه‌های نامناسب است. انتشار محتوای غیراخلاقی در سایت ممنوع است.";
}
