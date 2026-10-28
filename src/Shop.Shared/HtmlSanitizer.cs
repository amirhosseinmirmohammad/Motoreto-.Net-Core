using System.Text.RegularExpressions;

namespace Shop.Shared;

/// <summary>پاک‌سازی HTML از اسکریپت و رویدادهای خطرناک (ضد XSS).</summary>
public static partial class HtmlSanitizer
{
    /// <summary>تگ‌های مجاز (سفید‌لیست) — بقیه حذف می‌شوند.</summary>
    private static readonly string[] AllowedTags =
    [
        "p","br","b","strong","i","em","u","s","h1","h2","h3","h4","h5","h6",
        "ul","ol","li","blockquote","table","thead","tbody","tr","th","td",
        "a","img","span","div","hr","sub","sup","figure","figcaption"
    ];

    /// <summary>HTML خام را به نسخه امن تبدیل می‌کند (بدون script/iframe/event-handler/javascript:).</summary>
    public static string Sanitize(this string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var clean = html;

        // حذف کامل بلوک‌های خطرناک
        clean = ScriptTag().Replace(clean, "");
        clean = IframeTag().Replace(clean, "");
        clean = ObjectTag().Replace(clean, "");
        clean = EmbedTag().Replace(clean, "");
        clean = FormTag().Replace(clean, "");

        // حذف event handlers: onclick="..."، onerror=... و ...
        clean = EventHandler().Replace(clean, "");

        // حذف javascript: و data: (به‌جز data:image)
        clean = Regex.Replace(clean, @"(href|src)\s*=\s*[""']?\s*(javascript|vbscript)[^""']*", "$1=\"\"", RegexOptions.IgnoreCase);
        clean = Regex.Replace(clean, @"(href|src)\s*=\s*[""']?\s*data:(?!image)", "$1=\"\"", RegexOptions.IgnoreCase);

        // حذف تگ‌های غیرمجاز (محتوایشان حفظ می‌شود)
        foreach (var tag in new[] { "script", "iframe", "object", "embed", "form", "style", "link", "meta" })
            clean = Regex.Replace(clean, $"</?{tag}[^>]*>", "", RegexOptions.IgnoreCase);

        return clean;
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTag();

    [GeneratedRegex(@"<iframe[^>]*>[\s\S]*?</iframe>", RegexOptions.IgnoreCase)]
    private static partial Regex IframeTag();

    [GeneratedRegex(@"<object[^>]*>[\s\S]*?</object>", RegexOptions.IgnoreCase)]
    private static partial Regex ObjectTag();

    [GeneratedRegex(@"<embed[^>]*>[\s\S]*?</embed>", RegexOptions.IgnoreCase)]
    private static partial Regex EmbedTag();

    [GeneratedRegex(@"<form[^>]*>[\s\S]*?</form>", RegexOptions.IgnoreCase)]
    private static partial Regex FormTag();

    [GeneratedRegex(@"\s+on\w+\s*=\s*("".*?""|'.*?'|[^\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandler();
}
