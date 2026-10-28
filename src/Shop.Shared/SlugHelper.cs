using System.Text;
using System.Text.RegularExpressions;

namespace Shop.Shared;

/// <summary>ساخت اسلاگ یکتا و سئو-پسند؛ از حروف فارسی پشتیبانی می‌کند.</summary>
public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\u0600-\u06FF\-]+")]
    private static partial Regex InvalidCharsRegex();

    /// <summary>
    /// مثال: «گوشی موبایل سامسونگ A55» → «گوشی-موبایل-سامسونگ-a55»
    /// </summary>
    public static string Slugify(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalized = text.Trim().ToLowerInvariant();

        // نیم‌فاصله و فاصله‌های تکراری → خط تیره
        normalized = normalized.Replace('\u200c', '-');
        normalized = Regex.Replace(normalized, @"\s+", "-");

        // حذف کاراکترهای غیرمجاز
        normalized = InvalidCharsRegex().Replace(normalized, string.Empty);

        // خط تیره‌های تکراری
        normalized = Regex.Replace(normalized, @"-+", "-").Trim('-');

        return normalized;
    }
}
