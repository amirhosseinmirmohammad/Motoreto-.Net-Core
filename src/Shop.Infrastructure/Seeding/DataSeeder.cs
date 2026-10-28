using Shop.Domain;
using Shop.Domain.Enums;
using Shop.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.Shared;

namespace Shop.Infrastructure.Seeding;

/// <summary>
/// Seed کامل و idempotent فروشگاه: نقش‌ها، ادمین، تنظیمات، دسته‌ها،
/// محصولات تستی با تصاویر SVG، اسلایدر، وبلاگ، استان/شهر و کد تخفیف.
/// </summary>
public class DataSeeder(
    ApplicationDbContext db,
    UserManager<User> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ILogger<DataSeeder> logger)
{
    public const string AdminEmail = "admin@shop.ir";
    public const string AdminPassword = "Admin@1234";
    public const string DemoUserEmail = "user@shop.ir";
    public const string DemoUserPassword = "User@1234";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync(ct);
        await SeedUsersAsync(ct);
        await SeedSettingsAsync(ct);
        await SeedStatesAsync(ct);
        await SeedCategoriesAsync(ct);
        await SeedProductsAsync(ct);
        await SeedBrandsAndAttributesAsync(ct);
        await SeedSlidersAsync(ct);
        await SeedBlogsAsync(ct);
        await SeedDiscountsAsync(ct);
        await SeedCommentsAsync(ct);
        await SeedMarketplaceAsync(ct);
        await SeedPriceHistoryAsync(ct);
        await SeedStockMovementsAsync(ct);
    }

    // ---------------- Roles & Users ----------------

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        foreach (var role in new[] { "Administrator", "User", "Seller" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
        await Task.CompletedTask;
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await userManager.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new User
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = "مدیر",
                LastName = "فروشگاه",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Administrator");
            else
                logger.LogWarning("ساخت ادمین ناموفق: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (await userManager.FindByEmailAsync(DemoUserEmail) is null)
        {
            var user = new User
            {
                UserName = DemoUserEmail,
                Email = DemoUserEmail,
                EmailConfirmed = true,
                FirstName = "کاربر",
                LastName = "تستی",
                Mobile = "09121234567",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(user, DemoUserPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, "User");
        }

        await db.SaveChangesAsync(ct);
    }

    // ---------------- Settings ----------------

    private async Task SeedSettingsAsync(CancellationToken ct)
    {
        if (await db.SiteSettings.AnyAsync(ct)) return;

        db.SiteSettings.Add(new SiteSettings
        {
            StoreName = "مارکت",
            Tagline = "هر آنچه نیاز دارید، با بهترین قیمت",
            PrimaryColor = "#ef4056",
            SupportPhone = "021-91000000",
            Email = "support@shop.ir",
            Address = "تهران، خیابان ولیعصر، مرکز نوآوری، پلاک ۱",
            InstagramUrl = "https://instagram.com/motoreto.market",
            TelegramUrl = "https://t.me/motoreto_market",
            AboutUs = "موتورتو مارکت از سال ۱۴۰۵ فعالیت خود را با هدف ارائه بهترین محصولات با قیمت مناسب آغاز کرد. ما با تکیه بر تیم پشتیبانی حرفه‌ای و ارسال سریع، تجربه خرید آنلاین لذت‌بخشی برای شما رقم می‌زنیم.",
            ContactUs = "پاسخگوی شما هستیم: شنبه تا پنجشنبه، ۹ صبح تا ۱۸ عصر. از طریق ایمیل، تلفن یا فرم تماس با ما در ارتباط باشید.",
            ShippingPostCost = 490_000,
            ShippingExpressCost = 990_000,
            FreeShippingThreshold = 50_000_000,
            ReservationMinutes = 20,
            MaxQuantityPerProduct = 5,
            LowStockThreshold = 5,
            ReservationCleanupIntervalSeconds = 60,
            PaymentGateway = "Fake"
        });
        await db.SaveChangesAsync(ct);
    }

    // ---------------- States & Cities ----------------

    private static readonly (string State, string[] Cities)[] IranStates =
    [
        ("تهران", ["تهران", "اسلامشهر", "شهریار", "کرج", "قدس", "ورامین"]),
        ("اصفهان", ["اصفهان", "کاشان", "نجف‌آباد", "خمینی‌شهر"]),
        ("خراسان رضوی", ["مشهد", "نیشابور", "سبزوار", "تربت حیدریه"]),
        ("فارس", ["شیراز", "مرودشت", "جهرم", "کازرون"]),
        ("آذربایجان شرقی", ["تبریز", "مراغه", "مرند", "اهر"]),
        ("آذربایجان غربی", ["ارومیه", "خوی", "مهاباد", "بوکان"]),
        ("البرز", ["کرج", "هشتگرد", "نظرآباد", "فردis"]),
        ("گیلان", ["رشت", "انزلی", "لاهیجان", "رودسر"]),
        ("مازندران", ["ساری", "بابل", "آمل", "قائم‌شهر", "نوشهر"]),
        ("خوزستان", ["اهواز", "آبادان", "خرمشهر", "دزفول"]),
        ("کرمان", ["کرمان", "رفسنجان", "سیرجان", "جیرفت"]),
        ("یزد", ["یزد", "میبد", "اردکان", "مهریز"]),
        ("قم", ["قم"]),
        ("مرکزی", ["اراک", "ساوه", "خمین"]),
        ("قزوین", ["قزوین", "تاکستان"]),
        ("همدان", ["همدان", "ملایر", "نهاوند"]),
        ("کردستان", ["سنندج", "سقز", "مریوان"]),
        ("کرمانشاه", ["کرمانشاه", "اسلام‌آباد غرب", "هرسین"]),
        ("ایلام", ["ایلام", "دزفول بالا"]),
        ("لرستان", ["خرم‌آباد", "بروجرد", "دورود"]),
        ("بوشهر", ["بوشهر", "برازجان", "گناوه"]),
        ("هرمزگان", ["بندرعباس", "میناب", "قشم"]),
        ("سیستان و بلوچستان", ["زاهدان", "زابل", "چابهار", "ایرانشهر"]),
        ("گلستان", ["گرگان", "گنبد کاووس", "علی‌آباد"]),
        ("سمنان", ["سمنان", "شاهرود", "دامغان"]),
        ("زنجان", ["زنجان", "ابهر", "خرمدره"]),
        ("اردبیل", ["اردبیل", "پارس‌آباد", "مشگین‌شهر"]),
        ("چهارمحال و بختیاری", ["شهرکرد", "بروجن"]),
        ("کهگیلویه و بویراحمد", ["یاسوج", "دهدشت"]),
        ("خراسان شمالی", ["بجنورد", "شیروان"]),
        ("خراسان جنوبی", ["بیرجند", "قائن"])
    ];

    private async Task SeedStatesAsync(CancellationToken ct)
    {
        if (await db.States.AnyAsync(ct)) return;

        foreach (var (state, cities) in IranStates)
        {
            var s = new State { Name = state };
            s.Cities.AddRange(cities.Select(c => new City { Name = c }));
            db.States.Add(s);
        }
        await db.SaveChangesAsync(ct);
    }

    // ---------------- Categories ----------------

    private static readonly (string Name, string Icon, (string Name, string Icon, string[]? Children)[] Children)[] StoreCategories =
    [
        ("موبایل", "bi-phone",
        [
            ("گوشی موبایل", "bi-phone", ["سامسونگ", "شیائومی", "اپل", "آنر"]),
            ("هدفون و هندزفری", "bi-headphones", null),
            ("پاوربانک", "bi-battery-charging", null),
            ("قاب و گلس", "bi-shield", null)
        ]),
        ("لپ‌تاپ و کامپیوتر", "bi-laptop",
        [
            ("لپ‌تاپ", "bi-laptop", ["گیمینگ", "اداری", "التروبوک"]),
            ("کیبورد و ماوس", "bi-keyboard", null),
            ("مانیتور", "bi-display", null),
            ("هارد و SSD", "bi-device-hdd", null)
        ]),
        ("لوازم خانگی", "bi-house-gear",
        [
            ("لوازم برقی آشپزخانه", "bi-egg-fried", null),
            ("جاروبرقی", "bi-basket", null),
            ("آبمیوه‌گیری و مخلوط‌کن", "bi-cup-straw", null),
            ("اتو و بخارپز", "bi-fire", null)
        ]),
        ("مد و پوشاک", "bi-bag",
        [
            ("پوشاک مردانه", "bi-person", ["تی‌شرت", "شلوار"]),
            ("پوشاک زنانه", "bi-person-heart", ["مانتو", "شومیز"]),
            ("کفش", "bi-boombox", ["کفش مردانه", "کفش زنانه"]),
            ("ساعت", "bi-watch", ["ساعت هوشمند", "ساعت مچی"])
        ]),
        ("آرایشی و بهداشتی", "bi-droplet",
        [
            ("عطر و ادکلن", "bi-flower1", null),
            ("مراقبت پوست", "bi-droplet-half", null),
            ("لوازم آرایش", "bi-palette", null)
        ]),
        ("ورزش و سفر", "bi-bicycle",
        [
            ("لوازم کمپینگ", "bi-tree", null),
            ("دوچرخه", "bi-bicycle", null),
            ("لوازم بدنسازی", "bi-heart-pulse", null)
        ]),
        ("کتاب و لوازم تحریر", "bi-book",
        [
            ("کتاب چاپی", "bi-book", null),
            ("نوشت‌افزار", "bi-pencil", null),
            ("کوله و کیف", "bi-backpack", null)
        ]),
        ("اسباب‌بازی و کودک", "bi-balloon",
        [
            ("عروسک و پولیشی", "bi-emoji-smile", null),
            ("بازی فکری", "bi-puzzle", null),
            ("سیسمونی", "bi-stars", null)
        ])
    ];

    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(ct)) return;

        var order = 0;
        foreach (var (name, icon, children) in StoreCategories)
        {
            var parent = new Category
            {
                Name = name,
                Slug = SlugHelper.Slugify(name),
                Icon = icon,
                SortOrder = order++,
                IsActive = true
            };
            db.Categories.Add(parent);
            await db.SaveChangesAsync(ct);

            var childOrder = 0;
            foreach (var (childName, childIcon, grandChildren) in children)
            {
                var child = new Category
                {
                    Name = childName,
                    Slug = SlugHelper.Slugify(childName),
                    Icon = childIcon,
                    ParentId = parent.Id,
                    SortOrder = childOrder++,
                    IsActive = true
                };
                db.Categories.Add(child);
                await db.SaveChangesAsync(ct);

                if (grandChildren is not null)
                {
                    var grandOrder = 0;
                    foreach (var gc in grandChildren)
                    {
                        db.Categories.Add(new Category
                        {
                            Name = gc,
                            Slug = SlugHelper.Slugify($"{childName}-{gc}"),
                            ParentId = child.Id,
                            SortOrder = grandOrder++,
                            IsActive = true
                        });
                    }
                    await db.SaveChangesAsync(ct);
                }
            }
        }
    }

    // ---------------- Products ----------------

    private static readonly (string Cat, string Name, string English, long PriceToman, byte Discount, int Stock, bool Amazing, bool Special)[] ProductsData =
    [
        // گوشی موبایل
        ("گوشی موبایل", "گوشی موبایل سامسونگ Galaxy A55 5G ظرفیت ۲۵۶ گیگابایت", "Samsung Galaxy A55 5G", 24_900_000, 12, 15, true, true),
        ("گوشی موبایل", "گوشی موبایل شیائومی Redmi Note 13 Pro ظرفیت ۲۵۶ گیگابایت", "Xiaomi Redmi Note 13 Pro", 18_500_000, 15, 22, true, false),
        ("گوشی موبایل", "گوشی موبایل اپل iPhone 15 ظرفیت ۱۲۸ گیگابایت", "Apple iPhone 15", 62_000_000, 5, 8, false, true),
        ("گوشی موبایل", "گوشی موبایل سامسونگ Galaxy S24 Ultra ظرفیت ۵۱۲ گیگابایت", "Samsung Galaxy S24 Ultra", 89_000_000, 8, 5, false, true),
        ("گوشی موبایل", "گوشی موبایل شیائومی Poco X6 Pro ظرفیت ۵۱۲ گیگابایت", "Xiaomi Poco X6 Pro", 22_900_000, 10, 12, false, false),
        ("گوشی موبایل", "گوشی موبایل آنر X9b ظرفیت ۲۵۶ گیگابایت", "Honor X9b", 17_800_000, 0, 18, false, false),
        // هدفون
        ("هدفون و هندزفری", "هدفون بی‌سیم سونی WH-CH720N", "Sony WH-CH720N", 7_900_000, 20, 25, true, false),
        ("هدفون و هندزفری", "هندزفری بلوتوثی اپل AirPods Pro 2", "Apple AirPods Pro 2", 14_500_000, 10, 14, false, true),
        ("هدفون و هندزفری", "هدفون گیمینگ ریزر Kraken V3", "Razer Kraken V3", 5_400_000, 0, 9, false, false),
        ("هدفون و هندزفری", "ایرباد بی‌سیم شیائومی Redmi Buds 5", "Xiaomi Redmi Buds 5", 1_450_000, 25, 40, true, false),
        // پاوربانک
        ("پاوربانک", "پاوربانک شیائومی ۲۰۰۰۰ میلی‌آمپر Pro", "Xiaomi Power Bank Pro", 1_890_000, 15, 30, false, false),
        ("پاوربانک", "پاوربانک انکر PowerCore ۱۰۰۰۰ میلی‌آمپر", "Anker PowerCore", 2_350_000, 0, 20, false, false),
        // قاب و گلس
        ("قاب و گلس", "گلس محافظ صفحه نمایش سرامیک مناسب Galaxy A55", "Ceramic Glass A55", 280_000, 30, 100, false, false),
        ("قاب و گلس", "قاب سیلیکونی اورجینال مناسب iPhone 15", "Silicone Case iPhone 15", 690_000, 0, 60, false, false),
        // لپ‌تاپ
        ("لپ‌تاپ", "لپ‌تاپ ایسوس VivoBook 15 مدل X1504-core i5", "Asus VivoBook X1504", 38_900_000, 10, 7, true, true),
        ("لپ‌تاپ", "لپ‌تاپ لنوو IdeaPad 3 مدل 15ITL6", "Lenovo IdeaPad 3 15ITL6", 33_500_000, 8, 9, false, false),
        ("لپ‌تاپ", "لپ‌تاپ ایسوس TUF Gaming F15 RTX4060", "Asus TUF Gaming F15", 78_000_000, 5, 4, false, true),
        ("لپ‌تاپ", "لپ‌تاپ مک‌بوک ایر M3 ظرفیت ۲۵۶ گیگابایت", "Apple MacBook Air M3", 85_000_000, 3, 5, false, true),
        // کیبورد و ماوس
        ("کیبورد و ماوس", "ماوس بی‌سیم لاجیتچ MX Master 3S", "Logitech MX Master 3S", 6_200_000, 12, 16, false, false),
        ("کیبورد و ماوس", "کیبورد مکانیکال ریزر BlackWidow V4", "Razer BlackWidow V4", 8_900_000, 0, 6, false, false),
        ("کیبورد و ماوس", "ست کیبورد و ماوس بی‌سیم لوگی MK540", "Logitech MK540", 3_100_000, 18, 14, false, false),
        // مانیتور
        ("مانیتور", "مانیتور سامسونگ ۲۷ اینچ Odyssey G5", "Samsung Odyssey G5", 21_500_000, 10, 8, true, false),
        ("مانیتور", "مانیتور ال‌جی ۲۴ اینچ 24MP400", "LG 24MP400", 9_200_000, 0, 11, false, false),
        // هارد
        ("هارد و SSD", "هارد SSD سامسونگ 980 PRO ظرفیت ۱ ترابایت", "Samsung 980 PRO 1TB", 8_400_000, 15, 19, false, false),
        ("هارد و SSD", "هارد اکسترنال وسترن دیجیتال ۲ ترابایت", "WD Elements 2TB", 4_700_000, 5, 13, false, false),
        // لوازم برقی
        ("لوازم برقی آشپزخانه", "سرخ‌کن بدون روغن فیلیپس Airfryer XXL", "Philips Airfryer XXL", 24_500_000, 20, 10, true, true),
        ("لوازم برقی آشپزخانه", "چای‌ساز برقی فلر مدل TS 512", "Flama TS 512", 3_900_000, 0, 15, false, false),
        ("لوازم برقی آشپزخانه", "تست برقی تفال مدل Expression", "Tefal Expression", 4_500_000, 12, 12, false, false),
        ("لوازم برقی آشپزخانه", "قهوه‌ساز دلونگی مدل EC885", "Delonghi EC885", 31_000_000, 8, 5, false, true),
        // جاروبرقی
        ("جاروبرقی", "جاروبرقی سامسونگ مدل VC18M", "Samsung VC18M", 12_900_000, 10, 8, false, false),
        ("جاروبرقی", "جارو شارژی شیائومی G11 Plus", "Xiaomi G11 Plus", 18_900_000, 15, 6, true, false),
        // مخلوط‌کن
        ("آبمیوه‌گیری و مخلوط‌کن", "مخلوط‌کن و غذاساز بوش مدل MCM3501", "Bosch MCM3501", 11_500_000, 0, 9, false, false),
        ("آبمیوه‌گیری و مخلوط‌کن", "آبمیوه‌گیری گاسونیک مدل G-988", "Gasonik G-988", 7_800_000, 10, 11, false, false),
        // اتو
        ("اتو و بخارپز", "اتو بخار فیلیپس مدل GC1740", "Philips GC1740", 3_400_000, 0, 20, false, false),
        ("اتو و بخارپز", "مخزن بخار پولتی تفال مدل Pro Express", "Tefal Pro Express", 19_900_000, 12, 4, false, false),
        // پوشاک
        ("پوشاک مردانه", "تی‌شرت آستین کوتاه مردانه طرح دار", "Men T-Shirt", 890_000, 20, 50, false, false),
        ("پوشاک مردانه", "شلوار جین مردانه مدل اسلیم فیت", "Slim Fit Jeans", 1_650_000, 10, 35, false, false),
        ("پوشاک زنانه", "مانتو جلو باز زنانه پارچه کرپ", "Women Mantle", 2_250_000, 15, 28, false, false),
        ("پوشاک زنانه", "شومیز زنانه آستین بلند ساتن", "Women Satin Shirt", 1_290_000, 0, 30, false, false),
        ("کفش", "کفش راحتی مردانه نیوبالانس مدل 574", "New Balance 574", 6_800_000, 25, 18, true, false),
        ("کفش", "کتانی ورزشی زنانه آدیداس مدل Grand Court", "Adidas Grand Court", 5_400_000, 10, 15, false, false),
        ("ساعت", "ساعت هوشمند شیائومی Redmi Watch 4", "Xiaomi Redmi Watch 4", 4_900_000, 18, 22, true, false),
        ("ساعت", "ساعت مچی عقربه‌ای مردانه کاسیو MTP-1303", "Casio MTP-1303", 3_200_000, 0, 12, false, false),
        // آرایشی
        ("عطر و ادکلن", "عطر مردانه دیور Sauvage حجم ۱۰۰ میلی‌لیتر", "Dior Sauvage", 12_500_000, 8, 10, false, true),
        ("عطر و ادکلن", "ادکلن زنانه لانکوم La Vie Est Belle", "Lancome La Vie Est Belle", 9_800_000, 5, 8, false, false),
        ("مراقبت پوست", "کرم مرطوب‌کننده سراوی مدل Moisturizing Cream", "CeraVe Cream", 1_950_000, 10, 40, false, false),
        ("مراقبت پوست", "ضد آفتاب لافارر SPF50 مناسب پوست چرب", "La Farrerr SPF50", 890_000, 0, 60, false, false),
        ("لوازم آرایش", "پالت سایه چشم ۱۲ رنگ میبلین", "Maybelline Palette", 1_450_000, 15, 25, false, false),
        // ورزش
        ("لوازم کمپینگ", "چادر مسافرتی ۴ نفره ضد آب", "Camping Tent 4P", 8_900_000, 20, 7, false, false),
        ("لوازم کمپینگ", "کیف خواب کوهنوردی مدل Arctic 500", "Arctic 500 Sleeping Bag", 4_200_000, 0, 12, false, false),
        ("دوچرخه", "دوچرخه کوهستان اورلورد مدل 27.5 اینچ", "Overlord MTB 27.5", 15_900_000, 10, 6, true, false),
        ("لوازم بدنسازی", "دمبل هالتر ست ۲۰ کیلوگرمی", "Dumbbell Set 20kg", 6_500_000, 0, 10, false, false),
        // کتاب
        ("کتاب چاپی", "کتاب اثر مرکب نوشته دارن هاردی", "Compound Effect", 320_000, 10, 80, false, false),
        ("کتاب چاپی", "کتاب هنر شفاف اندیشیدن رولف دوبلی", "The Art of Thinking Clearly", 380_000, 0, 45, false, false),
        ("نوشت‌افزار", "خودکار بیک مدل Cristal بسته ۵۰ عددی", "Bic Cristal 50pcs", 450_000, 20, 100, false, false),
        ("کوله و کیف", "کوله‌پشتی لپ‌تاپ ضد آب مدل Business Pro", "Business Pro Backpack", 2_850_000, 12, 25, false, false),
        // اسباب‌بازی
        ("عروسک و پولیشی", "عروسک پولیشی خرس تدی ۸۰ سانتی", "Teddy Bear 80cm", 1_890_000, 25, 20, true, false),
        ("بازی فکری", "بازی فکری منچ حرفه‌ای چوبی", "Wooden Ludo", 690_000, 0, 30, false, false),
        ("بازی فکری", "مکعب جادویی ۵ در ۵ گسپر", "Ganper 5x5 Cube", 480_000, 10, 40, false, false),
        ("سیسمونی", "پستانک سیلیکونی فیلیپس Avent بسته ۲ عددی", "Philips Avent 2pcs", 580_000, 0, 55, false, false)
    ];

    private async Task SeedProductsAsync(CancellationToken ct)
    {
        if (await db.Products.AnyAsync(ct)) return;

        var brandIds = await db.Brands.AsNoTracking().Select(b => (int?)b.Id).ToListAsync(ct);

        var childCategories = await db.Categories
            .Where(c => c.ParentId != null)
            .ToDictionaryAsync(c => c.Name, c => c.Id, ct);

        var rootCategories = await db.Categories
            .Where(c => c.ParentId == null)
            .ToDictionaryAsync(c => c.Name, c => (Id: c.Id, Name: c.Name), ct);

        var random = new Random(42);
        var counter = 0;

        foreach (var (cat, name, english, priceToman, discount, stock, amazing, special) in ProductsData)
        {
            counter++;
            var categoryId = childCategories.TryGetValue(cat, out var cid)
                ? cid
                : rootCategories.Values.First().Id;

            var slug = SlugHelper.Slugify(name);
            var imageFile = $"p{counter:D3}.svg";

            // عکس واقعی WebP (سبک) — از قبل در wwwroot موجود است
            var mainImage = $"/images/products/real/{counter}.webp";

            var product = new Product
            {
                Name = name,
                EnglishName = english,
                Slug = slug,
                Code = $"SH-{100000 + counter}",
                UnitPrice = priceToman * 10, // تومان → ریال
                DiscountPercent = discount,
                Stock = stock,
                Description = BuildDescription(name, english),
                CategoryId = categoryId,
                MainImage = mainImage,
                IsAmazing = amazing,
                AmazingEndsAt = amazing ? DateTime.UtcNow.AddDays(3) : null,
                IsSpecial = special,
                IsActive = true,
                SalesCount = random.Next(0, 200),
                BrandId = brandIds.Count > 0 ? brandIds[random.Next(brandIds.Count)] : null,
                ViewCount = random.Next(50, 5000),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 90))
            };

            db.Products.Add(product);
            await db.SaveChangesAsync(ct);

            // گالری: دو تصویر مکمل
            db.ProductImages.AddRange(
                new ProductImage
                {
                    ProductId = product.Id,
                    Url = mainImage,
                    Alt = name,
                    SortOrder = 0
                },
                new ProductImage
                {
                    ProductId = product.Id,
                    Url = SvgImageFactory.Save("images/products", $"p{counter:D3}-2.svg",
                        SvgImageFactory.ProductImage(english, "نمای دوم محصول"),
                        ContentRoot()),
                    Alt = english,
                    SortOrder = 1
                });
        }

        await db.SaveChangesAsync(ct);
    }

    private static string ContentRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        var idx = baseDir.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
        return idx > 0 ? baseDir[..idx] : Directory.GetCurrentDirectory();
    }

    private static string BuildDescription(string name, string english) => $"""
        <p><strong>{name}</strong> یکی از محبوب‌ترین محصولات دسته خود است که با کیفیت ساخت بالا و قیمت مناسب، انتخابی هوشمندانه برای شماست.</p>
        <h3>ویژگی‌های اصلی</h3>
        <ul>
        <li>کیفیت ساخت عالی با ضمانت اصالت کالا</li>
        <li>پشتیبانی کامل پس از فروش</li>
        <li>امکان بازگشت کالا تا ۷ روز پس از خرید</li>
        <li>ارسال سریع به سراسر کشور</li>
        </ul>
        <h3>مشخصات فنی</h3>
        <table>
        <tr><td>برند</td><td>{english}</td></tr>
        <tr><td>گارانتی</td><td>۱۸ ماه گارانتی شرکتی</td></tr>
        <tr><td>اصالت کالا</td><td>تضمین اصالت و سلامت فیزیکی</td></tr>
        </table>
        <p>این محصول با <strong>گارانتی معتبر</strong> و <strong>ضمانت اصالت</strong> عرضه می‌شود و در صورت عدم رضایت، تا ۷ روز امکان بازگشت دارد.</p>
        """;

    // ---------------- Sliders / Blogs / Discounts / Comments ----------------

    // ---------------- Brands & Category Attributes ----------------

    /// <summary>
    /// برندها + ویژگی‌های داینامیک دسته‌بندی (ادمین-تعریف، دست باز).
    /// Idempotent — روی دیتابیس‌های موجود هم Backfill می‌کند (برند + ویژگی‌های نمونه).
    /// </summary>
    private async Task SeedBrandsAndAttributesAsync(CancellationToken ct)
    {
        var rnd = new Random(42);

        // ---- برندها ----
        if (!await db.Brands.AnyAsync(ct))
        {
            db.Brands.AddRange(
            [
                new Brand { Name = "اپل", Slug = "apple" },
                new Brand { Name = "سامسونگ", Slug = "samsung" },
                new Brand { Name = "شیائومی", Slug = "xiaomi" },
                new Brand { Name = "ال‌جی", Slug = "lg" },
                new Brand { Name = "سونی", Slug = "sony" },
                new Brand { Name = "ایسوس", Slug = "asus" },
                new Brand { Name = "لنوو", Slug = "lenovo" },
                new Brand { Name = "نوکیا", Slug = "nokia" },
                new Brand { Name = "هوآوی", Slug = "huawei" },
                new Brand { Name = "هایسنس", Slug = "hisense" }
            ]);
            await db.SaveChangesAsync(ct);
        }

        var brands = await db.Brands.AsNoTracking().ToListAsync(ct);

        // ---- Backfill: برند + ویژگی‌های نمونه برای محصولات فاقد برند ----
        var orphans = await db.Products
            .Include(p => p.Category)
            .ThenInclude(c => c!.Parent)
            .Include(p => p.Category)
            .ThenInclude(c => c!.Parent!)
            .ThenInclude(c => c.Parent)
            .Where(p => p.BrandId == null)
            .ToListAsync(ct);

        static IEnumerable<string> CategoryChain(Category? c)
        {
            while (c is not null)
            {
                yield return c.Name;
                c = c.Parent;
            }
        }

        foreach (var p in orphans)
        {
            var brand = brands[rnd.Next(brands.Count)];
            p.BrandId = brand.Id;
            p.AttributesJson = MergeAttrGroup(p.AttributesJson, "برند", brand.Name);

            // نام خود دسته + همه اجدادش (چون محصول در سطح-۳ است ولی ویژگی‌ها مال شاخه‌اند)
            var chain = string.Join('|', CategoryChain(p.Category));

            if (chain.Contains("موبایل") || chain.Contains("گوشی"))
            {
                var storages = new[] { "۶۴ گیگابایت", "۱۲۸ گیگابایت", "۲۵۶ گیگابایت" };
                var nets = new[] { "4G", "5G" };
                p.AttributesJson = MergeAttrGroup(p.AttributesJson, "حافظه داخلی", storages[rnd.Next(storages.Length)]);
                p.AttributesJson = MergeAttrGroup(p.AttributesJson, "شبکه ارتباطی", nets[rnd.Next(nets.Length)]);
            }
            else if (chain.Contains("لپ"))
            {
                var rams = new[] { "۸ گیگابایت", "۱۶ گیگابایت", "۳۲ گیگابایت" };
                p.AttributesJson = MergeAttrGroup(p.AttributesJson, "حافظه رم", rams[rnd.Next(rams.Length)]);
            }
        }

        if (orphans.Count > 0)
            await db.SaveChangesAsync(ct);

        // ---- ویژگی‌های داینامیک دسته‌بندی (نمونه اولیه — ادمین در پنل مدیریت می‌کند) ----
        if (!await db.CategoryAttributes.AnyAsync(ct))
        {
            var cats = await db.Categories.AsNoTracking().ToListAsync(ct);
            var adds = new List<CategoryAttribute>();

            void AddIf(IEnumerable<Category> list, params (string Name, bool Variant)[] defs)
            {
                foreach (var c in list)
                    foreach (var (n, v) in defs)
                        adds.Add(new CategoryAttribute { CategoryId = c.Id, Name = n, ShowInFilter = !v, IsVariantOption = v });
            }

            AddIf(cats.Where(c => c.Name.Contains("موبایل") || c.Name.Contains("گوشی")),
                ("حافظه داخلی", false), ("شبکه ارتباطی", false), ("رنگ", true));
            AddIf(cats.Where(c => c.Name.Contains("لپ")),
                ("پردازنده", false), ("حافظه رم", false));
            AddIf(cats.Where(c => c.Name.Contains("پوشاک") || c.Name.Contains("تی‌شرت") || c.Name.Contains("کفش")),
                ("جنس", false), ("طرح", false), ("سایز", true));

            db.CategoryAttributes.AddRange(adds);
            await db.SaveChangesAsync(ct);
        }
    }

    /// <summary>گروه ویژگی را به آرایه AttributesJson اضافه می‌کند (بدون تکرار).</summary>
    private static string MergeAttrGroup(string? json, string name, string value)
    {
        var groups = new List<AttrGroupSeed>();
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<List<AttrGroupSeed>>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed is not null) groups = parsed;
            }
            catch { /* JSON خراب — از صفر می‌سازیم */ }
        }

        if (groups.Any(g => g.Name == name))
            return SerializeGroups(groups);

        groups.Insert(0, new AttrGroupSeed { Name = name, Values = [value] });
        return SerializeGroups(groups);
    }

    private static string SerializeGroups(List<AttrGroupSeed> groups) =>
        System.Text.Json.JsonSerializer.Serialize(
            groups.Select(g => new { name = g.Name, values = g.Values }));

    private sealed class AttrGroupSeed
    {
        public string Name { get; set; } = default!;
        public List<string>? Values { get; set; }
    }

    private async Task SeedSlidersAsync(CancellationToken ct)
    {
        if (await db.Sliders.AnyAsync(ct)) return;

        var root = ContentRoot();

        (string Title, string Sub, string Theme, int Order)[] sliders =
        [
            ("حراج جمعه شگفت‌انگیز", "تا ۵۰٪ تخفیف روی هزاران کالا", "#ef4056", 1),
            ("فروش ویژه موبایل و لپ‌تاپ", "جدیدترین مدل‌ها با بهترین قیمت", "#3b82f6", 2),
            ("دنیای لوازم خانگی", "خانه‌ای هوشمند با موتورتو", "#10b981", 3),
            ("مد و پوشاک فصل", "برندهای محبوب، قیمت باورنکردنی", "#8b5cf6", 4)
        ];

        // عکس واقعی WebP برای هر اسلایدر (فایل‌های موجود در wwwroot)
        var sliderImages = new Dictionary<int, string>
        {
            [1] = "/images/products/real/1.webp",
            [2] = "/images/products/real/17.webp",
            [3] = "/images/products/real/35.webp",
            [4] = "/images/products/real/50.webp"
        };

        foreach (var (title, sub, theme, order) in sliders)
        {
            var url = sliderImages[order];

            db.Sliders.Add(new Slider
            {
                Title = title,
                SubTitle = sub,
                Image = url,
                Link = "/products",
                Theme = theme,
                SortOrder = order,
                IsActive = true
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedBlogsAsync(CancellationToken ct)
    {
        if (await db.Blogs.AnyAsync(ct)) return;

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        var root = ContentRoot();

        (string Title, string Short, string Body)[] posts =
        [
            ("راهنمای خرید گوشی موبایل در سال ۱۴۰۵",
                "چطور بین ده‌ها مدل گوشی، بهترین انتخاب را انجام دهیم؟ در این راهنما نکات کلیدی خرید موبایل را مرور می‌کنیم.",
                "<p>خرید گوشی موبایل یکی از مهم‌ترین تصمیم‌های خرید امروزی است. در این مقاله مهم‌ترین معیارها را بررسی می‌کنیم.</p><h3>۱. بودجه خود را مشخص کنید</h3><p>پیش از هر چیز بازه قیمتی خود را مشخص کنید. بازار موبایل از ۸ میلیون تومان تا بالای ۱۰۰ میلیون تومان گزینه دارد.</p><h3>۲. دوربین مهم است یا باتری؟</h3><p>اگر عکاسی برایتان اولویت دارد سراغ مدل‌هایی با سنسور بزرگ بروید؛ اگر کاربری طولانی دارید، ظرفیت باتری بالای ۵۰۰۰ میلی‌آمپر انتخاب هوشمندانه‌ای است.</p><h3>۳. حافظه و رم</h3><p>حداقل ۲۵۶ گیگابایت حافظه و ۸ گیگابایت رم را برای کاربری روان توصیه می‌کنیم.</p>"),
            ("۱۰ ترفند کاربردی برای افزایش عمر باتری لپ‌تاپ",
                "با این ترفندهای ساده، باتری لپ‌تاپ شما سال‌ها سالم می‌ماند.",
                "<p>باتری لپ‌تاپ یکی از پرتکرارترین قطعات معیوب است. با رعایت این نکات عمر آن را چند برابر کنید.</p><h3>شارژ بین ۲۰ تا ۸۰ درصد</h3><p>نگه‌داشتن شارژ در این بازه، فشار شیمیایی روی سلول‌ها را به حداقل می‌رساند.</p><h3>دوری از حرارت</h3><p>گرما دشمن شماره یک باتری است. تهویه لپ‌تاپ را مسدود نکنید.</p>"),
            ("بهترین لوازم برقی آشپزخانه برای آشپزهای تازه‌کار",
                "اگر تازه آشپزی را شروع کرده‌اید، این ۵ وسیله را در اولویت خرید قرار دهید.",
                "<p>آشپزخانه مجهز یعنی آشپزی لذت‌بخش‌تر. این پنج وسیله را در اولویت بگذارید.</p><h3>۱. سرخ‌کن بدون روغن</h3><p>غذای سالم‌تر با روغن کمتر — محبوب‌ترین وسیله آشپزخانه‌های امروزی.</p><h3>۲. مخلوط‌کن چندکاره</h3><p>از اسموتی تا خمیر پیتزا با یک دستگاه.</p>"),
            ("چطور کفش ورزشی مناسب انتخاب کنیم؟",
                "انتخاب کفش اشتباه می‌تواند به زانوهای شما آسیب بزند. این راهنما را از دست ندهید.",
                "<p>کفش ورزشی مناسب، عملکرد ورزشی و سلامت مفاصل شما را تضمین می‌کند.</p><h3>سایز دقیق پا</h3><p>پای خود را عصر — بعد از فعالیت روزانه — اندازه بگیرید.</p><h3>نوع کاربری</h3><p>برای دویدن، پیاده‌روی و باشگاه کفش‌های متفاوتی طراحی شده است.</p>"),
            ("معرفی پرفروش‌ترین کتاب‌های توسعه فردی",
                "پنج کتابی که دیدگاه هزاران خواننده را تغییر داده است.",
                "<p>کتاب‌های توسعه فردی می‌توانند نقطه عطف زندگی باشند. این پنج عنوان را از دست ندهید.</p><h3>اثر مرکب</h3><p>تصمیم‌های کوچک روزانه، نتایج بزرگ می‌سازند.</p><h3>هنر شفاف اندیشیدن</h3><p>محافظ در برابر خطاهای شناختی رایج.</p>"),
            ("ژست مینیمال در خرید؛ کمتر اما بهتر",
                "چرا خرید کمتر اما باکیفیت‌تر، هم اقتصادی‌تر است هم لذت‌بخش‌تر؟",
                "<p>سبک زندگی مینیمال درباره حذف زائدات است — از کمد لباس تا گوشی موبایل.</p><h3>کیفیت بر کمیت</h3><p>یک کالای باکیفیت، جایگزین پنج کالای بی‌کیفیت می‌شود.</p><h3>خرید آگاهانه</h3><p>پیش از خرید، نیاز واقعی خود را بپرسید.</p>")
        ];

        var i = 0;
        int[] blogImages = [1, 17, 35, 50, 55, 52];
        foreach (var (title, shortDesc, body) in posts)
        {
            i++;
            var url = $"/images/products/real/{blogImages[i - 1]}.webp";

            db.Blogs.Add(new Blog
            {
                Title = title,
                Slug = SlugHelper.Slugify(title),
                ShortDescription = shortDesc,
                Body = body,
                CoverImage = url,
                IsVisible = true,
                ViewCount = Random.Shared.Next(100, 3000),
                AuthorId = admin!.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedDiscountsAsync(CancellationToken ct)
    {
        if (await db.Discounts.AnyAsync(ct)) return;

        db.Discounts.AddRange(
            new Discount
            {
                Code = "WELCOME",
                Title = "خوش‌آمدگویی: ۱۰٪ تخفیف اولین خرید",
                Percent = 10,
                MaxAmount = 2_000_000_00,
                MinOrderAmount = 1_000_000_00,
                UsageLimit = 1000,
                ExpireAt = DateTime.UtcNow.AddMonths(6),
                IsActive = true
            },
            new Discount
            {
                Code = "DIGI20",
                Title = "جشنواره ویژه: ۲۰٪ تخفیف ویژه",
                Percent = 20,
                MaxAmount = 5_000_000_00,
                MinOrderAmount = 5_000_000_00,
                UsageLimit = 200,
                ExpireAt = DateTime.UtcNow.AddMonths(2),
                IsActive = true
            });
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCommentsAsync(CancellationToken ct)
    {
        if (await db.Comments.AnyAsync(ct)) return;

        var user = await userManager.FindByEmailAsync(DemoUserEmail);
        if (user is null) return;

        var products = await db.Products.OrderBy(p => p.Id).Take(8).ToListAsync(ct);
        string[] texts =
        [
            "کیفیت ساخت فوق‌العاده‌ای داره، ارسال هم خیلی سریع بود. کاملاً راضی‌ام.",
            "به نسبت قیمتش عالیه. بسته‌بندی هم خیلی مرتب و حرفه‌ای بود.",
            "چند هفته دارمش و کاملاً راضی‌ام. پیشنهاد می‌کنم حتماً بخرید.",
            "مطابق توضیحات بود. فقط کاش رنگ‌بندی بیشتری داشت.",
            "بهترین خریدی که این ماه کردم! پشتیبانی فروشگاه هم واقعاً پاسخگو بود."
        ];

        for (var i = 0; i < products.Count; i++)
        {
            db.Comments.Add(new Comment
            {
                ProductId = products[i].Id,
                UserId = user.Id,
                Rating = (byte)Random.Shared.Next(4, 6),
                Text = texts[i % texts.Length],
                IsApproved = true,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
            });
        }
        await db.SaveChangesAsync(ct);
    }

    // ---------------- مارکت‌پلیس: فروشندگان، آفرها، تنوع‌ها ----------------

    public const string Seller1Email = "seller1@shop.ir";
    public const string Seller2Email = "seller2@shop.ir";
    public const string SellerPassword = "Seller@1234";

    private async Task SeedMarketplaceAsync(CancellationToken ct)
    {
        if (await db.Sellers.AnyAsync(ct)) return;

        // کاربران فروشنده
        var seller1 = await EnsureUserAsync(Seller1Email, "تک‌لند استور", SellerPassword, ct);
        var seller2 = await EnsureUserAsync(Seller2Email, "پارس مارکت", SellerPassword, ct);
        var pending = await EnsureUserAsync("seller3@shop.ir", "فروشگاه نمونه", SellerPassword, ct);

        // فروشگاه‌های تأییدشده
        var s1 = new Seller
        {
            StoreName = "تک‌لند استور",
            Slug = "تک-لند-استور",
            OwnerUserId = seller1.Id,
            Description = "فروشگاه تخصصی کالای دیجیتال با ۵ سال سابقه و ضمانت اصالت کالا.",
            BusinessLicenseNumber = "14003512345",
            Phone = "02188776655",
            Status = SellerStatus.Approved,
            CommissionPercent = 6,
            CreatedAt = DateTime.UtcNow.AddDays(-120),
            ApprovedAt = DateTime.UtcNow.AddDays(-118)
        };
        var s2 = new Seller
        {
            StoreName = "پارس مارکت",
            Slug = "پارس-مارکت",
            OwnerUserId = seller2.Id,
            Description = "عرضه‌کننده لوازم خانگی و پوشاک با قیمت رقابتی.",
            BusinessLicenseNumber = "14009876543",
            Phone = "03134567890",
            Status = SellerStatus.Approved,
            CommissionPercent = 8,
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            ApprovedAt = DateTime.UtcNow.AddDays(-88)
        };
        var s3 = new Seller
        {
            StoreName = "فروشگاه نمونه (در انتظار تأیید)",
            Slug = "فروشگاه-نمونه",
            OwnerUserId = pending.Id,
            Description = "این فروشنده تازه ثبت‌نام کرده و منتظر بررسی مدارک است.",
            BusinessLicenseNumber = "14011223344",
            Status = SellerStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        db.Sellers.AddRange(s1, s2, s3);
        await db.SaveChangesAsync(ct);

        // تنوع‌های نمونه (رنگ/حافظه برای گوشی، رنگ/سایز برای پوشاک)
        await SeedVariantDemoAsync(ct);

        // پیشنهادهای فروش روی کالاهای پرفروش مارکت
        var products = await db.Products.OrderBy(p => p.Id).Take(12).ToListAsync(ct);
        var offersAdded = 0;

        for (var i = 0; i < products.Count; i++)
        {
            var p = products[i];
            if (p.Stock < 3) continue;

            if (i % 2 == 0)
            {
                db.SellerOffers.Add(MakeOffer(p, s1, priceFactor: 0.97, stock: Math.Min(5, p.Stock)));
                offersAdded++;
            }
            if (i % 3 == 0)
            {
                db.SellerOffers.Add(MakeOffer(p, s2, priceFactor: 1.04, stock: Math.Min(3, p.Stock)));
                offersAdded++;
            }
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("مارکت‌پلیس: {Sellers} فروشنده و {Offers} پیشنهاد فروش ساخته شد", 3, offersAdded);
    }

    private static SellerOffer MakeOffer(Product p, Seller seller, double priceFactor, int stock) => new()
    {
        ProductId = p.Id,
        SellerId = seller.Id,
        Price = (long)Math.Round(p.UnitPrice * priceFactor / 1000.0, MidpointRounding.AwayFromZero) * 1000,
        DiscountPercent = p.DiscountPercent,
        Stock = stock,
        Warranty = "گارانتی ۶ ماهه فروشگاه",
        DeliveryDays = Random.Shared.Next(2, 5),
        CreatedAt = DateTime.UtcNow.AddDays(-30)
    };

    private async Task<User> EnsureUserAsync(string email, string firstName, string password, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null) return user;

        user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = "فروشنده",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, "Seller");

        return user;
    }

    /// <summary>تنوع نمونه: گوشی (رنگ + حافظه) و تی‌شرت (رنگ + سایز).</summary>
    private async Task SeedVariantDemoAsync(CancellationToken ct)
    {
        var phone = await db.Products.FirstOrDefaultAsync(p => p.Code == "SH-100001", ct);
        if (phone is not null && !await db.ProductVariants.AnyAsync(v => v.ProductId == phone.Id, ct))
        {
            phone.AttributesJson = """[{"name":"رنگ","values":["آبی","مشکی","بنفش"]},{"name":"حافظه","values":["۱۲۸ گیگ","۲۵۶ گیگ"]}]""";

            var colors = new[] { ("آبی", "p001.svg"), ("مشکی", "p001-2.svg"), ("بنفش", "p001.svg") };
            var storages = new[] { ("۱۲۸ گیگ", 0L), ("۲۵۶ گیگ", 15_000_000L) };
            var rnd = new Random(7);

            foreach (var (color, img) in colors)
            foreach (var (storage, extra) in storages)
            {
                db.ProductVariants.Add(new ProductVariant
                {
                    ProductId = phone.Id,
                    AttributesJson = ProductVariant.BuildAttributesJson(("رنگ", color), ("حافظه", storage)),
                    Price = phone.UnitPrice + extra,
                    DiscountPercent = phone.DiscountPercent,
                    Stock = rnd.Next(3, 10),
                    Image = $"/images/products/{img}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var shirt = await db.Products.FirstOrDefaultAsync(p => p.Name.Contains("تی‌شرت آستین کوتاه مردانه"), ct);
        if (shirt is not null && !await db.ProductVariants.AnyAsync(v => v.ProductId == shirt.Id, ct))
        {
            shirt.AttributesJson = """[{"name":"رنگ","values":["سفید","مشکی","سرمه‌ای"]},{"name":"سایز","values":["M","L","XL"]}]""";

            foreach (var color in new[] { "سفید", "مشکی", "سرمه‌ای" })
            foreach (var size in new[] { "M", "L", "XL" })
            {
                db.ProductVariants.Add(new ProductVariant
                {
                    ProductId = shirt.Id,
                    AttributesJson = ProductVariant.BuildAttributesJson(("رنگ", color), ("سایز", size)),
                    Price = shirt.UnitPrice,
                    DiscountPercent = shirt.DiscountPercent,
                    Stock = Random.Shared.Next(2, 8),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>تاریخچه قیمت: ۶ نقطه در ۹۰ روز گذشته تا قیمت فعلی (برای نمودار).</summary>
    private async Task SeedPriceHistoryAsync(CancellationToken ct)
    {
        if (await db.PriceHistories.AnyAsync(ct)) return;

        var products = await db.Products.ToListAsync(ct);
        var rnd = new Random(42);
        var rows = new List<PriceHistory>();

        foreach (var p in products)
        {
            var final = p.DiscountPercent > 0 ? p.UnitPrice - p.UnitPrice * p.DiscountPercent / 100 : p.UnitPrice;
            double trend = 1.18; // شروع گران‌تر، ریزش تدریجی تا قیمت فعلی

            for (var i = 6; i >= 1; i--)
            {
                var wobble = 1 + (rnd.NextDouble() - 0.5) * 0.04;
                var price = (long)Math.Round(final * (i == 1 ? 1.0 : trend * wobble) / 1000.0) * 1000;
                rows.Add(new PriceHistory
                {
                    ProductId = p.Id,
                    Price = price,
                    RecordedAt = DateTime.UtcNow.AddDays(-i * 15)
                });
                trend -= 0.03;
            }
        }

        db.PriceHistories.AddRange(rows);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>کارت انبار: یک ردیف «ورود کالا» برای موجودی اولیه هر محصول.</summary>
    private async Task SeedStockMovementsAsync(CancellationToken ct)
    {
        if (await db.StockMovements.AnyAsync(ct)) return;

        var products = await db.Products.ToListAsync(ct);
        var rows = products.Select(p => new StockMovement
        {
            ProductId = p.Id,
            Type = StockMovementType.Purchase,
            Delta = p.Stock,
            StockAfter = p.Stock,
            Reference = "موجودی اولیه",
            Note = "ثبت اولیه انبار هنگام راه‌اندازی فروشگاه",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        });

        db.StockMovements.AddRange(rows);
        await db.SaveChangesAsync(ct);
    }
}
