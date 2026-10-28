using Microsoft.AspNetCore.Http;

namespace Shop.Presentation.Services;

/// <summary>
/// آپلود امن فایل (مدارک فروشنده، تصاویر) — اعتبارسنجی پسوند/حجم، نام‌گذاری یکتا.
/// </summary>
public interface IFileUploadService
{
    /// <summary>ذخیره فایل و برگرداندن مسیر نسبی؛ null = فایل نامعتبر یا خالی.</summary>
    Task<string?> SaveAsync(IFormFile? file, string folder, CancellationToken ct = default);
}

public class FileUploadService(IWebHostEnvironment env) : IFileUploadService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];
    private const long MaxSizeBytes = 5 * 1024 * 1024; // ۵ مگابایت

    public async Task<string?> SaveAsync(IFormFile? file, string folder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("فرمت فایل مجاز نیست (فقط jpg، png، webp، pdf).");

        if (file.Length > MaxSizeBytes)
            throw new InvalidOperationException("حجم فایل نباید بیشتر از ۵ مگابایت باشد.");

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", folder, DateTime.UtcNow.ToString("yyyyMM"));
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return $"/uploads/{folder}/{DateTime.UtcNow:yyyyMM}/{fileName}";
    }
}
