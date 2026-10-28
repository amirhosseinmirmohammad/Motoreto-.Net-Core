using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using System.Text;
using System.Xml.Linq;

namespace Shop.Presentation.Controllers;

/// <summary>
/// فایل‌های استاندارد موتورهای جستجو و هوش مصنوعی:
/// robots.txt (SEO + اجازه کرالرهای AI)، sitemap.xml، llms.txt (GEO).
/// </summary>
public class SeoController(
    IProductService products,
    ICategoryService categories,
    IBlogService blogs,
    ISettingsService settings) : Controller
{
    [HttpGet("/robots.txt")]
    [OutputCache(PolicyName = "SeoCache")]
    public async Task<IActionResult> Robots(CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /admin");
        sb.AppendLine("Disallow: /panel");
        sb.AppendLine("Disallow: /cart");
        sb.AppendLine("Disallow: /checkout");
        sb.AppendLine("Disallow: /payment");
        sb.AppendLine("Disallow: /api/");
        sb.AppendLine("Disallow: /account");
        sb.AppendLine();
        sb.AppendLine("# کرالرهای هوش مصنوعی خوش‌آمدید (AEO/GEO)");
        sb.AppendLine("User-agent: GPTBot");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine("User-agent: ClaudeBot");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine("User-agent: PerplexityBot");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine("User-agent: Google-Extended");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {BaseUrl(ct)}/sitemap.xml");

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("/llms.txt")]
    [OutputCache(PolicyName = "SeoCache")]
    public async Task<IActionResult> Llms(CancellationToken ct)
    {
        var config = await settings.GetAsync(ct);
        var sb = new StringBuilder();

        sb.AppendLine($"# {config.StoreName}");
        sb.AppendLine();
        sb.AppendLine($"> {config.StoreName} یک فروشگاه اینترنتی ایرانی است که " +
                      "بیش از ۶۰ کالا در ۸ دستهٔ اصلی (موبایل، لپ‌تاپ و کامپیوتر، لوازم خانگی، مد و پوشاک، " +
                      "آرایشی و بهداشتی، ورزش و سفر، کتاب، اسباب‌بازی) را با ارسال به سراسر ایران عرضه می‌کند. " +
                      "پرداخت اینترنتی و پرداخت در محل، ۷ روز ضمانت بازگشت کالا.");
        sb.AppendLine();
        sb.AppendLine("## لینک‌های کلیدی");
        sb.AppendLine();
        sb.AppendLine("- [همه محصولات](/products): فهرست کامل با فیلتر قیمت و دسته‌بندی");
        sb.AppendLine("- [پیشنهاد شگفت‌انگیز](/products?sort=discount): تخفیف‌دارترین کالاها");
        sb.AppendLine("- [وبلاگ](/blog): راهنمای خرید");
        sb.AppendLine("- [سوالات متداول](/faq): ارسال، بازگشت کالا، پرداخت در محل");
        sb.AppendLine($"- [تماس](/contact): تلفن پشتیبانی {config.SupportPhone}");
        sb.AppendLine();
        sb.AppendLine("## سیاست‌ها");
        sb.AppendLine();
        sb.AppendLine("- ارسال: پست پیشتاز ۴ تا ۷ روز کاری؛ اکسپرس ۲۴ تا ۴۸ ساعت");
        sb.AppendLine("- بازگشت کالا: تا ۷ روز پس از تحویل");
        sb.AppendLine("- پرداخت: اینترنتی (درگاه امن) یا در محل");
        sb.AppendLine();

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [OutputCache(PolicyName = "SeoCache")]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var baseUrl = BaseUrl(ct);
        var ns = (XNamespace)"http://www.sitemaps.org/schemas/sitemap/0.9";

        var urls = new List<(string Loc, string? LastMod)>
        {
            ($"{baseUrl}/", null),
            ($"{baseUrl}/products", null),
            ($"{baseUrl}/blog", null),
            ($"{baseUrl}/about", null),
            ($"{baseUrl}/contact", null),
            ($"{baseUrl}/faq", null),
            ($"{baseUrl}/terms", null)
        };

        // محصولات
        var filter = new ProductFilter { Page = 1, PageSize = 48 };
        for (var page = 1; page <= 10; page++)
        {
            filter.Page = page;
            var result = await products.GetFilteredAsync(filter, ct);
            foreach (var p in result.Items)
                urls.Add(($"{baseUrl}/p/{Uri.EscapeDataString(p.Slug)}", null));

            if (page >= result.TotalPages) break;
        }

        // دسته‌ها
        var menu = await categories.GetMenuAsync(ct);
        foreach (var root in menu)
        {
            urls.Add(($"{baseUrl}/products?category={Uri.EscapeDataString(root.Slug)}", null));
            foreach (var child in root.Children)
                urls.Add(($"{baseUrl}/products?category={Uri.EscapeDataString(child.Slug)}", null));
        }

        // وبلاگ
        var posts = await blogs.GetVisibleAsync(1, 24, ct);
        foreach (var post in posts.Items)
            urls.Add(($"{baseUrl}/blog/{Uri.EscapeDataString(post.Slug)}", null));

        var items = urls.Select(u => new XElement(ns + "url",
            new XElement(ns + "loc", u.Loc),
            u.LastMod != null ? new XElement(ns + "lastmod", u.LastMod) : null));

        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset", items));

        return Content(xml.Declaration + Environment.NewLine + xml, "application/xml", Encoding.UTF8);
    }

    private string BaseUrl(CancellationToken ct)
    {
        var request = Request;
        var scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? request.Scheme;
        var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.Value;
        return $"{scheme}://{host}";
    }
}
