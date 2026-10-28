-- ================= بازسازی کامل کاتالوگ ۶۰ محصول (تومان، سئو، واریانت) =================
BEGIN;

-- ۱) به‌روزرسانی محصولات: نام یکتا، اسلاگ لاتین، برند، قیمت تومانی، متا
UPDATE products p SET
  name = v.name,
  slug = v.slug,
  brand_id = v.brand,
  unit_price = v.price,
  discount_percent = v.disc,
  is_amazing = v.amz,
  is_special = v.sp,
  meta_title = v.name || ' | خرید آنلاین با بهترین قیمت | مارکت',
  meta_description = 'خرید ' || v.name || ' اصل و اورجینال با گارانتی معتبر از مارکت؛ بهترین قیمت روز، ارسال سریع و پرداخت امن.'
FROM (VALUES
  (1,  'گوشی موبایل سامسونگ Galaxy A55 ظرفیت ۲۵۶ گیگابایت', 'samsung-galaxy-a55-256gb', 2, 24900000::bigint, 8, true,  false),
  (2,  'گوشی موبایل شیائومی Redmi Note 13 Pro ظرفیت ۲۵۶ گیگابایت', 'xiaomi-redmi-note-13-pro-256gb', 3, 18700000, 12, true, false),
  (3,  'گوشی موبایل اپل iPhone 15 ظرفیت ۱۲۸ گیگابایت', 'apple-iphone-15-128gb', 1, 58900000, 0, false, true),
  (4,  'گوشی موبایل آنر X9b ظرفیت ۲۵۶ گیگابایت', 'honor-x9b-256gb', 9, 15900000, 6, false, false),
  (5,  'گوشی موبایل شیائومی Poco X6 Pro ظرفیت ۵۱۲ گیگابایت', 'xiaomi-poco-x6-pro-512gb', 3, 26400000, 0, false, true),
  (6,  'گوشی موبایل سامسونگ Galaxy S23 FE ظرفیت ۲۵۶ گیگابایت', 'samsung-galaxy-s23-fe-256gb', 2, 41200000, 5, true, false),
  (7,  'هدفون بی‌سیم سونی مدل WH-CH720', 'sony-wh-ch720-wireless-headphone', 5, 3450000, 0, false, false),
  (8,  'هندزفری بلوتوث نوکیا مدل E3511', 'nokia-e3511-bluetooth-earphone', 8, 890000, 15, true, false),
  (9,  'هدفون گیمینگ هایسنس با میکروفون', 'hisense-gaming-headset-mic', 10, 1290000, 0, false, false),
  (10, 'هندزفری سیمی اپل EarPods', 'apple-earpods-wired', 1, 720000, 0, false, false),
  (11, 'پاوربانک اپل ۲۰ هزار میلی‌آمپر سریع‌شارژ', 'apple-powerbank-20000mah', 1, 2350000, 0, false, false),
  (12, 'پاوربانک لنوو ۱۰ هزار میلی‌آمپر مدل PB500', 'lenovo-pb500-powerbank-10000mah', 7, 1180000, 10, false, false),
  (13, 'قاب محافظ گوشی سامسونگ S23 سری شفاف', 'samsung-s23-clear-case', 9, 245000, 0, false, false),
  (14, 'گلس محافظ صفحهنمایش سرامیک مناسب گوشی شیائومی', 'xiaomi-ceramic-screen-protector', 9, 165000, 20, false, false),
  (15, 'لپ‌تاپ ۱۵ اینچی سامسونگ Galaxy Book3 Core i5', 'samsung-galaxy-book3-i5', 2, 41500000, 0, false, true),
  (16, 'لپ‌تاپ لنوو IdeaPad Slim 3 Ryzen 5', 'lenovo-ideapad-slim3-ryzen5', 7, 28700000, 7, false, false),
  (17, 'لپ‌تاپ اپل MacBook Air M2 ۱۳ اینچ', 'apple-macbook-air-m2-13', 1, 78900000, 0, false, true),
  (18, 'لپ‌تاپ گیمینگ ایسوس TUF F15 RTX 4050', 'asus-tuf-f15-rtx4050', 6, 62300000, 0, false, false),
  (19, 'کیبورد مکانیکی گیمینگ ایسوس ROG Strix', 'asus-rog-strix-mechanical-keyboard', 6, 4290000, 0, false, false),
  (20, 'ماوس بی‌سیم لنوو ۲۴۰۰DPI مدل ۵۳۰', 'lenovo-530-wireless-mouse', 7, 690000, 0, false, false),
  (21, 'پکیج کیبورد و ماوس شیائومی بی‌سیم', 'xiaomi-wireless-keyboard-mouse-combo', 3, 950000, 12, false, false),
  (22, 'مانیتور ۲۷ اینچی نوکیا UltraSlim IPS', 'nokia-ultraslim-27-ips-monitor', 8, 15900000, 0, false, false),
  (23, 'مانیتور حرفه‌ای اپل Studio Display ۲۷ اینچ', 'apple-studio-display-27', 1, 92500000, 0, false, false),
  (24, 'هارد SSD اکسترنال ال‌جی ۱ ترابایت', 'lg-external-ssd-1tb', 4, 3890000, 0, false, false),
  (25, 'هارد SSD شیائومی ۵۱۲ گیگابایت پرتابل', 'xiaomi-portable-ssd-512gb', 3, 2190000, 8, false, false),
  (26, 'سرخ‌کن توستر ال‌جی چهار اسلایسی', 'lg-4-slice-toaster', 4, 3290000, 0, false, false),
  (27, 'چای‌ساز اپل مدل AC-2200', 'apple-ac2200-tea-maker', 1, 4590000, 10, false, false),
  (28, 'غذاساز خانگی نوکیا چندکاره', 'nokia-multifunction-food-processor', 8, 6890000, 0, false, false),
  (29, 'مخلوط‌کن حرفه‌ای اپل ۱۰۰۰ وات', 'apple-1000w-professional-blender', 1, 5290000, 5, false, false),
  (30, 'جاروبرقی صنعتی هایسنس ۲۲۰۰ وات', 'hisense-2200w-vacuum-cleaner', 10, 8790000, 0, false, false),
  (31, 'جاروبرقی رباتیک اپل مدل Clean-S1', 'apple-clean-s1-robot-vacuum', 1, 23900000, 0, false, true),
  (32, 'آبمیوه‌گیری ایسوس پرقدرت ۸۰۰ وات', 'asus-800w-juicer', 6, 4790000, 0, false, false),
  (33, 'مخلوط‌کن دستی سونی مدل MixPro', 'sony-mixpro-hand-blender', 5, 2190000, 15, true, false),
  (34, 'اتو بخار اپل مدل Steam-350', 'apple-steam-350-iron', 1, 2890000, 0, false, false),
  (35, 'اتو پرسی سونی خانگی', 'sony-home-steam-press', 5, 8930000, 0, false, false),
  (36, 'پیراهن مردانه آستین بلند طرح خال‌خالی', 'mens-long-sleeve-polka-shirt', NULL, 890000, 0, false, false),
  (37, 'شلوار جین مردانه مدل کلاسیک آبی', 'mens-classic-blue-jeans', 9, 1250000, 0, false, false),
  (38, 'مانتو زنانه جلو باز طرح ساده', 'womens-open-front-mantle', 1, 1690000, 0, false, false),
  (39, 'شومیز زنانه یقه هفت طرح‌دار', 'womens-vneck-patterned-blouse', 1, 890000, 25, true, false),
  (40, 'کفش مردانه اسپرت روزمره', 'mens-casual-sport-shoes', 7, 2190000, 0, false, false),
  (41, 'کفش زنانه مجلسی پاشنه کوتاه', 'womens-low-heel-party-shoes', 7, 1890000, 0, false, false),
  (42, 'ساعت هوشمند هایسنس مدل Watch-S2', 'hisense-watch-s2-smartwatch', 10, 4590000, 10, false, true),
  (43, 'ساعت هوشمند ورزشی مدل Fit-PRO', 'fit-pro-sport-smartwatch', 10, 3290000, 0, false, false),
  (44, 'عطر مردانه ادکلن حجم ۱۰۰ میل', 'mens-perfume-100ml', 4, 2790000, 0, false, false),
  (45, 'ادکلن زنانه سونی مدل Rose ۹۰ میل', 'sony-rose-90ml-perfume', 5, 3290000, 0, false, false),
  (46, 'کرم مرطوب‌کننده و آبرسان پوست', 'skin-moisturizing-cream', 1, 690000, 0, false, false),
  (47, 'ضد آفتاب SPF50 مناسب پوست چرب', 'spf50-oily-skin-sunscreen', 7, 820000, 18, false, false),
  (48, 'پالت رژ لب مات ۱۲ رنگ', 'matte-lipstick-palette-12', 4, 540000, 0, false, false),
  (49, 'چادر مسافرتی ۴ نفره ضد آب', '4-person-waterproof-tent', 5, 5690000, 0, false, false),
  (50, 'کیسه خواب کوهنوردی حرارتی', 'thermal-sleeping-bag', 2, 2390000, 0, false, false),
  (51, 'دوچرخه کوهستان هایسنس ۲۷.۵ اینچ', 'hisense-275-mountain-bike', 10, 12900000, 0, false, false),
  (52, 'دمبل روکش‌دار ۱۰ کیلوگرمی جفت', 'pair-10kg-rubber-dumbbell', 6, 1790000, 0, false, false),
  (53, 'کتاب چاپی رمان诸葛 کامل ۵ جلدی', 'complete-novel-set-5-vol', 3, 890000, 0, false, false),
  (54, 'کتاب آشپزی تصویری قدم به قدم', 'step-by-step-cookbook', 5, 450000, 0, false, false),
  (55, 'ست نوشت‌افزار اداری هایسنس ۳۲ پارچه', 'hisense-32pc-office-stationery-set', 10, 390000, 0, false, false),
  (56, 'کوله پشتی لپ‌تاپ ضد آب ایسوس', 'asus-waterproof-laptop-backpack', 6, 1290000, 0, false, false),
  (57, 'عروسک پولیشی خرس تدی ۸۰ سانتی', 'teddy-bear-plush-80cm', 8, 980000, 0, false, false),
  (58, 'بازی فکری مونوپلی مدل کلاسیک', 'classic-monopoly-board-game', 1, 690000, 0, false, false),
  (59, 'دومینو حرفه‌ای چوبی در جعبه چوبی', 'professional-wooden-domino-set', 9, 450000, 0, false, false),
  (60, 'ست سیسمونی نوزاد ۹ پارچه', '9pc-newborn-baby-set', 10, 2390000, 0, false, false)
) AS v(id, name, slug, brand, price, disc, amz, sp)
WHERE p.id = v.id;

-- ۲) ویژگی‌های فیلترپذیر (کلیدها باید با category_attributes یکی باشند)
UPDATE products SET attributes_json = '[{"name":"حافظه داخلی","values":["۱۲۸ گیگ","۲۵۶ گیگ"]},{"name":"شبکه ارتباطی","values":["5G","4G"]},{"name":"رنگ","values":["مشکی","آبی","سفید"]}]'::text WHERE id BETWEEN 1 AND 6;
UPDATE products SET attributes_json = '[{"name":"نوع اتصال","values":["بلوتوث","سیمی"]},{"name":"گارانتی","values":["۱۸ ماه"]}]'::text WHERE id IN (7,8,9,10);
UPDATE products SET attributes_json = '[{"name":"ظرفیت","values":["۱۰ هزار","۲۰ هزار"]},{"name":"گارانتی","values":["۱۲ ماه"]}]'::text WHERE id IN (11,12);
UPDATE products SET attributes_json = '[{"name":"جنس","values":["سیلیکون","چرم"]},{"name":"رنگ","values":["مشکی","شفاف"]}]'::text WHERE id IN (13,14);
UPDATE products SET attributes_json = '[{"name":"رم","values":["۸ گیگ","۱۶ گیگ","۳۲ گیگ"]},{"name":"حافظه","values":["۵۱۲ SSD","۱ ترابایت"]}]'::text WHERE id IN (15,16,17,18);
UPDATE products SET attributes_json = '[{"name":"نوع اتصال","values":["بی‌سیم","سیمی"]},{"name":"گارانتی","values":["۱۲ ماه"]}]'::text WHERE id IN (19,20,21);
UPDATE products SET attributes_json = '[{"name":"اندازه","values":["۲۴ اینچ","۲۷ اینچ"]},{"name":"گارانتی","values":["۲۴ ماه"]}]'::text WHERE id IN (22,23);
UPDATE products SET attributes_json = '[{"name":"حافظه","values":["۵۱۲ گیگ","۱ ترابایت"]}]'::text WHERE id IN (24,25);
UPDATE products SET attributes_json = '[{"name":"توان","values":["۸۰۰ وات","۱۰۰۰ وات"]},{"name":"گارانتی","values":["۱۸ ماه"]}]'::text WHERE id IN (26,27,28,29,32,33);
UPDATE products SET attributes_json = '[{"name":"توان","values":["۲۰۰۰ وات"]},{"name":"گارانتی","values":["۱۸ ماه"]}]'::text WHERE id IN (30,31);
UPDATE products SET attributes_json = '[{"name":"توان","values":["۱۴۰۰ وات"]},{"name":"گارانتی","values":["۱۲ ماه"]}]'::text WHERE id IN (34,35);
UPDATE products SET attributes_json = '[{"name":"جنس","values":["نخ پنبه","پلی‌استر"]},{"name":"سایز","values":["S","M","L","XL"]},{"name":"رنگ","values":["مشکی","سفید","سرمه‌ای"]}]'::text WHERE id IN (36,37,38,39);
UPDATE products SET attributes_json = '[{"name":"سایز","values":["۴۰","۴۱","۴۲","۴۳"]},{"name":"رنگ","values":["مشکی","قهوه‌ای"]}]'::text WHERE id IN (40,41);
UPDATE products SET attributes_json = '[{"name":"جنس بند","values":["سیلیکون","چرم"]}]'::text WHERE id IN (42,43);
UPDATE products SET attributes_json = '[{"name":"حجم","values":["۵۰ میل","۱۰۰ میل"]},{"name":"نوع","values":["ادکلن","او دو توالت"]}]'::text WHERE id IN (44,45);
UPDATE products SET attributes_json = '[{"name":"نوع پوست","values":["خشک","چرب","معمولی"]},{"name":"حجم","values":["۵۰ میل"]}]'::text WHERE id IN (46,47);
UPDATE products SET attributes_json = '[{"name":"رنگ","values":["قرمز","صورتی"]}]'::text WHERE id = 48;
UPDATE products SET attributes_json = '[{"name":"ظرفیت","values":["۲ نفر","۴ نفر"]},{"name":"گارانتی","values":["۱۲ ماه"]}]'::text WHERE id IN (49,50);
UPDATE products SET attributes_json = '[{"name":"اندازه","values":["۲۶ اینچ","۲۷.۵ اینچ"]}]'::text WHERE id = 51;
UPDATE products SET attributes_json = '[{"name":"وزن","values":["۵ کیلوگرم","۱۰ کیلوگرم"]}]'::text WHERE id = 52;
UPDATE products SET attributes_json = '[{"name":"نوع جلد","values":["گالینگور","شومیز"]}]'::text WHERE id IN (53,54);
UPDATE products SET attributes_json = '[{"name":"تعداد پارچه","values":["۳۲ پارچه"]}]'::text WHERE id = 55;
UPDATE products SET attributes_json = '[{"name":"جنس","values":["پارچه ضد آب"]}]'::text WHERE id = 56;
UPDATE products SET attributes_json = '[{"name":"اندازه","values":["۶۰ سانت","۸۰ سانت"]}]'::text WHERE id = 57;
UPDATE products SET attributes_json = '[{"name":"سن مناسب","values":["۸+ سال","۱۲+ سال"]}]'::text WHERE id IN (58,59);
UPDATE products SET attributes_json = '[{"name":"تعداد پارچه","values":["۹ پارچه"]}]'::text WHERE id = 60;

-- ۳) junction دسته‌های چندگانه — بازسازی + نمونه چنددسته‌ای
DELETE FROM product_categories;
INSERT INTO product_categories (product_id, category_id) SELECT id, category_id FROM products;
INSERT INTO product_categories (product_id, category_id) VALUES
  (1,3),(2,4),(3,5),(4,6),(5,4),(6,3),
  (15,12),(16,14),(18,12),(22,12),(23,16),
  (36,25),(37,26),(38,28),(39,29),(40,31),(41,32),
  (42,34),(44,39),(46,38)
ON CONFLICT DO NOTHING;

-- ۴) واریانت‌ها: حذف قبلی و ساخت ترکیب‌های جدید با موجودی مستقل
DELETE FROM product_variants;

-- گوشی‌ها (۱ تا ۶): حافظه × رنگ
INSERT INTO product_variants (product_id, attributes_json, reserved_count, stock, price, discount_percent, is_active, created_at)
SELECT p.id,
  jsonb_build_array(
    jsonb_build_object('name','حافظه داخلی','value',m.v),
    jsonb_build_object('name','رنگ','value',c.v)
  )::text,
  0, 3 + ((p.id * 7 + m.ord * 5 + c.ord * 11) % 22),
  CASE WHEN c.v = 'سفید' THEN 250000 ELSE NULL END,
  0, true, now()
FROM products p
CROSS JOIN LATERAL (VALUES ('۱۲۸ گیگ',0),('۲۵۶ گیگ',1),('۵۱۲ گیگ',2)) m(v,ord)
CROSS JOIN LATERAL (VALUES ('مشکی',0),('آبی',1),('سفید',2)) c(v,ord)
WHERE p.id BETWEEN 1 AND 6;

-- لپ‌تاپ‌ها (۱۵ تا ۱۸): رم × حافظه
INSERT INTO product_variants (product_id, attributes_json, reserved_count, stock, price, discount_percent, is_active, created_at)
SELECT p.id,
  jsonb_build_array(
    jsonb_build_object('name','رم','value',r.v),
    jsonb_build_object('name','حافظه','value',d.v)
  )::text,
  0, 2 + ((p.id * 5 + r.ord * 3 + d.ord * 7) % 12),
  CASE WHEN d.v = '۱ ترابایت' THEN 3500000 ELSE NULL END,
  0, true, now()
FROM products p
CROSS JOIN LATERAL (VALUES ('۸ گیگ',0),('۱۶ گیگ',1),('۳۲ گیگ',2)) r(v,ord)
CROSS JOIN LATERAL (VALUES ('۵۱۲ SSD',0),('۱ ترابایت',1)) d(v,ord)
WHERE p.id BETWEEN 15 AND 18;

-- پوشاک و کفش (۳۶ تا ۴۱): سایز × رنگ
INSERT INTO product_variants (product_id, attributes_json, reserved_count, stock, price, discount_percent, is_active, created_at)
SELECT p.id,
  jsonb_build_array(
    jsonb_build_object('name','سایز','value',s.v),
    jsonb_build_object('name','رنگ','value',c.v)
  )::text,
  0, 2 + ((p.id * 3 + s.ord * 5 + c.ord * 7) % 15),
  NULL, 0, true, now()
FROM products p
CROSS JOIN LATERAL (VALUES ('S',0),('M',1),('L',2),('XL',3)) s(v,ord)
CROSS JOIN LATERAL (VALUES ('مشکی',0),('سرمه‌ای',1),('سفید',2)) c(v,ord)
WHERE p.id BETWEEN 36 AND 41;

-- عطرها (۴۴، ۴۵): حجم
INSERT INTO product_variants (product_id, attributes_json, reserved_count, stock, price, discount_percent, is_active, created_at)
SELECT p.id,
  jsonb_build_array(jsonb_build_object('name','حجم','value',z.v))::text,
  0, 4 + ((p.id * 3 + z.ord * 6) % 14),
  CASE WHEN z.v = '۱۰۰ میل' THEN 800000 ELSE NULL END,
  0, true, now()
FROM products p
CROSS JOIN LATERAL (VALUES ('۵۰ میل',0),('۱۰۰ میل',1)) z(v,ord)
WHERE p.id IN (44,45);

-- ۵) موجودی کل محصولات دارای واریانت = جمع واریانت‌ها
UPDATE products p SET stock = s.total
FROM (SELECT product_id, SUM(stock) AS total FROM product_variants GROUP BY product_id) s
WHERE p.id = s.product_id;

-- ۶) ویژگی‌های نوع-واریانت برای دسته‌های موبایل و لپ‌تاپ و پوشاک و کفش و عطر
UPDATE category_attributes SET is_variant_option = true WHERE name IN ('حافظه داخلی','رنگ','رم','سایز','حجم') AND is_variant_option = false;

-- ۷) محصول ۱ باید استوک و شگفت‌انگیز باشد (برای نمایش شمارش معکوس)
UPDATE products SET is_amazing = true, amazing_ends_at = now() + interval '3 days', stock = GREATEST(stock, 10) WHERE id = 1;

COMMIT;
