using Microsoft.Extensions.Logging;

namespace Shop.Infrastructure.Gateways;

/// <summary>
/// پایه آداپتورهای اعتباری/کیفی (دیجی‌پی، اسنپ‌پی، بله).
/// این سرویس‌ها API عمومی بدون قرارداد ندارند؛ پس از دریافت کلید از پنل هر سرویس،
/// متدهای Create/Verify را با مستنداتشان کامل کنید.
/// </summary>
public abstract class ConfigurableGatewayBase : Shop.Application.DTOs.IPaymentGateway
{
    public abstract string GatewayName { get; }

    // پیاده‌سازی صریح عضو اینترفیس
    string Shop.Application.DTOs.IPaymentGateway.Name => GatewayName;

    public virtual Task<Application.DTOs.PaymentResult> CreatePaymentAsync(Application.DTOs.PaymentRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(new Application.DTOs.PaymentResult
        {
            Success = false,
            Message = $"درگاه {GatewayName} هنوز متصل نشده است. پس از دریافت مجوز سرویس، کلیدهای آن را در تنظیمات وارد کنید."
        });
    }

    public virtual Task<Application.DTOs.PaymentVerificationResult> VerifyAsync(string authority, bool customerApproved, CancellationToken ct = default)
        => Task.FromResult(new Application.DTOs.PaymentVerificationResult
        {
            Success = false,
            Message = $"درگاه {GatewayName} پیکربندی نشده است."
        });
}

public class DigipayGateway(ILogger<DigipayGateway> logger) : ConfigurableGatewayBase
{
    public override string GatewayName => "Digipay";
}

public class SnappPayGateway(ILogger<SnappPayGateway> logger) : ConfigurableGatewayBase
{
    public override string GatewayName => "SnappPay";
}

public class BaleGateway(ILogger<BaleGateway> logger) : ConfigurableGatewayBase
{
    public override string GatewayName => "Bale";
}
