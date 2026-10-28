using System.Globalization;
using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shop.Domain;
using Shop.Infrastructure;
using Shop.Infrastructure.Data;
using Shop.Infrastructure.Seeding;
using Shop.Presentation.Services;

// ---------- Serilog Bootstrap ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- Serilog کامل (از appsettings) ----------
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // ---------- CultureInfo: فارسی ----------
    var fa = CultureInfo.GetCultureInfo("fa-IR");
    CultureInfo.DefaultThreadCurrentCulture = fa;
    CultureInfo.DefaultThreadCurrentUICulture = fa;

    // ---------- Redis ----------
    var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:56379";
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "shop:";
    });

    // ---------- Session (Redis-backed) — کپچا و وضعیت‌های موقت؛ آماده چند-سروری ----------
    builder.Services.AddSession(options =>
    {
        options.Cookie.Name = "shop.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.IdleTimeout = TimeSpan.FromMinutes(60);
    });

    // ---------- لایه‌ها (Composition Root) ----------
    // ---------- JWT (برای API موبایل/سرویس‌ها) ----------
    builder.Services.AddSingleton(new Shop.Shared.JwtTokenService(new Shop.Shared.JwtTokenOptions
    {
        Issuer = builder.Configuration["Jwt:Issuer"] ?? "shop",
        Audience = builder.Configuration["Jwt:Audience"] ?? "shop-api",
        Key = builder.Configuration["Jwt:Key"] ?? "shop-dev-secret-key-change-in-production-32ch!",
        AccessMinutes = builder.Configuration.GetValue<int>("Jwt:AccessMinutes", 15),
        RefreshDays = builder.Configuration.GetValue<int>("Jwt:RefreshDays", 7)
    }));

    // ---------- CORS (داینامیک از تنظیمات دیتابیس) ----------
    builder.Services.AddCors();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<Shop.Presentation.Services.IFileUploadService, Shop.Presentation.Services.FileUploadService>();
    builder.Services.AddSingleton<Shop.Presentation.Services.ObjectStorageService>();
    builder.Services.AddScoped<Shop.Presentation.Services.ImageUploadService>();
    builder.Services.AddHttpClient<Shop.Presentation.Services.FeedImportService>();
    builder.Services.AddHostedService<Shop.Presentation.Services.FeedSyncJob>();
    builder.Services.AddScoped<Shop.Application.Interfaces.ISmsService, Shop.Presentation.Services.KavenegarSmsService>();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<Shop.Application.Interfaces.INotificationBroadcaster, Shop.Presentation.Services.SignalRBroadcaster>();

    // ---------- Identity ----------
    builder.Services
        .AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "shop.auth";
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = ctx =>
        {
            // برای درخواست‌های AJAX وضعیت 401 برگردان (به‌جای ریدایرکت)
            if (ctx.Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

    // برای [Authorize] و AuthenticationState در کامپوننت‌های Blazor
    builder.Services.AddCascadingAuthenticationState();

    // ---------- MVC ----------
    builder.Services.AddControllersWithViews(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

    // ---------- Cache (HybridCache: L1 حافظه + L2 Redis — ثبت Redis بالای فایل) ----------
    builder.Services.AddMemoryCache();

    // ---------- Blazor ----------
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

    builder.Services.AddOutputCache(options =>
    {
        // صفحه اصلی و لیست محصولات: کش کوتاه — هزینه دیتابیس تقریباً صفر
        options.AddPolicy("HomeCache", policy => policy.Expire(TimeSpan.FromSeconds(30)).SetVaryByRouteValue("controller", "action"));
        options.AddPolicy("CatalogCache", policy => policy.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
        options.AddPolicy("BlogCache", policy => policy.Expire(TimeSpan.FromSeconds(60)));
        options.AddPolicy("SeoCache", policy => policy.Expire(TimeSpan.FromHours(1)));
    });

    builder.Services.AddHealthChecks()
        .AddCheck<DbHealthCheck>("database", tags: ["ready"]);



    builder.Services.AddScoped<IGuestCartAccessor, GuestCartAccessor>();

var app = builder.Build();

// ---- Security Headers ----
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

// ---- Captcha ساده (ریاضی) برای فرم‌های عمومی ----
app.MapGet("/api/captcha", (HttpContext ctx) =>
{
    var r = Random.Shared;
    int a = r.Next(2, 9), b = r.Next(2, 9);
    var answer = a + b;
    ctx.Session.SetString("captcha", answer.ToString());
    return Results.Json(new { question = $"{a} + {b} = ?" });
});

app.MapPost("/api/captcha/verify", (HttpContext ctx, [FromBody] JsonElement body) =>
{
    var expected = ctx.Session.GetString("captcha");
    var provided = body.TryGetProperty("answer", out var a) ? a.GetString() : null;
    var ok = expected is not null && provided == expected;
    return Results.Json(new { valid = ok });
});

// اتصال به Object Storage (SeaweedFS/S3) — ساخت باکت در صورت نبود
try
{
    using (var bootScope = app.Services.CreateScope())
    {
        var storageBoot = bootScope.ServiceProvider.GetRequiredService<Shop.Presentation.Services.ObjectStorageService>();
        await storageBoot.EnsureBucketAsync();
        Log.Information("Object storage ready (bucket: {Bucket})", app.Configuration["ObjectStorage:Bucket"] ?? "shop");
    }
}
catch (Exception storageEx)
{
    Log.Warning(storageEx, "Object storage unavailable — uploads will fail until it is reachable");
}

// بنرهای اسلایدر — در صورت نبود تولید می‌شوند
try
{
    Shop.Presentation.Services.BannerSeeder.Ensure(app.Environment);
    Log.Information("Slider banners ready");
}
catch (Exception bannerEx)
{
    Log.Warning(bannerEx, "Banner generation failed");
}

// افیلیت: ثبت کلیک، ست کوکی ۳۰ روزه و انتقال به خانه
app.MapGet("/aff/{code}", async (string code, ApplicationDbContext db, HttpContext ctx) =>
{
    var aff = await db.Affiliates.FirstOrDefaultAsync(a => a.Code == code && a.IsActive);
    if (aff is not null)
    {
        aff.ClickCount++;
        await db.SaveChangesAsync();
        ctx.Response.Cookies.Append("aff", code, new CookieOptions { MaxAge = TimeSpan.FromDays(30), SameSite = SameSiteMode.Lax });
    }
    return Results.Redirect("/?ref=" + code);
});

// بنرهای اسلایدر — در صورت نبود تولید می‌شوند

// افیلیت مدل CPL: لندینگ ثبت سرنخ برای همکار
app.MapGet("/aff/lead/{code}", async (string code, ApplicationDbContext db) =>
{
    var aff = await db.Affiliates.AsNoTracking().FirstOrDefaultAsync(a => a.Code == code && a.IsActive);
    return aff is null ? Results.NotFound() : Results.Content(
        $@"<!DOCTYPE html><html lang='fa' dir='rtl'><head><meta charset='utf-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'><title>پیشنهاد ویژه مارکت</title>
        <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css'></head>
        <body class='bg-light'><div class='container' style='max-width:480px;margin-top:8vh'>
        <div class='card border-0 shadow-sm rounded-4 p-4'>
        <h4 class='fw-bold text-center mb-1'>🎁 پیشنهاد ویژه مارکت</h4>
        <p class='text-center text-muted small'>اطلاعات‌تان را ثبت کنید تا مشاور رایگان خرید و کد تخفیف اولین خرید برایتان ارسال شود.</p>
        <form method='post' action='/aff/lead/{code}'>
            <input class='form-control mb-2' name='FullName' placeholder='نام و نام خانوادگی' required>
            <input class='form-control mb-3' name='Phone' placeholder='شماره موبایل' dir='ltr' required>
            <button class='btn btn-primary w-100 py-2'>ثبت و دریافت مشاوره رایگان</button>
        </form></div></div></body></html>", "text/html");
});

app.MapPost("/aff/lead/{code}", async (string code, IFormCollection form, ApplicationDbContext db) =>
{
    var aff = await db.Affiliates.FirstOrDefaultAsync(a => a.Code == code && a.IsActive);
    if (aff is null) return Results.NotFound();

    var fullName = form["FullName"].ToString().Trim();
    var phone = form["Phone"].ToString().Trim();
    if (fullName.Length < 2 || phone.Length < 10)
        return Results.BadRequest("اطلاعات نامعتبر است.");

    db.AffiliateLeads.Add(new AffiliateLead
    {
        AffiliateId = aff.Id,
        FullName = fullName,
        Phone = phone,
        CreatedAt = DateTime.UtcNow
    });

    // پورسانت CPL
    const int cplBonus = 15000;
    aff.LeadCount++;
    aff.Earned += cplBonus;
    await db.SaveChangesAsync();

    return Results.Content(
        "<!DOCTYPE html><html lang='fa' dir='rtl'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'><title>ثبت شد</title><link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.rtl.min.css'></head><body class='bg-light'><div class='container' style='max-width:480px;margin-top:10vh'><div class='card border-0 shadow-sm rounded-4 p-4 text-center'><h4 class='text-success fw-bold'>✓ ثبت شد!</h4><p class='text-muted'>کارشناسان ما به‌زودی با شما تماس می‌گیرند.</p></div></div></body></html>", "text/html");
});

    // ---------- Migration + Seed (با Retry) ----------
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                Log.Information("Migrate: تلاش {Attempt}", attempt);
                await db.Database.MigrateAsync();
                break;
            }
            catch (Exception ex) when (attempt < 5)
            {
                Log.Warning(ex, "Migrate ناموفق؛ تلاش مجدد بعد از ۲ ثانیه");
                await Task.Delay(2000);
            }
        }

        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        Log.Information("Seed: تکمیل شد");
    }

    // ---------- Pipeline ----------
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Status/500");
        app.UseHsts();
    }
    else
    {
        app.UseExceptionHandler("/Home/Status/500");
    }

    // ۴۰۴/۵۰۰ و بقیه کدها → صفحه قشنگ (بدون redirect)
    app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseResponseCompression();

    // Static files با کش بلند‌مدت برای asset های نسخه‌دار
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            var path = ctx.Context.Request.Path;
            if (path.StartsWithSegments("/lib") || path.StartsWithSegments("/fonts") ||
                path.StartsWithSegments("/images") || path.StartsWithSegments("/css") ||
                path.StartsWithSegments("/js"))
            {
                ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
            }
        }
    });

    // فایل‌های بدون پسوند PWA
    app.UseWhen(
        ctx => ctx.Request.Path == "/service-worker.js" || ctx.Request.Path == "/manifest.webmanifest",
        branch => branch.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.CacheControl = "no-cache";
            await next();
        }));

    app.UseRouting();

    // Session (Redis) — قبل از هر اندپوینتی که استفاده می‌کند (کپچا)
    app.UseSession();

    // ---------- Hangfire: داشبورد (فقط ادمین) + jobهای تکرارشونده ----------
    app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
    {
        Authorization = [new Shop.Presentation.Services.HangfireAdminFilter()]
    });
    using (var hfScope = app.Services.CreateScope())
    {
        Hangfire.RecurringJob.AddOrUpdate<Shop.Infrastructure.Background.ClassifiedJobs>(
            "classified-expire-cleanup",
            j => j.ExpireAndCleanupAsync(),
            Hangfire.Cron.HourInterval(1));

        // بکاپ روزانه دیتابیس — ۰۳:۳۰ به وقت سرور
        Hangfire.RecurringJob.AddOrUpdate<Shop.Infrastructure.Background.DatabaseBackupJob>(
            "database-backup",
            j => j.RunAsync(),
            "30 3 * * *");
    }

    // ---------- CORS داینامیک (از SiteSettings) ----------
    app.Use(async (context, next) =>
    {
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin))
        {
            var settingsService = context.RequestServices.GetRequiredService<Shop.Application.Interfaces.ISettingsService>();
            var config = await settingsService.GetAsync(context.RequestAborted);

            var allowed = new List<string>();
            if (!string.IsNullOrEmpty(config.FrontendUrl))
                allowed.Add(config.FrontendUrl.TrimEnd('/'));
            if (!string.IsNullOrEmpty(config.AdditionalCorsOrigins))
                allowed.AddRange(config.AdditionalCorsOrigins
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(o => o.TrimEnd('/')));

            if (allowed.Contains(origin.TrimEnd('/'), StringComparer.OrdinalIgnoreCase))
            {
                context.Response.Headers.AccessControlAllowOrigin = origin;
                context.Response.Headers.AccessControlAllowCredentials = "true";
                context.Response.Headers.AccessControlAllowHeaders = "Content-Type, Authorization, RequestVerificationToken, X-Requested-With";
                context.Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE, OPTIONS";
                context.Response.Headers.Vary = "Origin";
            }
        }

        // Preflight
        if (context.Request.Method == HttpMethods.Options)
        {
            context.Response.StatusCode = 204;
            return;
        }

        await next();
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseOutputCache();

    // Blazor (فرم‌های SSR) به AntiForgery Middleware نیاز دارد — بعد از UseAuthentication/UseAuthorization
    app.UseAntiforgery();

    // ---------- Routes ----------
    var supportedCultures = new[] { fa };
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new("fa-IR"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures
    });

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    // مسیرهای خوانا و سئو-پسند
    app.MapControllerRoute(
        name: "product",
        pattern: "p/{slug}",
        defaults: new { controller = "Product", action = "Details" });

    app.MapControllerRoute(
        name: "blogPost",
        pattern: "blog/{slug}",
        defaults: new { controller = "Blog", action = "Details" });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true
    });

    // ---------- SignalR: اعلان و چت زنده ----------
    app.MapHub<Shop.Presentation.Hubs.AppHub>("/hubs/app");

    app.MapRazorComponents<Shop.Presentation.Components.App>()
        .AddInteractiveServerRenderMode();

    app.UseSerilogRequestLogging();

    Log.Information("فروشگاه Shop در حال اجرا...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "اجرای برنامه با خطا متوقف شد");
}
finally
{
    await Log.CloseAndFlushAsync();
}
