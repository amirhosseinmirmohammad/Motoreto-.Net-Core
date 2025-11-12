using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ----- Serilog (از appsettings) -----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// config
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// conn
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// db
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(conn, npg => npg.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null)));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// identity
builder.Services
    .AddIdentity<User, IdentityRole<Guid>>(o =>
    {
        o.Password.RequireDigit = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequireUppercase = false;
        o.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ----- HTTP Logging (built-in) برای متادیتاهای درخواست -----
app.Use(async (ctx, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await next();
        sw.Stop();
        Log
        .ForContext("Path", ctx.Request.Path.Value)
        .ForContext("Method", ctx.Request.Method)
        .ForContext("StatusCode", ctx.Response?.StatusCode)
        .ForContext("User", ctx.User?.Identity?.Name ?? "anonymous")
        .ForContext("Ip", ctx.Connection.RemoteIpAddress?.ToString())
        .Information("HTTP {Method} {Path} responded {StatusCode} in {Elapsed} ms",
                     ctx.Request.Method, ctx.Request.Path, ctx.Response?.StatusCode, sw.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        sw.Stop();
        Log
        .ForContext("Path", ctx.Request.Path.Value)
        .ForContext("Method", ctx.Request.Method)
        .ForContext("User", ctx.User?.Identity?.Name ?? "anonymous")
        .ForContext("Ip", ctx.Connection.RemoteIpAddress?.ToString())
        .ForContext("ElapsedMs", sw.ElapsedMilliseconds)
        .Error(ex, "HTTP {Method} {Path} threw an exception after {Elapsed} ms",
               ctx.Request.Method, ctx.Request.Path, sw.ElapsedMilliseconds);
        throw;
    }
});

// migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    for (int i = 0; i < 5; i++)
    {
        try
        {
            Log.Information("DB:Migrate attempt {Attempt}", i + 1);
            await db.Database.MigrateAsync();
            Log.Information("DB:Migrate success");
            break;
        }
        catch (Exception ex) when (i < 4)
        {
            Log.Warning(ex, "DB:Migrate failed, will retry");
            await Task.Delay(1500);
        }
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "Administrator", "User", "Operator" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var res = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            if (res.Succeeded)
                Log.Information("Seed:Role created {Role}", role);
            else
                Log.Warning("Seed:Role create failed {Role} {@Errors}", role, res.Errors);
        }
        else
        {
            Log.Debug("Seed:Role exists {Role}", role);
        }
    }
}

// swagger (اگر در Production هم می‌خواهی، شرط را بردار)
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; // Swagger در /swagger
    });
}

// static files
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webp"] = "image/webp";
provider.Mappings[".webmanifest"] = "application/manifest+json";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".woff"] = "font/woff";
provider.Mappings[".woff2"] = "font/woff2";
provider.Mappings[".ttf"] = "font/ttf";
provider.Mappings[".otf"] = "font/otf";
provider.Mappings[".eot"] = "application/vnd.ms-fontobject";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ← ریدایرکت روت به Swagger (تداخل با route های دیگر ندارد تا وقتی HomeController روی "/" اترویت نداشته باشد)
app.MapGet("/", () => Results.Redirect("/swagger", permanent: false));

// areas + default routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

try
{
    Log.Information("App starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "App terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
