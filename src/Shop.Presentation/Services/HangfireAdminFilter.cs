using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;

namespace Shop.Presentation.Services;

/// <summary>داشبورد Hangfire فقط برای ادمین‌ها.</summary>
public class HangfireAdminFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        if (http.User.Identity?.IsAuthenticated != true) return false;
        if (http.User.IsInRole("Administrator")) return true;

        var userManager = http.RequestServices.GetRequiredService<UserManager<Shop.Domain.User>>();
        var user = userManager.GetUserAsync(http.User).GetAwaiter().GetResult();
        return user is not null && userManager.IsInRoleAsync(user, "Administrator").GetAwaiter().GetResult();
    }
}
