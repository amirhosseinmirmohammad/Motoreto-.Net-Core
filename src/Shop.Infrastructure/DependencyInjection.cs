using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Background;
using Shop.Infrastructure.Data;
using Shop.Infrastructure.Gateways;
using Shop.Infrastructure.Seeding;
using Shop.Infrastructure.Services;

namespace Shop.Infrastructure;

/// <summary>نقطه ثبت وابستگی‌های لایه Infrastructure (Composition Root).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' یافت نشد.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null);
                })
                .UseSnakeCaseNamingConvention());

        // ---- سرویس‌های فروشگاه ----
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IStockReservationService, StockReservationService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<INewsLetterService, NewsLetterService>();
        services.AddScoped<ISiteMessageService, SiteMessageService>();
        services.AddScoped<ISellerService, SellerService>();
        services.AddScoped<ICompareService, CompareService>();
        services.AddScoped<IPriceHistoryService, PriceHistoryService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddSingleton<ISmsService, Services.ConsoleSmsService>();
        services.AddScoped<ISiteMessageService, SiteMessageService>();

        // ---- HybridCache: L1 حافظه + L2 Redis (سراسری و چندنمونه‌ای) ----
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });

        // ---- Decoratorها (کش دو لایه روی سرویس‌های پرخوانی) ----
        services.AddScoped<CategoryService>();
        services.AddScoped<ICategoryService>(sp =>
            new CachedCategoryService(sp.GetRequiredService<CategoryService>(), sp.GetRequiredService<HybridCache>()));

        services.AddScoped<SliderService>();
        services.AddScoped<ISliderService>(sp =>
            new CachedSliderService(sp.GetRequiredService<SliderService>(), sp.GetRequiredService<HybridCache>()));

        services.AddScoped<ISettingsService, SettingsService>();

        // ---- درگاه‌های پرداخت (Strategy + Factory با Keyed DI) ----
        services.AddHttpClient();
        services.AddHttpContextAccessor();
        services.AddKeyedScoped<IPaymentGateway, FakePaymentGateway>("Fake");
        services.AddKeyedScoped<IPaymentGateway, ZarinpalGateway>("Zarinpal");
        services.AddKeyedScoped<IPaymentGateway, DigipayGateway>("Digipay");
        services.AddKeyedScoped<IPaymentGateway, SnappPayGateway>("SnappPay");
        services.AddKeyedScoped<IPaymentGateway, BaleGateway>("Bale");
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        // ---- Jobهای پس‌زمینه ----
        services.AddHostedService<ReservationCleanupJob>();

        // ---- Seed اولیه ----
        services.AddScoped<DataSeeder>();

        return services;
    }
}
