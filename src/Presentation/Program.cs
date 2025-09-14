using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// config
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

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

// migrate + seed (با ریتری کوتاه)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    for (int i = 0; i < 5; i++)
    {
        try { await db.Database.MigrateAsync(); break; }
        catch when (i < 4) { await Task.Delay(1500); }
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "Administrator", "User", "Operator" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
}

// errors & swagger
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
        c.RoutePrefix = "swagger";
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
