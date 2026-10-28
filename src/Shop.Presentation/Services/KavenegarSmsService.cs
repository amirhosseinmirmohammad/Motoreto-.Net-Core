using Microsoft.Extensions.Configuration;

namespace Shop.Presentation.Services;

/// <summary>ارسال SMS واقعی از طریق کاوه‌نگار.</summary>
public class KavenegarSmsService : ISmsService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _sender;

    public KavenegarSmsService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Sms:ApiKey"] ?? "";
        _sender = config["Sms:Sender"] ?? "";
    }

    public async Task SendAsync(string mobile, string message, CancellationToken ct = default)
    {
        var url = $"https://api.kavenegar.com/v1/{_apiKey}/sms/send.json?receptor={mobile}&sender={_sender}&message={Uri.EscapeDataString(message)}";
        try { await _http.GetAsync(url, ct); } catch { }
    }
}