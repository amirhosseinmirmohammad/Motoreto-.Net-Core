using System.Net.Http.Json;
using System.Text.Json;
using Shop.Application.DTOs;

using Microsoft.Extensions.Logging;

namespace Shop.Infrastructure.Gateways;

/// <summary>
/// درگاه زرین‌پال — پیاده‌سازی کامل با API رسمی (v4).
/// کلید مرچنت از پنل ادمین → تنظیمات (PaymentMerchantId) خوانده می‌شود.
/// حالت Sandbox با PaymentSandbox=true.
/// </summary>
public class ZarinpalGateway(HttpClient http, ILogger<ZarinpalGateway> logger) : IPaymentGateway
{
    public string Name => "Zarinpal";

    public async Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        var merchantId = GetSetting("PaymentMerchantId");
        var sandbox = GetSetting("PaymentSandbox") == "true";

        if (string.IsNullOrWhiteSpace(merchantId))
            return Fail("مرچنت‌آی‌دی زرین‌پال در تنظیمات ثبت نشده است. از پنل ادمین → تنظیمات وارد کنید.");

        var authority = Guid.NewGuid().ToString("N");
        var baseUrl = sandbox ? "https://sandbox.zarinpal.com" : "https://payment.zarinpal.com";

        try
        {
            var payload = new
            {
                merchant_id = merchantId,
                amount = request.Amount / 10, // زرین‌پال تومان کار می‌کند
                callback_url = request.CallbackUrl,
                description = request.Description
            };

            var response = await http.PostAsJsonAsync($"{baseUrl}/pg/v4/payment/request.json", payload, ct);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            var data = json.GetProperty("data");

            if (data.TryGetProperty("authority", out var authorityEl))
            {
                var gatewayAuthority = authorityEl.GetString() ?? authority;
                return new PaymentResult
                {
                    Success = true,
                    Message = "در حال انتقال به درگاه زرین‌پال...",
                    Authority = gatewayAuthority,
                    RedirectUrl = $"{baseUrl}/pg/StartPay/{gatewayAuthority}"
                };
            }

            var errors = json.TryGetProperty("errors", out var errEl) ? errEl.ToString() : "خطای نامشخص";
            logger.LogWarning("Zarinpal request failed: {Errors}", errors);
            return Fail("ارتباط با زرین‌پال برقرار نشد: " + errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Zarinpal CreatePayment error");
            return Fail("خطا در ارتباط با درگاه زرین‌پال.");
        }
    }

    public async Task<PaymentVerificationResult> VerifyAsync(string authority, bool customerApproved, CancellationToken ct = default)
    {
        if (!customerApproved)
            return new PaymentVerificationResult { Success = false, Message = "پرداخت توسط کاربر لغو شد." };

        var merchantId = GetSetting("PaymentMerchantId");
        var sandbox = GetSetting("PaymentSandbox") == "true";
        var baseUrl = sandbox ? "https://sandbox.zarinpal.com" : "https://payment.zarinpal.com";

        try
        {
            var payload = new { merchant_id = merchantId, authority };
            var response = await http.PostAsJsonAsync($"{baseUrl}/pg/v4/payment/verify.json", payload, ct);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            var data = json.GetProperty("data");
            var code = data.TryGetProperty("code", out var codeEl) ? codeEl.GetInt32() : 0;

            if (code is 100 or 101)
            {
                var refId = data.TryGetProperty("ref_id", out var refEl) ? refEl.GetInt64().ToString() : authority;
                return new PaymentVerificationResult { Success = true, Message = "پرداخت موفق.", RefId = refId };
            }

            return new PaymentVerificationResult { Success = false, Message = "تأیید پرداخت ناموفق بود. کد: " + code };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Zarinpal Verify error");
            return new PaymentVerificationResult { Success = false, Message = "خطا در تأیید پرداخت زرین‌پال." };
        }
    }

    private static string? GetSetting(string key) =>
        PaymentGatewaySettings.Current.TryGetValue(key, out var value) ? value : null;

    private static PaymentResult Fail(string message) => new() { Success = false, Message = message };
}

/// <summary>تنظیمات درگاه‌ها (از پنل ادمین پر می‌شود).</summary>
public static class PaymentGatewaySettings
{
    public static Dictionary<string, string> Current { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>درگاه پرداخت شبیه‌سازی‌شده برای توسعه و تست (بدون پول واقعی).</summary>
public class FakePaymentGateway : IPaymentGateway
{
    public string Name => "Fake";

    public Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        var authority = Guid.NewGuid().ToString("N");
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            Message = "در حال انتقال به درگاه پرداخت...",
            Authority = authority,
            RedirectUrl = $"/payment/gateway/{authority}?amount={request.Amount}&order={request.OrderId}"
        });
    }

    public Task<PaymentVerificationResult> VerifyAsync(string authority, bool customerApproved, CancellationToken ct = default)
        => Task.FromResult(customerApproved
            ? new PaymentVerificationResult
            {
                Success = true,
                Message = "پرداخت موفق.",
                RefId = $"FAKE-{DateTime.UtcNow:HHmmss}-{Random.Shared.Next(1000, 9999)}"
            }
            : new PaymentVerificationResult { Success = false, Message = "پرداخت توسط کاربر لغو شد." });
}

/// <summary>Factory + Keyed DI: انتخاب درگاه فعال از تنظیمات (OCP — افزودن درگاه = یک کلاس + یک خط).</summary>
public class PaymentGatewayFactory(IServiceProvider provider, ISettingsService settings) : IPaymentGatewayFactory
{
    public async Task<IPaymentGateway> GetActiveAsync(CancellationToken ct = default)
    {
        var config = await settings.GetAsync(ct);
        return provider.GetRequiredKeyedService<IPaymentGateway>(config.PaymentGateway);
    }
}

public interface IPaymentGatewayFactory
{
    Task<IPaymentGateway> GetActiveAsync(CancellationToken ct = default);
}