namespace Shop.Domain;

/// <summary>نظر کاربر روی محصول.</summary>
public class Comment
{
    public long Id { get; set; }

    public long ProductId { get; set; }
    public virtual Product Product { get; set; } = default!;

    public Guid UserId { get; set; }
    public virtual User User { get; set; } = default!;

    /// <summary>امتیاز ۱ تا ۵.</summary>
    public byte Rating { get; set; }

    /// <summary>مثبت / نقاط ضعف / متن نظر.</summary>
    public string Text { get; set; } = default!;

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>نظر کاربر روی مقاله وبلاگ.</summary>
public class BlogComment
{
    public long Id { get; set; }

    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public string Text { get; set; } = default!;

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
}
