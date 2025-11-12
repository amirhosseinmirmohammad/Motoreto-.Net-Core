using Microsoft.AspNetCore.StaticFiles;
using MudBlazor;
using MudBlazor.Services;
using PWA.Components;
using WebEssentials.AspNetCore.Pwa;

var builder = WebApplication.CreateBuilder(args);

// ---------------- HttpClient ----------------
var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
              ?? throw new InvalidOperationException("Api BaseUrl is missing");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseUrl) });

// ---------------- Razor/Blazor ----------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ---------------- MudBlazor ----------------
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;        // ❌ بدون دکمه بستن
    config.SnackbarConfiguration.RequireInteraction = true;    // ✅ خودش محو میشه
    config.SnackbarConfiguration.VisibleStateDuration = 5000;   // ⏱ مدت نمایش (۵ ثانیه)
    config.SnackbarConfiguration.HideTransitionDuration = 400;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    config.SnackbarConfiguration.ClearAfterNavigation = true;
});


MudGlobal.UnhandledExceptionHandler = (exception) => Console.WriteLine(exception);

// ---------------- PWA ----------------
builder.Services.AddProgressiveWebApp(new PwaOptions
{
    AllowHttp = true,
    RegisterServiceWorker = true,
    RegisterWebmanifest = true,
    //Strategy = ServiceWorkerStrategy.CacheFirst,
    CacheId = "motoreto-v3",
    EnableCspNonce = true,
    OfflineRoute = "/offline.html"
});

var app = builder.Build();

// ---------------- Pipeline ----------------
if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// ---------------- Static Files ----------------
var ctp = new FileExtensionContentTypeProvider();

// ✅ اضافه‌شده برای فونت‌ها (در بعضی سیستم‌ها بدون این مپ‌ها MIME اشتباه می‌خورد)
ctp.Mappings[".woff"] = "font/woff";
ctp.Mappings[".woff2"] = "font/woff2";
ctp.Mappings[".ttf"] = "font/ttf";
ctp.Mappings[".otf"] = "font/otf";
ctp.Mappings[".eot"] = "application/vnd.ms-fontobject";
ctp.Mappings[".svg"] = "image/svg+xml";
ctp.Mappings[".wasm"] = "application/wasm";
ctp.Mappings[".webmanifest"] = "application/manifest+json";

// ✅ مسیرهای StaticFile
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = ctp,
    //OnPrepareResponse = ctx =>
    //{
    //    const int days = 30;
    //    ctx.Context.Response.Headers["Cache-Control"] = $"public, max-age={days * 86400}";
    //    ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    //}
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AddInteractiveWebAssemblyRenderMode();

app.Run();
