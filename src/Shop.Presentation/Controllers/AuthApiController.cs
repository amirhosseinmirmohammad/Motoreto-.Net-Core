using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Interfaces;
using Shop.Domain;
using Shop.Infrastructure.Data;
using Shop.Presentation.Services;
using Shop.Shared;

namespace Shop.Presentation.Controllers;

/// <summary>
/// احراز هویت API: JWT Access (۱۵ دقیقه) + Refresh (۷ روز) + ورود با رمز یا کد پیامکی.
/// </summary>
[Route("api/auth")]
public class AuthApiController(
    UserManager<User> userManager,
    JwtTokenService tokens,
    ILoyaltyService loyalty,
    ISmsService sms,
    ICartService cart,
    IGuestCartAccessor guestCart,
    ApplicationDbContext db) : Controller
{
    public class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class OtpSendRequest
    {
        /// <summary>مثل ۰۹۱۲۳۴۵۶۷۸۹</summary>
        public string Mobile { get; set; } = default!;
    }

    public class OtpVerifyRequest
    {
        public string Mobile { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? ReferralCode { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
    }

    /// <summary>ورود با ایمیل/رمز → جفت توکن.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "ایمیل یا رمز عبور اشتباه است." });

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "ایمیل یا رمز عبور اشتباه است." });

        return Ok(await IssueTokensAsync(user));
    }

    /// <summary>ارسال کد یک‌بارمصرف پیامکی.</summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] OtpSendRequest request, CancellationToken ct)
    {
        var mobile = NormalizeMobile(request.Mobile);
        if (mobile is null)
            return BadRequest(new { message = "شماره موبایل نامعتبر است." });

        // حداکثر یک کد در دقیقه
        var last = await db.OtpCodes
            .Where(o => o.Mobile == mobile)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);

        if (last is not null && (DateTime.UtcNow - last.CreatedAt).TotalSeconds < 60)
            return BadRequest(new { message = "برای درخواست کد جدید کمی صبر کنید." });

        // کد قبلی را باطل کن
        await db.OtpCodes
            .Where(o => o.Mobile == mobile && !o.IsUsed)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true), ct);

        var code = Random.Shared.Next(100000, 999999).ToString();
        db.OtpCodes.Add(new OtpCode
        {
            Mobile = mobile,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        });
        await db.SaveChangesAsync(ct);

        await sms.SendAsync(mobile, $"کد ورود شما به فروشگاه مارکت: {code.ToFarsiDigitsSafe()}");

        return Ok(new { message = "کد پیامکی ارسال شد.", expiresIn = 120 });
    }

    /// <summary>تأیید کد → ورود/ثبت‌نام خودکار + جفت توکن.</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request)
    {
        var mobile = NormalizeMobile(request.Mobile);
        if (mobile is null)
            return BadRequest(new { message = "شماره موبایل نامعتبر است." });

        var otp = await db.OtpCodes
            .Where(o => o.Mobile == mobile && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        if (otp is null || otp.Code != request.Code.Trim())
            return BadRequest(new { message = "کد واردشده صحیح نیست یا منقضی شده است." });

        otp.IsUsed = true;
        await db.SaveChangesAsync();

        // کاربر با این موبایل وجود دارد؟ وگرنه خودکار ثبت‌نام
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
        if (user is null)
        {
            user = new User
            {
                UserName = mobile,
                Email = $"{mobile}@otp.local",
                EmailConfirmed = true,
                FirstName = "کاربر",
                LastName = "مارکت",
                Mobile = mobile,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
                return BadRequest(new { message = "ثبت‌نام ناموفق بود." });

            await userManager.AddToRoleAsync(user, "User");

            // هدیه ثبت‌نام + کد معرف
            await loyalty.ChargeWalletAsync(user.Id, 500_000, WalletTransactionType.RegistrationGift,
                "هدیه خوش‌آمدگویی به مارکت");
            await loyalty.GetOrCreateReferralCodeAsync(user.Id);
        }

        // ادغام سبد مهمان
        var guestToken = guestCart.GetToken();
        if (!string.IsNullOrEmpty(guestToken))
            await cart.MergeGuestCartAsync(guestToken, user.Id);

        var result = await IssueTokensAsync(user);
        return Ok(result);
    }

    /// <summary>چرخش توکن: refresh جدید + access جدید.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var userId = tokens.ValidateRefreshToken(request.RefreshToken);
        if (userId is null)
            return Unauthorized(new { message = "توکن نامعتبر یا منقضی است." });

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "حساب کاربری غیرفعال است." });

        return Ok(await IssueTokensAsync(user));
    }

    /// <summary>اطلاعات کاربر جاری با Bearer token.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = ValidateBearer();
        if (userId is null)
            return Unauthorized(new { message = "توکن نامعتبر است." });

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.Email,
            user.Mobile,
            user.FirstName,
            user.LastName,
            user.WalletBalance,
            user.Points,
            user.ReferralCode,
            Roles = roles
        });
    }

    // ---- private ----

    private async Task<object> IssueTokensAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains("Administrator") ? "Administrator" : roles.Contains("Seller") ? "Seller" : "User";

        var (access, accessExpiry) = tokens.CreateAccessToken(user.Id, user.Mobile ?? string.Empty, role);
        var (refresh, refreshExpiry) = tokens.CreateRefreshToken(user.Id);

        return new
        {
            accessToken = access,
            refreshToken = refresh,
            expiresIn = (int)(accessExpiry - DateTime.UtcNow).TotalSeconds,
            refreshExpiresIn = (int)(refreshExpiry - DateTime.UtcNow).TotalDays * 24 * 3600,
            user = new { user.Id, user.Email, user.Mobile, user.FirstName, user.LastName, role }
        };
    }

    private Guid? ValidateBearer()
    {
        var header = Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return tokens.ValidateAccessToken(header["Bearer ".Length..].Trim());
    }

    private static string? NormalizeMobile(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var digits = input.ToLatinDigits().Replace(" ", "").Replace("-", "");
        digits = digits.StartsWith("+98") ? "0" + digits[3..] : digits;
        digits = digits.StartsWith("98") && digits.Length == 12 ? "0" + digits[2..] : digits;
        return System.Text.RegularExpressions.Regex.IsMatch(digits, @"^09\d{9}$") ? digits : null;
    }
}

/// <summary>امتداد کوچک برای فارسی‌سازی ارقام داخل سرویس‌ها.</summary>
public static class MobileStringExtensions
{
    public static string ToFarsiDigitsSafe(this string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input
            .Replace('0', '۰').Replace('1', '۱').Replace('2', '۲').Replace('3', '۳').Replace('4', '۴')
            .Replace('5', '۵').Replace('6', '۶').Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
    }
}
