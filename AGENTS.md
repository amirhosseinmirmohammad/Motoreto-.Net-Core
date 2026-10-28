# AGENTS.md — State پروژه مارکت (فروشگاه اینترنتی)

> ⚠️ **قانون برای هر ایجنت:** بعد از هر تغییر، این فایل را به‌روز کن (بخش «آخرین تغییرات» و «کارهای باقی‌مانده»).
> آخرین به‌روزرسانی: 2026-10-28 (نسخه ۲)

---

## ۱) پروژه چیست؟
فروشگاه اینترنتی کامل فارسی (مارکت) — .NET 10 + PostgreSQL + Redis، مارکت‌پلیس با فروشندگان، واریانت، سئو، پنل‌های Blazor SPA.
مسیر: `D:\Projects\Motoreto` | Solution: `Shop.slnx` | معماری: ۵ لایه (Domain/Application/Infrastructure/Shared/Presentation)

## ۲) اجرا و بیلد
```bash
dotnet build Shop.slnx
# اجرا (محیط ابزار ایجنت پراسس‌ها را می‌کُشد! برای تست دائمی از bat استفاده کن):
D:\Projects\Motoreto\start-server.bat   → http://localhost:5200
D:\Projects\Motoreto\stop-server.bat
```
- **کانتینرها (podman):** `shop-pg` (PostgreSQL 16، پورت 56432، user `shop_user`، db `shop`، pass `Shop!2026`)، `shop-redis` (56379)، `shop-seaweed` (S3 در 8333، master در 9333)
- **لاگ سرور:** `server.log` در ریشه پروژه + `Logs/app-*.json`
- **ورودها:** ادمین `admin@shop.ir / Admin@1234` — کاربر `user@shop.ir / User@1234`
- ⚠️ قبل از بیلد، سرور را بکش (`Stop-Process Shop.Presentation`) وگرنه lock می‌شود. گاهی شِل‌های ابزار هم code 255 می‌میرند — دوباره تلاش کن، خطای کد نیست.

## ۳) قراردادهای مهم
- **پول = تومان در کل سیستم** (ذخیره‌سازی و نمایش). `ToToman()` دیگر تقسیم نمی‌کند. JSON-LD قیمت را ×۱۰ با `IRR` می‌دهد (استاندارد اسکیما).
- **اعداد فارسی:** `farsi-digits.js?v=2` سراسری (observer کل body را دوباره پیمایش می‌کند — نسخه قدیمی باگ batch-drop داشت). ورودی‌ها/input ها دست نمی‌خورند.
- **کش استاتیک:** هر تغییر css/js → bump نسخه در `App.razor` (الان: site.css v13، site.js v9، farsi v2).
- **AttributesJson محصول:** `[{"name":"...","values":["..."]}]` — همیشه با PropertyNameCaseInsensitive.
- **کوئری‌های Blazor ترتیبی** (یک DbContext اسکوپ‌شده در هر circuit — موازی ممنوع).
- **ریسپانسیو/RTL:** Bootstrap 5 RTL. فرم‌ها: `SearchableSelect`/`MultiSelect`/`UploadDropzone`/`RichEditor`(Quill)/`Pager` در `Components/Shared`.

## ۴) معماری پنل‌ها (همه Blazor SPA — بدون رفرش)
- **ادمین:** `/manage/...` با `AdminLayout` — ۲۰+ صفحه: داشبورد، محصولات(لیست/فرم+واریانت+سئو)، برندها، دسته‌ها، ویژگی‌های دسته، انبار، تخفیف‌ها، سفارش‌ها، پرداخت‌ها، فروشندگان، کاربران، نظرات، نظرات وبلاگ، وبلاگ(لیست/فرم)، اسلایدرها، پیام‌ها، تنظیمات
- **کاربر:** `/panel`، `/panel/profile`، `/panel/password` با `UserLayout`
- **فروشنده:** `/seller`، `/seller/register`، `/seller/offer`، `/seller/new-product` با `SellerLayout`
- `/admin` (MVC قدیمی) هنوز هست ولی لینک ندارد — قابل حذف
- **ریدایرکت بعد از ورود:** ادمین→`/manage`، فروشنده تأییدشده→`/seller`، کاربر→`/panel` (در `AccountController.Login/Register`)

## ۵) فیچرهای کلیدی پیاده‌شده
- **واریانت/SKU:** ویژگی‌های `IsVariantOption` دسته → فرم محصول «ساخت ترکیب‌ها» (ضرب دکارتی) → `product_variants` با قیمت/موجودی مستقل؛ موجودی کل = جمع. صفحه محصول: انتخابگر pill با قیمت/موجودی زنده → سبد با `variantId` (بک‌اند کامل: سبد/رزرو/سفارش).
- **دسته چندگانه محصول:** junction `product_categories` (فیلتر /products هنوز فقط CategoryId را می‌بیند — **باقی‌مانده**).
- **برند↔دسته (n:n):** junction `brand_categories` + صفحه مدیریت + فیلتر برند بر اساس دسته‌های انتخابی در فرم‌ها (قانون: اگر نگاشتی نیست، همه برندها).
- **دسته‌های فعالیت فروشنده:** junction `seller_categories` — فروشنده فقط کالاهای دسته‌های خودش را می‌بیند/آفر می‌زند.
- **آپلود:** `UploadDropzone` (drag&drop + progress واقعی + حذف/تغییر) → تبدیل WebP (ImageSharp 4.1.3) → **SeaweedFS/S3** (Minio SDK، bucket `shop`، config در appsettings `ObjectStorage`).
- **اسلایدر:** بنرهای تولیدی ۱۶۰۰×۶۰۰ (`BannerSeeder`، در `wwwroot/images/banners`) + overlay متنی + مدیریت `/manage/sliders` (با تخلیه کش `home-data-v1`).
- **تومان/ریال:** داده‌ها یک‌بار ÷۱۰ شدند (مهاجرت کامل).
- **SEO/AEO/GEO:** فیلدهای MetaTitle/MetaDescription روی Product (+فرم ادمین)، `@section Head` در Details: title/desc/canonical/OG/Twitter/JSON-LD Product schema (قیمت ×۱۰ IRR، aggregateRating). اسلاگ‌های seed لاتین.
- **Seed کاتالوگ:** `seed-catalog.sql` (در ریشه) — ۶۰ محصول یکتا با نام/اسلاگ/برند/قیمت تومانی/ویژگی/واریانت(۱۵۴ SKU)/چنددسته(۱۸)/شگفت‌انگیز(۶). اجرای مجدد idempotent تقریبی (آپدیت‌ها).

## ۶) کارهای باقی‌مانده (به ترتیب اولویت)
1. **فروشنده:** ادیت اطلاعات/ارسال/پرداخت فروشگاه (فیلدهای `ShippingTitle,DeliveryDays,ShippingCostOverride,FreeShippingThresholdOverride,PaymentMethods,Latitude,Longitude` آماده‌اند — UI ادیت در پنل فروشنده بساز) — مایگری ستون‌ها اعمال شد.
2. **Sort پیشرفته بقیه لیست‌ها** (الگو: ProductsList — sort select + OrderBy شرطی).
3. **انتخاب واریانت در Blazor ProductList cards** (الان فقط صفحه جزئیات MVC).
4. **حذف /admin قدیمی** (MVC) بعد از اطمینان.
5. **deploy** (docker-compose در ریشه هست — postgres+redis؛ سرویس app اضافه شود).
6. صفحه محصول: گالری/نظرات تکمیلی.

## ۶.۵) آخرین تغییرات (نسخه ۲)
- **افیلیت کامل:** entity `Affiliate` (+ مایگریشن `SellerStoreInfoAndAffiliate`)، endpoint `/aff/{code}` (کلیک+کوکی ۳۰روزه) در Program.cs، اتریبیوشن در CheckoutController (کمیسیون% → Earned)، تب افیلیت در `/panel` (ساخت کد + لینک + آمار)، ادمین `/manage/affiliates` (کمیسیون/فعال-غیرفعال)
- **Seller:** فیلدهای `PaymentMethods` (csv: online,cash,cod)، `Latitude`، `Longitude` اضافه شد (مایگریشن)
- **فیلتر /products با junction دسته‌ها** ✅ — محصول چنددسته‌ای در همه دسته‌هایش دیده می‌شود
- **Sort لیست محصولات ادمین** ✅ (جدید/ارزان/گران/موجودی)
- **تومان‌سازی کامل** ✅ — دیتابیس ÷۱۰، `ToToman` بدون تقسیم، لیبل‌ها
- **Seed کاتالوگ** ✅ (`seed-catalog.sql` قابل اجرای مجدد) — ۶۰ محصول یکتا با اسلاگ لاتین سئو، برند، ویژگی فیلترپذیر، **۱۵۴ واریانت** (موبایل: حافظه×رنگ، لپ‌تاپ: رم×حافظه، پوشاک/کفش: سایز×رنگ، عطر: حجم)، ۱۸ محصول چنددسته‌ای، ۶ شگفت‌انگیز، متای سئو برای همه
- **SEO/AEO/GEO صفحه محصول** ✅ — `@section Head` در Details: title/desc/canonical/OG/Twitter/JSON-LD Product schema (قیمت ×۱۰ IRR، aggregateRating)؛ متا از `Product.MetaTitle/MetaDescription` (فیلدهای جدید + فرم ادمین بخش «سئو»)
- **AGENTS.md** ساخته شد ← همین فایل

## ۶.۶) باگ‌های حل‌شده اخیر (اگر برگشتند اینجا را ببین)
- `farsi-digits.js` observer فقط دسته اول mutation را می‌گرفت → نسخه v2 کل body را rewalk می‌کند
- دکمه «حذف تصویر» زیر input نامرئی گیر می‌کرد → حالت پیش‌نمایش دکمه‌های واقعی دارد
- Quill/SEO: `@section Head` به `_Layout` اضافه شد؛ Quill از CDN (آفلاین → ادیتور متنی جایگزین)
- سرور از داخل ابزار ایجنت اجرا شود بعد از مدتی کشته می‌شود → همیشه `start-server.bat` توسط کاربر اجرا شود

## ۶.۷) کارهای باقی‌مانده (قدیمی — جزئیات بیشتر)
1. **فیلتر /products با junction دسته‌ها** — الان فقط `p.CategoryId`؛ باید `ids.Contains(p.CategoryId) || p.ProductCategories/جunction.Any(...)` شود (ProductService خط ~162).
2. **سیستم افیلیت** — درخواست شده، شروع نشده: entity `Affiliate`(UserId,Code,Commission,Balance,Earned,Clicks)، ستون `orders.affiliate_code`، endpoint `/aff/{code}` + کوکی ۳۰ روزه، اتریبیوشن هنگام ثبت سفارش، پنل ادمین + تب در پنل کاربر.
3. **Sort پیشرفته لیست‌ها** — فقط محصولات sort ندارد؛ dropdown (جدیدترین/ارزان/گران/پرفروش) + تعمیم به بقیه لیست‌ها. Pager در ۱۰ لیست هست.
4. **روش پرداخت/ارسال هر فروشنده** — Seller فیلدها هست (`ShippingTitle,DeliveryDays,ShippingCostOverride,FreeShippingThresholdOverride,TaxPercent,ServiceFee,PaymentMethods,Latitude,Longitude` — PaymentMethods/Lat/Lng تازه اضافه شد، ستون‌ها در مایگریشن بعدی باید بیایند!) + UI ادیت در پنل فروشنده.
5. **حذف /admin قدیمی** (MVC) بعد از اطمینان.
6. **git init + commit + deploy** — هنوز انجام نشده!
7. صفحه محصول: گالری/نظرات تکمیلی، مقایسه: sort/search خود ویو.

## ۷) مایگریشن‌ها (به ترتیب)
`AddBrandsAndCategoryAttributes` → `MultiCategoryAndSellerCategories` → `ProductSeoFields` → (بعدی: ستون‌های PaymentMethods/Latitude/Longitude روی sellers + جدول affiliates)

## ۸) نکته‌های توسعه
- کامپوننت‌های Blazor: `@inject ApplicationDbContext Db` مستقیم ok (sequential).
- `[Inject]` property در .razor مشکل‌ساز بوده → همیشه از دایرکتیو `@inject` استفاده کن.
- در مارکاپ Razor هرگز `?? ''` ننویس (empty char literal) → متد هندلر بساز.
- `InputNumber` از byte پشتیبانی نمی‌کند → input معمولی + TryParse.
- Content-Type آپلودها: فقط jpg/png/webp/gif. تصاویر دمو: `/images/products/real/1..60.webp` (۱:۱ با id محصول).
- اگر صفحه‌ای 500 داد: اول `Logs/app-*.json` یا خروجی سرور را ببین؛ خطاهای رایج: قلم نال در DTO/پروجکشن.
