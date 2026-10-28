using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Webp;

namespace Shop.Presentation.Services;

/// <summary>
/// تولید یک‌باره بنرهای اسلایدر (۱۶۰۰×۶۰۰) — گرادیان مورب برند + هاله‌ها + حلقه + نوارهای مورب + وینیت.
/// محاسبه مستقیم پیکسل‌ها (بدون پکیج Drawing). اگر فایل‌ها موجود باشند کاری نمی‌کند.
/// </summary>
public static class BannerSeeder
{
    private record Theme(string Name, string C1, string C2, string C3, string Glow);

    private static readonly Theme[] Themes =
    [
        new("sale",    "#b3123c", "#ef4056", "#ff7a59", "#ffd166"),  // حراج — برند
        new("mobile",  "#1f2340", "#3b4370", "#6c8cd5", "#8ecae6"),  // موبایل و لپ‌تاپ
        new("home",    "#0f6b5c", "#14947f", "#52c7b0", "#c9f2e4"),  // لوازم خانگی
        new("fashion", "#4a1d6e", "#7b3fa0", "#c77dff", "#ffe3ff"),  // مد و پوشاک
    ];

    private static Rgba32 Hex(string h)
    {
        var v = Convert.ToUInt32(h.TrimStart('#'), 16);
        return new Rgba32((byte)(v >> 16), (byte)(v >> 8), (byte)v, 255);
    }

    public static void Ensure(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.WebRootPath ?? "wwwroot", "images", "banners");
        Directory.CreateDirectory(dir);

        foreach (var t in Themes)
        {
            var path = Path.Combine(dir, $"banner-{t.Name}.webp");
            if (File.Exists(path)) continue;

            var c1 = Hex(t.C1); var c2 = Hex(t.C2); var c3 = Hex(t.C3); var glow = Hex(t.Glow);
            const int W = 1600, H = 600;

            using var img = new Image<Rgba32>(W, H);
            img.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < H; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (var x = 0; x < W; x++)
                    {
                        // ۱) گرادیان مورب سه‌مرحله‌ای
                        var t2 = Math.Clamp(0.5f * (x / (float)W) + 0.5f * (1f - y / (float)H), 0f, 1f);
                        var px = t2 < 0.55f
                            ? Lerp(c1, c2, t2 / 0.55f)
                            : Lerp(c2, c3, (t2 - 0.55f) / 0.45f);

                        // ۲) هاله تم بالا-راست
                        px = Blend(px, glow, 110 * Falloff(x, y, 1230, 170, 340));

                        // ۳) هاله سفید پایین-چپ
                        px = Blend(px, new Rgba32(255, 255, 255, 255), 70 * Falloff(x, y, 340, 480, 230));

                        // ۴) حلقه تزئینی
                        var dr = Math.Abs(Dist(x, y, 1180, 300) - 176) / 24f;
                        if (dr < 1f)
                            px = Blend(px, new Rgba32(255, 255, 255, 255), 80 * (1f - dr));

                        // ۵) نوارهای مورب ظریف
                        var u = ((x - 900) * 0.7071f + (600 - y) * 0.7071f) / 600f;
                        var sA = u is > 0.42f and < 0.62f ? BandAlpha(u, 0.42f, 0.46f, 0.58f, 0.62f, 26) : 0;
                        px = Blend(px, new Rgba32(255, 255, 255, 255), sA);
                        var u2 = ((x - 1050) * 0.80f + (600 - y) * 0.60f) / 550f;
                        var s2 = u2 is > 0.55f and < 0.77f ? BandAlpha(u2, 0.55f, 0.60f, 0.72f, 0.77f, 16) : 0;
                        px = Blend(px, new Rgba32(255, 255, 255, 255), s2);

                        // ۶) وینیت پایین برای خوانایی متن
                        var v = Math.Clamp((y - 300) / 300f, 0f, 1f);
                        px = Blend(px, new Rgba32(15, 15, 30, 255), 95 * v);

                        row[x] = px;
                    }
                }
            });

            using var fs = File.Create(path);
            img.Save(fs, new WebpEncoder { Quality = 82 });
        }
    }

    private static float Dist(int x, int y, int cx, int cy) =>
        MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

    /// <summary>۰ در لبه بیرونی → ۱ در مرکز.</summary>
    private static float Falloff(int x, int y, int cx, int cy, int radius)
    {
        var d = Dist(x, y, cx, cy) / radius;
        return d >= 1f ? 0f : MathF.Pow(1f - d, 1.6f);
    }

    /// <summary>آلفای نوار با شیب ورود/خروج.</summary>
    private static float BandAlpha(float u, float in0, float in1, float out0, float out1, float max)
    {
        if (u <= in0 || u >= out1) return 0;
        if (u < in1) return max * (u - in0) / (in1 - in0);
        if (u > out0) return max * (out1 - u) / (out1 - out0);
        return max;
    }

    private static Rgba32 Lerp(Rgba32 a, Rgba32 b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Rgba32(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t), 255);
    }

    /// <summary>blend دستی src روی dst با آلفای ۰..۲۵۵.</summary>
    private static Rgba32 Blend(Rgba32 dst, Rgba32 src, float alpha)
    {
        if (alpha <= 0) return dst;
        if (alpha > 255) alpha = 255;
        var a = alpha / 255f;
        return new Rgba32(
            (byte)(dst.R + (src.R - dst.R) * a),
            (byte)(dst.G + (src.G - dst.G) * a),
            (byte)(dst.B + (src.B - dst.B) * a), 255);
    }
}
