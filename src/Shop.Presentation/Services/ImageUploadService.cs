using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Shop.Presentation.Services;

/// <summary>
/// آپلود تصویر: تبدیل خودکار به WebP (کیفیت ۸۰)، تغییر اندازه (حداکثر ۱۲۰۰ پیکسل)
/// و ذخیره روی Object Storage (SeaweedFS/S3). خروجی URL عمومی تصویر است.
/// </summary>
public class ImageUploadService(ObjectStorageService storage)
{
    private const int MaxSide = 1200;
    private const int Quality = 80;
    private const long MaxBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/avif"
    ];

    /// <summary>فایل مرورگر (Blazor) → WebP → Object Storage. خروجی: URL عمومی.</summary>
    public async Task<string> SaveAsync(IBrowserFile file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Size > MaxBytes)
            throw new InvalidOperationException("حجم فایل بیش از ۱۰ مگابایت است.");

        if (!AllowedTypes.Contains(file.ContentType))
            throw new InvalidOperationException("فقط تصویر JPG / PNG / WebP / GIF مجاز است.");

        await using var stream = file.OpenReadStream(MaxBytes, ct);
        return await SaveWebpAsync(stream, ct);
    }

    /// <summary>استریم آماده (با progress در UploadDropzone خوانده شده) → WebP → Object Storage.</summary>
    public async Task<string> SaveWebpAsync(Stream input, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(input, ct);

        // تغییر اندازه نسبی — نسبت ابعاد حفظ می‌شود
        if (image.Width > MaxSide || image.Height > MaxSide)
        {
            var ratio = Math.Min((double)MaxSide / image.Width, (double)MaxSide / image.Height);
            image.Mutate(x => x.Resize((int)Math.Round(image.Width * ratio), (int)Math.Round(image.Height * ratio)));
        }

        // تبدیل به WebP در حافظه
        var ms = new MemoryStream();
        await image.SaveAsync(ms, new WebpEncoder { Quality = Quality }, ct);
        ms.Position = 0;

        var objectName = $"uploads/{DateTime.UtcNow:yyyy/MM}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}.webp";
        return await storage.UploadAsync(ms, ms.Length, objectName, "image/webp", ct);
    }
}
