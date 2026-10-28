using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shop.Presentation.Areas.Admin.Controllers;

/// <summary>همه کنترلرهای ادمین فقط برای Administrator.</summary>
[Area("Admin")]
[Authorize(Roles = "Administrator")]
public abstract class AdminBaseController : Controller
{
}
