using System.Security.Cryptography;
using System.Text;

namespace Shop.Infrastructure.Seeding;

/// <summary>
/// Factory Pattern: تولید تصاویر SVG باکیفیت و سبک برای محصولات/اسلایدرها/وبلاگ —
/// بدون وابستگی خارجی؛ هر تصویر زیر ۲ کیلوبایت است.
/// </summary>
public static class SvgImageFactory
{
    private static readonly (string Fa, string Hex)[] Palettes =
    [
        ("فیروزه‌ای", "#0ea5e9"), ("بنفش", "#8b5cf6"), ("صورتی", "#ec4899"),
        ("نارنجی", "#f97316"), ("سبز", "#10b981"), ("آبی", "#3b82f6"),
        ("قرمز", "#ef4444"), ("زرد", "#eab308"), ("تیل", "#14b8a6")
    ];

    /// <summary>تصویر محصول: گرادیان دو رنگ + نام محصول.</summary>
    public static string ProductImage(string name, string subtitle)
    {
        var (c1, c2) = ColorsFor(name);
        var safeName = Escape(name);
        var safeSub = Escape(subtitle);

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="600" height="600" viewBox="0 0 600 600">
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="{c1}"/>
      <stop offset="1" stop-color="{c2}"/>
    </linearGradient>
  </defs>
  <rect width="600" height="600" fill="url(#g)" rx="24"/>
  <circle cx="300" cy="240" r="130" fill="#ffffff" opacity="0.16"/>
  <circle cx="300" cy="240" r="92" fill="#ffffff" opacity="0.22"/>
  <text x="300" y="258" font-family="Vazirmatn, Tahoma, sans-serif" font-size="64" fill="#ffffff" text-anchor="middle" font-weight="bold">🛍️</text>
  <text x="300" y="450" font-family="Vazirmatn, Tahoma, sans-serif" font-size="34" fill="#ffffff" text-anchor="middle" font-weight="bold" direction="rtl">{safeName}</text>
  <text x="300" y="500" font-family="Vazirmatn, Tahoma, sans-serif" font-size="22" fill="#ffffff" opacity="0.85" text-anchor="middle">{safeSub}</text>
</svg>
""";
    }

    /// <summary>تصویر اسلایدر عریض با عنوان بزرگ.</summary>
    public static string SliderImage(string title, string subtitle, string theme)
    {
        var safeTitle = Escape(title);
        var safeSub = Escape(subtitle);

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="420" viewBox="0 0 1200 420">
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="{theme}"/>
      <stop offset="1" stop-color="#111827"/>
    </linearGradient>
  </defs>
  <rect width="1200" height="420" fill="url(#g)"/>
  <circle cx="1050" cy="80" r="180" fill="#ffffff" opacity="0.08"/>
  <circle cx="120" cy="360" r="140" fill="#ffffff" opacity="0.06"/>
  <text x="600" y="190" font-family="Vazirmatn, Tahoma, sans-serif" font-size="52" fill="#ffffff" text-anchor="middle" font-weight="bold" direction="rtl">{safeTitle}</text>
  <text x="600" y="250" font-family="Vazirmatn, Tahoma, sans-serif" font-size="24" fill="#ffffff" opacity="0.85" text-anchor="middle" direction="rtl">{safeSub}</text>
</svg>
""";
    }

    /// <summary>تصویر کاور مقاله وبلاگ.</summary>
    public static string BlogImage(string title)
    {
        var (c1, c2) = ColorsFor(title);
        var safeTitle = Escape(title.Length > 40 ? title[..40] + "…" : title);

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="800" height="420" viewBox="0 0 800 420">
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="{c1}"/>
      <stop offset="1" stop-color="{c2}"/>
    </linearGradient>
  </defs>
  <rect width="800" height="420" fill="url(#g)" rx="16"/>
  <text x="400" y="210" font-family="Vazirmatn, Tahoma, sans-serif" font-size="30" fill="#ffffff" text-anchor="middle" font-weight="bold" direction="rtl">📝</text>
  <text x="400" y="270" font-family="Vazirmatn, Tahoma, sans-serif" font-size="26" fill="#ffffff" text-anchor="middle" font-weight="bold" direction="rtl">{safeTitle}</text>
</svg>
""";
    }

    /// <summary>ذخیره فایل SVG در مسیر مقصد (اگر نباشد).</summary>
    public static string Save(string wwwRootRelativeDir, string fileName, string svg, string contentRootPath)
    {
        var dir = Path.Combine(contentRootPath, "wwwroot", wwwRootRelativeDir.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, fileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, svg, new UTF8Encoding(false));
        }

        return $"/{wwwRootRelativeDir}/{fileName}";
    }

    private static (string C1, string C2) ColorsFor(string seed)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(seed));
        var i1 = hash[0] % Palettes.Length;
        var i2 = (i1 + 1 + hash[1] % (Palettes.Length - 1)) % Palettes.Length;
        return (Palettes[i1].Hex, Palettes[i2].Hex);
    }

    private static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
