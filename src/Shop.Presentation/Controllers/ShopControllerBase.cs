using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Controllers;

/// <summary>پایه کنترلرهای فروشگاه — ابزارهای مشترک (Authorize اختیاری توسط فرزند).</summary>
public abstract class ShopControllerBase : Controller
{
    protected Guid UserId =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException();
}
