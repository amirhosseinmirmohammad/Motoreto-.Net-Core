# AGENTS.md — State پروژه مارکت (فروشگاه اینترنتی)

> ⚠️ **قانون برای هر ایجنت:** بعد از هر تغییر، این فایل را به‌روز کن (بخش «آخرین تغییرات» و «کارهای باقی‌مانده»).
> ⛔ **قانون ثابت پروژه: هیچ محتوای جنسی/مستهجن نباید در هیچ بخشی منتشر شود.** ورودی‌های عمومی (آگهی، چت، نظرات) باید از `Shop.Shared.ContentFilter.ViolationMessage()` عبور کنند؛ آگهی‌ها همیشه Pending→تأیید ادمین. متن حقوقی مرجع: `/terms`.
> آخرین به‌روزرسانی: 2026-10-30 (نسخه ۱۴)

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
1. **CRUD باقی‌مانده:**
   - GatewaysAdmin: ادیت درگاه موجود (merchant/secret/fee/sandbox) — الان فقط toggle/حذف/افزودن
   - MessagesAdmin: «علامت‌گذاری به‌عنوان نخوانده» (mark-read هست، برعکسش نیست)
   - CategoryAttributes: ادیت ویژگی (نام/فیلتر/واریانت/ترتیب)
   - SellerPropose: لیست «پیشنهادهای من» فروشنده — نیاز به ستون مالکیت روی Product (مایگریشن: ProposedBySellerId)
   - تب «اتصال فروشگاه» feed در پنل فروشنده (فاز ۲ فید) + webhook سفارش + CPC
2. **Sort بقیه لیست‌های ادمین** — انجام‌شده: Products، Orders، Users، Comments، BlogComments، Payments. باقی: Brands، Blog، Inventory، Messages، Discounts
3. **انتخاب واریانت در Blazor ProductList cards** (الان فقط صفحه جزئیات MVC).
4. **deploy** (docker-compose در ریشه هست — postgres+redis؛ سرویس app اضافه شود). Release build لایسنس ImageSharp می‌خواهد.
5. صفحه محصول: گالری/نظرات تکمیلی.
6. تست دستی جریان Checkout چندفروشنده‌ای (تعرفه‌های اختصاصی + فیلتر پرداخت).
7. git ✅ — ریپو از قبل بود؛ کامیت‌ها منظم گرفته شود.

## ۶.۵) آخرین تغییرات (نسخه ۱۴)
- **دسته‌های بیشتر دیوار ✅** — سرگرمی/اجتماعی/صنعتی/ابزار/مواد غذایی/اجاره خودرو (+ویژگی‌های هرکدام) در seed-classified.sql (اجرای مجدد idempotent)
- **محله/نقشه آگهی ✅** — `Neighborhood/Latitude/Longitude` روی ClassifiedAd (مایگریشن `AdNeighborhoodAndCoords`) + فیلدها در فرم ثبت + نمایش محله و لینک «📍 موقعیت روی نقشه» در صفحه آگهی

## ۶.۴) آخرین تغییرات (نسخه ۱۳)
- **Sort ادمین تکمیل ✅** — BrandsAdmin (نام/جدید) و MessagesAdmin (جدید/قدیم با اولویت نخوانده). همه لیست‌های ادمین حالا sort دارند (Inventory فیلتر نوع دارد — کافی)
- باقی دیوار: دسته‌های بیشتر، نقشه/محله، صفحه «دیوار من» یکپارچه

## ۶.۴) آخرین تغییرات (نسخه ۱۲)
- **واریانت در کارت‌های ProductList ✅** — کارت محصول: pillهای واریانت (مثل «قرمز / ۱۲۸ گیگ») با تغییر قیمت/موجودی زنده + دکمه «افزودن به سبد» با variantId (`/api/cart/add` با `window.shopAddToCart`). واریانت‌ها یک کوئری برای کل صفحه در `ProductList` لود و به ProductCard پاس می‌شوند (بدون N+1). card root از `<a>` به `<div>` تبدیل شد (تودرتویی تگ a حل شد)
- **CSRF meta ✅** — `<meta name="csrf-token">` در _Layout + fallback در `antiforgeryToken()` تا فرم‌های Blazor هم توکن داشته باشند
- توجه: کارت ProductCard حالا Interactive است (داخل صفحات rendermode InteractiveServer)

## ۶.۴) آخرین تغییرات (نسخه ۱۱)
- **فیلترهای پیشرفته `/ads` ✅** — قیمت از/تا (SQL) + **فیلتر ویژگی‌های داینامیک دسته** (انتخابی/متن/عدد — درون‌حافظه‌ای روی AttributesJson) با prefill از query string
- **Sort ادمین ✅** — BlogList (جدید/قدیم/پربازدید) و DiscountsAdmin (جدید/نزدیک انقضا/پراستفاده). باقی: Brands/Inventory/Messages

## ۶.۴) آخرین تغییرات (نسخه ۱۰)
- **آگهی‌های ذخیره‌شده ✅** — entity `ClassifiedFavorite` + دکمه ذخیره/حذف در صفحه آگهی + بخش «آگهی‌های ذخیره‌شده» در `/panel/ads` (مایگریشن `ClassifiedFavoritesAndSavedSearches`)
- **جستجوهای ذخیره‌شده با اعلان ✅** — entity `ClassifiedSavedSearch` (Q/CatId/CityId) + دکمه «ذخیره جستجو» در `/ads` (prefill از ?cat=&city=&q=) + در `/panel/ads` لیست با لینک بازآفرین فیلترها + **با تأیید آگهی منطبق در پنل ادمین، اعلان خودکار به همهٔ ذخیره‌کنندگان منطبق** (تا ۲۰۰ نفر)
- badge «⚡ فوری» در کارت‌های لیست آگهی

## ۶.۴) آخرین تغییرات (نسخه ۹)
- **گزارش تخلف آگهی ✅** — دکمه در صفحه آگهی → فرم دلیل → SiteMessages برای ادمین
- **ویرایش آگهی ✅** — `/panel/ads/edit/{id}` (فقط مالک): فرم داینامیک prefill + ویرایش → Pending مجدد؛ فیلتر محتوا روی ویرایش هم
- **تمدید آگهی ✅** — دکمه «تمدید ۳۰ روز» (نزدیک انقضا یا منقضی) → ExpiresAt+30، منقضی → Approved
- **SMS اعلان‌های مهم ✅** — `NotificationEvent.Sms` → رویدادهای سفارش/پرداخت/ارسال/لغو پیامک هم می‌روند (کاوه‌نگار، در NotificationConsumer)
- باگ: ViewCount حذف‌شده از ClassifiedAd برگشت؛ WalletDelta به متغیر محلی اصلاح شد

## ۶.۴) آخرین تغییرات (نسخه ۸)
- **سیستم اعلان کامل (Kafka + Outbox Pattern) ✅** — `Notification` + `OutboxMessage` entity (مایگریشن `NotificationsOutboxAndAdPricing`). جریان: رویداد در **همان تراکنش دیتابیس** در outbox ذخیره می‌شود (هیچ رویدادی با rollback/قطعی بروکر گم نمی‌شود) → `OutboxDispatcherJob` هر ۵ ثانیه به کافکا می‌فرستد (بدون کافکا: مستقیم اعلان می‌سازد؛ retry تا ۵۰ بار) → `NotificationConsumerJob` مصرف و درج می‌کند. **تریگرها:** ثبت سفارش (خریدار+فروشندگان)، تغییر وضعیت سفارش، تأیید پرداخت، تأیید/رد فروشگاه، تأیید/رد/فروخته‌شد آگهی، پیام چت (طرف مقابل)، شارژ/برداشت کیف پول، تسویه افیلیت. UI: زنگوله هدر با badge + dropdown (پول هر ۴۵ ثانیه) + `/panel/notifications` + API `api/notifications`
- **Hangfire ✅** — dashboard فقط ادمین `/hangfire`؛ job ساعتی `classified-expire-cleanup`: انقضای آگهی‌ها (→Expired)، برداشت خودکار نردبان/فوری بعد ۷ روز، حذف ردشده‌های ۹۰روزه. jobهای تکراری جدید باید Hangfire بروند نه BackgroundService
- **تعرفهٔ دیوار ✅** — هر دسته: `FreeAdLimit/AdPrice/BumpPrice/UrgentPrice/ComboPrice` (ادمین قابل تغییر). ثبت آگهی: سهمیه رایگان → بعدش از کیف پول (`ClassifiedAdPayment`). خرید نردبان/فوری/کمبو از `/panel/ads` با کسر کیف پول + تراکنش + اعلان. انقضای ۷ روزه با Hangfire
- **معماری:** مسیر انتخابی = مونولیت ماژولار با رویداد Kafka (آماده استخراج میکروسرویس: Notifications اولین کاندید) — نقشه راه پایین
- **اعلان بروزرسانی PWA ✅** — SW جدید → بنر «نسخه جدید منتشر شد» + دکمه بروزرسانی (skipWaiting کنترل‌شده) + رفرش خودکار

## ۶.۴) نقشه راه میکروسرویس (تصمیم معماری)
مونولیت ماژولار فعلی با مرزهای آماده استخراج: **Modules:** Identity / Catalog(محصول،برند،دسته،انبار) / Orders(سفارش،پرداخت،سبد،رزرو) / Marketplace(Seller,Offer,Feed) / Classifieds(آگهی،چت،تعرفه) / Notifications(کاملاً Kafka-based — اولین کاندید استخراج) / Shared(ContentFilter,PersianDate,Slug). قوانین: ماژول‌ها فقط با IEventPublisher/Kafka با هم حرف بزنند نه DbContext مستقیم بین‌ماژولی؛ استخراج هر ماژول = جدا کردن DbContext + هاست جدید + YARP gateway.

## ۶.۳) آخرین تغییرات (نسخه ۷)
- **چت آگهی‌ها ✅** — `/panel/ads/chat/{id}`: گفتگوی خریدار↔آگهی‌دهنده (فقط طرفین)، فیلتر محتوا روی ارسال، Enter برای ارسال. فرم پیام در صفحه آگهی برای کاربر واردشده + دکمه گفتگو برای مالک. لیست گفتگوها در `/panel/ads`
- **آگهی‌های من ✅** — `/panel/ads`: آگهی‌های کاربر (وضعیت/بازدید/فروخته‌شد/حذف با پاک‌سازی چت) + لینک در UserLayout
- **قوانین و مقررات ✅** — `/terms` بازنویسی حقوقی ۱۳ ماده (مسئولیت آگهی‌دهنده، ممنوعیت محتوای مستهجن — ماده ۶ پررنگ، تجارت الکترونیکی، صلاحیت دادگاه تهران) + لینک در هدر + چک‌باکس تعهد در فرم آگهی
- **فیلتر محتوای غیراخلاقی ✅** — `Shop.Shared.ContentFilter` (واژه‌های فارسی/لاتین + عبور از جداکننده‌ها) وصل‌شده به: فرم آگهی، چت، نظرات محصول، نظرات وبلاگ
- **گالری چندتصویری آگهی** — اسکیما آماده (`ImagesJson`)، UI باقی است

## ۶.۴) آخرین تغییرات (نسخه ۶)
- **سیستم آگهی دست دوم (مدل دیوار) ✅** — entity `Classified` (دسته/ویژگی داینامیک text-number-select-bool/آگهی با AttributesJson + Status Pending→Approved + انقضای ۳۰روزه + نردبان) + مایگریشن `ClassifiedAds` + `seed-classified.sql` (۹ دسته دیوار: املاک/وسایل نقلیه/موبایل/کامپیوتر/لوازم خانگی/استخدام/خدمات/استفاده‌ی نو/شخصی + ۳۰+ ویژگی اختصاصی هر دسته). صفحات: `/ads` (چیپ دسته + شهر + سرچ + sort) ، `/ads/{id}` (مشخصات جدولی + نمایش شماره تماس) ، `/ads/post` (فرم داینامیک بر اساس دسته + UploadDropzone) ، `/manage/classifieds` (مدیریش تأیید/رد/نردبان/حذف + CRUD دسته/ویژگی). لینک در هدر سایت و AdminLayout.
- **شمسی‌سازی پنل‌ها ✅** — همه `Fa()` های ادمین به `ToPersianDate/ToPersianDateTime` (ارقام فارسی + وقت ایران) وصل شد + UserComments/FeedsAdmin.
- **زمان‌بندی شگفت‌انگیز ✅** — `AmazingScheduleEnabled/StartMinutes/EndMinutes` در SiteSettings + UI تنظیمات (input time، پشتیبانی بازه شبانه) + `AmazingWindow.Resolve` (وقت ایران) در GetAmazingAsync + شمارش‌معکوس تا پایان بازه. خارج از بازه → بخش خالی. توجه: کش home-data-v1 تا انقضا ممکن است کهنه باشد.

## ۶.۴) آخرین تغییرات (نسخه ۵)
- **اتصال تنظیمات فروشنده به Checkout ✅** — کلاس `MarketplaceShipping` (Infrastructure/Services): ارسال چندفروشنده‌ای = هر فروشنده/مارکت یک مرسوله مستقل؛ پست با `ShippingCostOverride`/`FreeShippingThresholdOverride` هر فروشنده؛ اکسپرس = تعرفه مارکت × تعداد مرسوله. اعمال‌شده در `StockReservationService` (هر دو متد) و `OrderService.PlaceOrder` + **اعتبارسنجی PaymentMethods** (روش پرداخت باید از سوی همه فروشندگان سبد پذیرفته باشد). CheckoutController: `AcceptOnline/AcceptCod/ShipmentCount` به ViewBag → View: غیرفعال‌سازی روش پرداخت مجاز نیست + اعلان تعداد مرسوله‌ها + JS برآورد اکسپرس ضرب‌در مرسوله‌ها. نکته: برآورد پستِ سمت کلاینت هنوز تعرفه فروشنده را ندارد (سرور ملاک نهایی است).
- **git ✅** — ریپو از قبل موجود بود (agents.md قدیمی بود)؛ کامیت `d1926ee` با همه تغییرات نسخه ۳-۵ + خروج `Logs/` و `server*.log` از git.

## ۶.۴) آخرین تغییرات (نسخه ۴)
- **ممیزی کامل CRUD هر ۲۲ صفحه ادمین + پنل فروشنده + پنل کاربر انجام شد** (هدف: ادمین همه‌کاره، فروشنده/کاربر محدود به دامنه خود). موارد پیاده‌شده:
  - **پنل فروشنده:** ویرایش آفر (`/seller/offer?id=` — فرم prefill می‌شود، کالا قفل) + حذف آفر (سبد/رزروهای متصل پاک می‌شوند) — سرویس: `GetOfferForEditAsync`/`DeleteOfferAsync` + دکمه‌ها در جدول `/seller`
  - **UsersAdmin:** ساخت کاربر (UserManager) + ویرایش (نام/موبایل) + شارژ/برداشت کیف پول (WalletTransaction با نوع AdminCharge) + حذف (گارد: سفارش دارد→بلاک؛ ادمین→بلاک)
  - **CategoriesAdmin:** ویرایش (نام/والد/ترتیب/آیکون) + حذف (گارد فرزند/کالا؛ پاک‌سازی junctions + attributes)
  - **DiscountsAdmin:** ویرایش کامل (درصد/سقف مبلغ/حداقل سبد/سقف استفاده/انقضا) + حذف
  - **PaymentsAdmin:** از فقط-خواندنی به کامل — فیلتر وضعیت + جستجوی شماره سفارش + Pager + تغییر دستی وضعیت (CompletedAt ست می‌شود) + حذف
  - **SellersAdmin:** ویرایش کمیسیون بعد از تأیید + فعال/غیرفعال + حذف (گارد: آفر دارد→بلاک)
  - **FeedsAdmin:** ویرایش (نام/URL/فاصله sync) + حذف (کالاها قطع نسبت + غیرفعال)
  - **BlogList:** حذف مقاله (+کامنت‌هایش) | **ProductsList:** حذف (گارد: سفارش/دفتر انبار→بلاک؛ وگرنه پاک‌سازی کامل وابستگی‌ها)
  - **OrdersAdmin:** حذف سفارش (فقط AwaitingPayment/Canceled؛ payments+items) | حذف و «لغو تأیید» از هم جدا شدند در هر دو صفحه نظرات + دکمه لغو تأیید
  - **AffiliatesAdmin:** باگ «دکمه تسویه نبود» ✅ + ستون‌های جاافتاده پرداخت‌شده/موجودی + بج «درخواست تسویه»
  - **پنل کاربر:** صفحه جدید `/panel/comments` «نظرات من» — ویرایش (مجدداً در انتظار تأیید می‌شود) + حذف
- **Sort جدید:** OrdersAdmin (قدیمی/مبلغ) و UsersAdmin (نام/سفارش/کیف‌پول) + نظرات (امتیاز) — الگوی ProductsList

## ۶.۵-قدیم) آخرین تغییرات (نسخه ۳)
- **تنظیمات فروشگاه در پنل فروشنده ✅** — `/seller/settings`: تماس/معرفی (Description, Address, Phone, Email)، ارسال (ShippingTitle, DeliveryDays, DeliveryDaysMax)، هزینه ارسال و حد ارسال رایگان با سوییچ «پیش‌فرض مارکت» (null = پیش‌فرض)، روش‌های پرداخت (چک‌باکس online/cash/cod → csv)، Lat/Lng با لینک پیش‌نمایش گوگل‌مپ. + لینک در منوی `SellerLayout` و کارت «فروشگاه من» در `/seller`
- **FeedSyncJob ✅** — `Shop.Presentation/Services/FeedSyncJob.cs` (BackgroundService، الگوی ReservationCleanupJob): هر ۵ دقیقه فیدهای موعددار (IsActive + SyncIntervalMinutes) را با `FeedImportService` همگام می‌کند؛ در Program.cs ثبت شد
- **بخش فید (تروب) در نسخه قبلی انجام شده بود و این فایل قدیمی مانده بود:** entity `ShopFeed` + مایگریشن `ShopFeeds` + `Product.SourceFeedId` + `FeedImportService` (upsert با کلید feed+external_id، غیرفعال‌سازی غایب‌ها) + ادمین `/manage/feeds` (افزودن/سینک دستی/فعال-غیرفعال)
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

## ۶.۸) افیلیت — سه مدل پرداخت (کامل شد)
- **CPS (فروش):** کوکی ۳۰ روزه → تسویه → `Earned += Total × Commission%` — ضدتقلب: خرید خودِ همکار کمیسیون ندارد
- **CPA (ثبت‌نام):** کوکی در Register خوانده می‌شود → `RegistrationCount++` + پاداش ۲۰٬۰۰۰ تومان (`AccountController.Register` — ثابت `cpaBonus`)
- **CPL (سرنخ):** لندینگ عمومی `/aff/lead/{code}` (فرم نام+موبایل، بدون لاگین) → `affiliate_leads` + `LeadCount++` + ۱۵٬۰۰۰ تومان — endpoint های GET/POST در Program.cs
- **تسویه:** کاربر در `/panel` موجودی + «درخواست تسویه» (حد نصاب ۵۰۰٬۰۰۰ تومان → `PayoutRequestedAt`)؛ ادمین در `/manage/affiliates`: جدول کامل (کلیک/CPA/CPL/CPS/کسب‌شده/پرداخت‌شده/موجودی) + دکمه «تسویه شد» (PaidOut=Earned)
- **سود مارکت:** کمیسیون همکار (پیش‌فرض ۳٪، ادمین ۱-۳۰٪) از کمیسیون ۵٪ فروشنده کسر می‌شود؛ self-referral بلاک
- **تبلیغ در لندینگ CPL:** فرم مستقل HTML (بدون لاگین) برای کانال‌ها/بلاگرها

## ۶.۸-فید) اتصال فروشگاه‌ها به سبک تروب (Feed-based) — فاز ۱ (بک‌اند + ادمین + sync) انجام شد
مدل: فروشگاه‌های بیرونی به‌جای ثبت دستی محصول، **feed URL** می‌دهند (JSON/XML با title,price,stock,image,category,external_id) → ما import/sync دوره‌ای می‌کنیم → کالاهایشان با برچسب منبع در مارکت لیست می‌شود → سفارش‌ها به فروشگاه اطلاع داده می‌شود (webhook/ایمیل) → درآمد ما: کمیسیون فروش یا CPC.
- **Entity جدید `ShopFeed`:** Id, SellerId? (یا ShopName+Email برای فروشگاه بدون حساب), FeedUrl, Token (برای webhook بازگشت), SyncIntervalMinutes, Status(Active/Blocked), LastSyncAt, LastSyncError, ProductCount
- **ستون روی Product:** `source_feed_id` (null = کالای مارکت — در فرم ادمین قفل شود)
- **ImportService:** HttpClient → دانلود feed → پارس JSON (قرارداد: `{"products":[{external_id,title,price_toman,stock,image_url,category,description}]}`) → upsert با کلید (feed_id, external_id) → قیمت/موجودی آپدیت، حذف غایب‌ها (IsActive=false)
- **Sync:** دکمه «همگام‌سازی» در ادمین + BackgroundJob دوره‌ای (الگوی ReservationCleanupJob)
- **ورک‌فلو:** فروشگاه در `/seller/register` ثبت‌نام می‌کند → در پنلش تب «اتصال فروشگاه» feed URL می‌دهد → ادمین در `/manage/feeds` تأیید می‌کند → sync
- **بعداً:** webhook سفارش + مدل CPC (شمارش کلیک روی کالاهای feed)
- ✅ **انجام‌شده:** entity `ShopFeed` + مایگریشن `ShopFeeds` + `Product.SourceFeedId` + `FeedImportService` (`Shop.Presentation/Services`) + ادمین `/manage/feeds` + `FeedSyncJob` دوره‌ای (هر ۵ دقیقه)
- ⬜ **باقی‌مانده:** تب «اتصال فروشگاه» در پنل فروشنده (فروشنده خودش feed URL بدهد و ادمین فقط تأیید کند) + webhook اطلاع سفارش + مدل CPC (شمارش کلیک روی کالاهای feed)

## ۶.۷) کارهای باقی‌مانده (قدیمی — جزئیات بیشتر)
1. **فیلتر /products با junction دسته‌ها** — الان فقط `p.CategoryId`؛ باید `ids.Contains(p.CategoryId) || p.ProductCategories/جunction.Any(...)` شود (ProductService خط ~162).
2. **سیستم افیلیت** — درخواست شده، شروع نشده: entity `Affiliate`(UserId,Code,Commission,Balance,Earned,Clicks)، ستون `orders.affiliate_code`، endpoint `/aff/{code}` + کوکی ۳۰ روزه، اتریبیوشن هنگام ثبت سفارش، پنل ادمین + تب در پنل کاربر.
3. **Sort پیشرفته لیست‌ها** — فقط محصولات sort ندارد؛ dropdown (جدیدترین/ارزان/گران/پرفروش) + تعمیم به بقیه لیست‌ها. Pager در ۱۰ لیست هست.
4. **روش پرداخت/ارسال هر فروشنده** — ✅ انجام شد (نسخه ۳): ستون‌ها مایگریت شده‌اند + UI ادیت در `/seller/settings` (تماس/ارسال/هزینه‌ها/پرداخت/Lat-Lng).
5. **حذف /admin قدیمی** (MVC) بعد از اطمینان.
6. **git init + commit + deploy** — هنوز انجام نشده!
7. صفحه محصول: گالری/نظرات تکمیلی، مقایسه: sort/search خود ویو.

## ۷) مایگریشن‌ها (به ترتیب)
`Initial` → `MarketplaceAndWarehouse` → `LoyaltyAndOtp` → `CorsAndLoyaltyFields` → `AddBrandsAndCategoryAttributes` → `MultiCategoryAndSellerCategories` → `BrandCategoryJunction` → `ProductSeoFields` → `SellerStoreInfoAndAffiliate` → `AffiliateCpaCpl` → `ShopFeeds` → `SellerStoreFields` — همه اعمال شده‌اند؛ مایگریشن معوقی وجود ندارد.

## ۸) نکته‌های توسعه
- کامپوننت‌های Blazor: `@inject ApplicationDbContext Db` مستقیم ok (sequential).
- **اتریبیوت‌های DataAnnotations در بلاک `@code` فایل‌های razor باید با نام کامل نوشته شوند** (`[System.ComponentModel.DataAnnotations.StringLength(80)]` و `Range`) — چون `Range` با `System.Range` تداخل می‌گیرد و using هم در دسترس نیست (الگوی Seller.cs).
- `[Inject]` property در .razor مشکل‌ساز بوده → همیشه از دایرکتیو `@inject` استفاده کن.
- در مارکاپ Razor هرگز `?? ''` ننویس (empty char literal) → متد هندلر بساز.
- `InputNumber` از byte پشتیبانی نمی‌کند → input معمولی + TryParse.
- Content-Type آپلودها: فقط jpg/png/webp/gif. تصاویر دمو: `/images/products/real/1..60.webp` (۱:۱ با id محصول).
- اگر صفحه‌ای 500 داد: اول `Logs/app-*.json` یا خروجی سرور را ببین؛ خطاهای رایج: قلم نال در DTO/پروجکشن.
