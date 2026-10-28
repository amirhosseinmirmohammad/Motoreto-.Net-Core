using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shop.Shared;

/// <summary>
/// ساخت JSON-LD ساختارمند برای موتورهای جستجو (SEO)، دستیارهای هوشمند (AEO)
/// و موتورهای مولد مثل ChatGPT/Perplexity (GEO).
/// </summary>
public static class SeoJsonLd
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static Dictionary<string, object?> Ctx(string type) => new()
    {
        ["@context"] = "https://schema.org",
        ["@type"] = type
    };

    public static string Organization(string name, string url, string? logo, string? phone, string? email, string? instagram, string? telegram)
    {
        var sameAs = new List<string>();
        if (!string.IsNullOrEmpty(instagram)) sameAs.Add(instagram);
        if (!string.IsNullOrEmpty(telegram)) sameAs.Add(telegram);

        var obj = Ctx("OnlineStore");
        obj["name"] = name;
        obj["url"] = url;
        obj["logo"] = logo;
        obj["email"] = email;
        obj["telephone"] = phone;
        obj["sameAs"] = sameAs;
        obj["address"] = new Dictionary<string, object?> { ["@type"] = "PostalAddress", ["addressCountry"] = "IR" };

        return Serialize(obj);
    }

    public static string WebSiteWithSearch(string name, string url)
    {
        var obj = Ctx("WebSite");
        obj["name"] = name;
        obj["url"] = url;
        obj["potentialAction"] = new Dictionary<string, object?>
        {
            ["@type"] = "SearchAction",
            ["target"] = new Dictionary<string, object?>
            {
                ["@type"] = "EntryPoint",
                ["urlTemplate"] = $"{url}/products?q={{search_term_string}}"
            },
            ["query-input"] = "required name=search_term_string"
        };
        return Serialize(obj);
    }

    /// <summary>Product + Offer + امتیاز + نظرات — قلب سئوی صفحه محصول.</summary>
    public static string Product(
        string name, string description, string url, string image,
        string sku, string brand, long price, bool inStock,
        double? rating, int? reviewCount, IEnumerable<(string Author, string Text, double Score)>? reviews)
    {
        var offers = new Dictionary<string, object?>
        {
            ["@type"] = "Offer",
            ["price"] = price,
            ["priceCurrency"] = "IRR",
            ["availability"] = inStock ? "https://schema.org/InStock" : "https://schema.org/OutOfStock",
            ["url"] = url,
            ["itemCondition"] = "https://schema.org/NewCondition"
        };

        var obj = Ctx("Product");
        obj["name"] = name;
        obj["description"] = Truncate(description, 300);
        obj["image"] = image;
        obj["sku"] = sku;
        obj["brand"] = new Dictionary<string, object?> { ["@type"] = "Brand", ["name"] = brand };
        obj["offers"] = offers;

        if (rating is > 0 && reviewCount is > 0)
        {
            obj["aggregateRating"] = new Dictionary<string, object?>
            {
                ["@type"] = "AggregateRating",
                ["ratingValue"] = rating,
                ["reviewCount"] = reviewCount
            };
        }

        if (reviews is not null)
        {
            obj["review"] = reviews.Take(10).Select(r => new Dictionary<string, object?>
            {
                ["@type"] = "Review",
                ["author"] = new Dictionary<string, object?> { ["@type"] = "Person", ["name"] = r.Author },
                ["reviewBody"] = Truncate(r.Text, 500),
                ["reviewRating"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Rating",
                    ["ratingValue"] = r.Score,
                    ["bestRating"] = 5
                }
            }).ToList();
        }

        return Serialize(obj);
    }

    public static string BreadCrumb(IEnumerable<(string Name, string? Url)> items)
    {
        var obj = Ctx("BreadcrumbList");
        obj["itemListElement"] = items.Select((item, i) => new Dictionary<string, object?>
        {
            ["@type"] = "ListItem",
            ["position"] = i + 1,
            ["name"] = item.Name,
            ["item"] = item.Url
        }).ToList();
        return Serialize(obj);
    }

    /// <summary>AEO: سوالات متداول ساختارمند برای دستیارهای هوشمند.</summary>
    public static string FaqPage(IEnumerable<(string Question, string Answer)> faqs)
    {
        var obj = Ctx("FAQPage");
        obj["mainEntity"] = faqs.Select(f => new Dictionary<string, object?>
        {
            ["@type"] = "Question",
            ["name"] = f.Question,
            ["acceptedAnswer"] = new Dictionary<string, object?>
            {
                ["@type"] = "Answer",
                ["text"] = f.Answer
            }
        }).ToList();
        return Serialize(obj);
    }

    public static string BlogPosting(string title, string description, string url, string image, DateTime datePublished, string authorName)
    {
        var obj = Ctx("BlogPosting");
        obj["headline"] = Truncate(title, 110);
        obj["description"] = Truncate(description, 300);
        obj["image"] = image;
        obj["url"] = url;
        obj["datePublished"] = datePublished.ToString("yyyy-MM-ddTHH:mm:ssZ");
        obj["author"] = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = authorName };
        obj["publisher"] = new Dictionary<string, object?> { ["@type"] = "Organization", ["name"] = authorName };
        return Serialize(obj);
    }

    public static string ItemList(string name, IEnumerable<(string Name, string Url)> items)
    {
        var obj = Ctx("ItemList");
        obj["name"] = name;
        obj["itemListElement"] = items.Select((item, i) => new Dictionary<string, object?>
        {
            ["@type"] = "ListItem",
            ["position"] = i + 1,
            ["name"] = item.Name,
            ["url"] = item.Url
        }).ToList();
        return Serialize(obj);
    }

    private static string Truncate(string text, int max) =>
        string.IsNullOrEmpty(text) ? string.Empty
        : text.Length <= max ? text
        : text[..max] + "…";

    private static string Serialize(object obj) => JsonSerializer.Serialize(obj, Options);
}
