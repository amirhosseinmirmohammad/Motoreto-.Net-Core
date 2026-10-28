namespace Shop.Domain;

/// <summary>مقاله وبلاگ فروشگاه.</summary>
public class Blog
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;

    public string ShortDescription { get; set; } = default!;
    public string Body { get; set; } = default!;

    /// <summary>تصویر کاور.</summary>
    public string CoverImage { get; set; } = default!;

    public bool IsVisible { get; set; } = true;

    public long ViewCount { get; set; }

    public Guid AuthorId { get; set; }
    public virtual User? Author { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual List<BlogComment> Comments { get; set; } = [];
}

/// <summary>اسلایدر صفحه اصلی.</summary>
public class Slider
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;
    public string? SubTitle { get; set; }
    public string Image { get; set; } = default!;
    public string? Link { get; set; }

    /// <summary>رنگ پس‌زمینه (hex) برای هماهنگی گرافیک.</summary>
    public string? Theme { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
