using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Shop.Shared;

public class JwtTokenOptions
{
    public string Issuer { get; set; } = "shop";
    public string Audience { get; set; } = "shop-api";
    /// <summary>کلید حداقل ۳۲ کاراکتر — از appsettings/تنظیمات.</summary>
    public string Key { get; set; } = default!;
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
}

/// <summary>
/// ساخت/اعتبارسنجی JWT (HS256) + Refresh Token امضاشده — بدون پکیج خارجی.
/// </summary>
public class JwtTokenService(JwtTokenOptions options)
{
    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    /// <summary>Access Token: ۱۵ دقیقه (قابل تنظیم) با claims استاندارد.</summary>
    public (string Token, DateTime ExpiresAt) CreateAccessToken(Guid userId, string mobile, string role)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(options.AccessMinutes);

        var payload = new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["mobile"] = mobile ?? string.Empty,
            ["role"] = role,
            ["iss"] = options.Issuer,
            ["aud"] = options.Audience,
            ["iat"] = ((DateTimeOffset)now).ToUnixTimeSeconds(),
            ["exp"] = ((DateTimeOffset)expires).ToUnixTimeSeconds(),
            ["jti"] = Guid.NewGuid().ToString("N")
        };

        var header = Base64Url(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var body = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signature = Base64Url(HMAC(Encoding.UTF8.GetBytes($"{header}.{body}")));

        return ($"{header}.{body}.{signature}", expires);
    }

    /// <summary>
    /// Refresh Token: امضاشده و stateless (userId + expiry) — چرخش در هر refresh.
    /// </summary>
    public (string Token, DateTime ExpiresAt) CreateRefreshToken(Guid userId)
    {
        var expires = DateTime.UtcNow.AddDays(options.RefreshDays);
        var payload = $"{userId:N}|{((DateTimeOffset)expires).ToUnixTimeSeconds()}";
        var signature = Base64Url(HMAC(Encoding.UTF8.GetBytes($"refresh|{payload}")));
        return ($"{Base64Url(Encoding.UTF8.GetBytes(payload))}.{signature}", expires);
    }

    /// <summary>اعتبارسنجی refresh و استخراج userId (null = نامعتبر/منقضی).</summary>
    public Guid? ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var parts = refreshToken.Split('.');
            if (parts.Length != 2) return null;

            var payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            var expected = Base64Url(HMAC(Encoding.UTF8.GetBytes($"refresh|{payload}")));
            if (parts[1] != expected) return null;

            var segments = payload.Split('|');
            if (segments.Length != 2) return null;

            var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(segments[1])).UtcDateTime;
            if (expiry < DateTime.UtcNow) return null;

            return Guid.TryParseExact(segments[0], "N", out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>اعتبارسنجی access token و استخراج userId.</summary>
    public Guid? ValidateAccessToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var expectedSig = Base64Url(HMAC(Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}")));
            if (parts[2] != expectedSig) return null;

            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64UrlDecode(parts[1]));
            if (payload is null) return null;

            var exp = payload["exp"].GetInt64();
            if (DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime < DateTime.UtcNow) return null;

            if (payload["iss"].GetString() != options.Issuer || payload["aud"].GetString() != options.Audience)
                return null;

            return Guid.TryParse(payload["sub"].GetString(), out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }

    private byte[] HMAC(byte[] data) =>
        new HMACSHA256(Encoding.UTF8.GetBytes(options.Key)).ComputeHash(data);
}
