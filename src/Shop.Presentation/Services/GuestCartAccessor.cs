namespace Shop.Presentation.Services;

/// <summary>دسترسی یکنواخت به توکن سبد مهمان (کوکی). SRP: تنها نقطه مدیریت کوکی سبد.</summary>
public interface IGuestCartAccessor
{
    /// <summary>توکن فعلی مهمان؛ در صورت نبود، می‌سازد و در Response ثبت می‌کند.</summary>
    string GetOrCreateToken();

    /// <summary>توکن مهمان (بدون ساخت).</summary>
    string? GetToken();
}

public class GuestCartAccessor(IHttpContextAccessor accessor) : IGuestCartAccessor
{
    public const string CookieName = "shop.guest";

    public string GetOrCreateToken()
    {
        var ctx = accessor.HttpContext ?? throw new InvalidOperationException("بدون HttpContext");
        var token = GetToken();

        if (token is not null) return token;

        token = Guid.NewGuid().ToString("N");

        ctx.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(30)
        });

        return token;
    }

    public string? GetToken()
    {
        var ctx = accessor.HttpContext;
        if (ctx is null) return null;

        return ctx.Request.Cookies.TryGetValue(CookieName, out var token) ? token : null;
    }
}
