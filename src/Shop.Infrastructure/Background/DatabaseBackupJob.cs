using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Shop.Infrastructure.Background;

/// <summary>
/// بکاپ روزانه PostgreSQL با pg_dump (custom format).
/// تنظیمات: Backup:PgDumpPath (پیش‌فرض pg_dump در PATH) و Backup:OutputDir.
/// اگر pg_dump در دسترس نباشد فقط لاگ می‌شود — خطا درخواست‌ها را نمی‌شکند.
/// </summary>
public class DatabaseBackupJob(IConfiguration config, ILogger<DatabaseBackupJob> logger)
{
    public async Task RunAsync()
    {
        var conn = config.GetConnectionString("DefaultConnection");
        var pgDump = config.GetValue<string>("Backup:PgDumpPath", "pg_dump");
        var outDir = config.GetValue<string>("Backup:OutputDir", "backups");
        var keep = config.GetValue<int>("Backup:Keep", 7);

        if (string.IsNullOrWhiteSpace(conn))
        {
            logger.LogWarning("Backup: ConnectionString خالی است — بکاپ رد شد");
            return;
        }

        Directory.CreateDirectory(outDir);
        var file = Path.Combine(outDir, $"shop-{DateTime.UtcNow:yyyyMMdd-HHmm}.dump");

        var psi = new ProcessStartInfo(pgDump)
        {
            Arguments = $"--no-owner --format=custom --file \"{file}\" \"{conn}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };

        using var p = Process.Start(psi);
        if (p is null)
        {
            logger.LogError("Backup: اجرای pg_dump شروع نشد");
            return;
        }

        var err = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();

        if (p.ExitCode != 0)
        {
            logger.LogError("Backup: pg_dump ناموفق (کد {Code}): {Err}", p.ExitCode, err);
            return;
        }

        // نگه‌داشت آخرین نسخه‌ها
        var dumps = new DirectoryInfo(outDir).GetFiles("shop-*.dump")
            .OrderByDescending(f => f.CreationTimeUtc).ToList();
        foreach (var old in dumps.Skip(keep))
            old.Delete();

        logger.LogInformation("Backup دیتابیس ساخته شد: {File} ({Size} KB) — نگه‌داشت {Keep} نسخه",
            file, new FileInfo(file).Length / 1024, keep);
    }
}
