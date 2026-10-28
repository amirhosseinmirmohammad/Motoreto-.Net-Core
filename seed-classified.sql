-- ============================================================
-- Seed آگهی‌های دست دوم (مدل دیوار) — دسته‌ها + ویژگی‌های داینامیک
-- قابل اجرای مجدد (idempotent)
-- ============================================================

-- ---------- دسته‌ها ----------
INSERT INTO classified_categories (name, slug, icon, sort_order, is_active)
SELECT v.name, v.slug, v.icon, v.ord, true
FROM (VALUES
    ('املاک', 'amlak', 'bi-house-door', 1),
    ('وسایل نقلیه', 'vehicles', 'bi-car-front', 2),
    ('موبایل و تبلت', 'mobile', 'bi-phone', 3),
    ('لپ‌تاپ و کامپیوتر', 'computer', 'bi-laptop', 4),
    ('لوازم خانگی', 'home-appliances', 'bi-tv', 5),
    ('استخدام و کاریابی', 'jobs', 'bi-briefcase', 6),
    ('خدمات', 'services', 'bi-tools', 7),
    ('استفاده‌ی نو', 'new-items', 'bi-gift', 8),
    ('لوازم شخصی', 'personal', 'bi-handbag', 9)
) AS v(name, slug, icon, ord)
WHERE NOT EXISTS (SELECT 1 FROM classified_categories c WHERE c.slug = v.slug);

-- ---------- ویژگی‌های املاک (اجاره/فروش) ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('نوع آگهی', 'select', 'اجاره‌ی خانه آپارتمان,فروش خانه آپارتمان,اجاره‌ی مغازه,فروش مغازه,زمین و کلنگی', true, 1),
    ('متراژ', 'number', NULL, true, 2),
    ('اتاق‌ها', 'number', NULL, false, 3),
    ('سال ساخت', 'number', NULL, false, 4),
    ('طبقه', 'text', NULL, false, 5),
    ('پارکینگ', 'bool', NULL, false, 6),
    ('انباری', 'bool', NULL, false, 7),
    ('آسانسور', 'bool', NULL, false, 8)
) AS v(name, type, options, req, ord) ON c.slug = 'amlak'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های وسایل نقلیه ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('نوع', 'select', 'خودروی سواری,موتورسیکلت,وانت و کامیون', true, 1),
    ('برند', 'select', 'پراید,پژو,سمند,تیبا,کوییک,شاهین,دنا,رانا,هیوندای,کیا,تویوتا,هوندا,سایر', true, 2),
    ('مدل (سال)', 'number', NULL, true, 3),
    ('کارکرد (کیلومتر)', 'number', NULL, false, 4),
    ('رنگ', 'select', 'سفید,مشکی,نقره‌ای,خاکستری,قرمز,آبی,سایر', false, 5),
    ('گیربکس', 'select', 'دستی,اتوماتیک', false, 6),
    ('مهلت بیمه (ماه)', 'number', NULL, false, 7)
) AS v(name, type, options, req, ord) ON c.slug = 'vehicles'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های موبایل و تبلت ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('برند', 'select', 'سامسونگ,شیائومی,اپل,هواوی,نوکیا,سایر', true, 1),
    ('حافظه داخلی', 'select', '۳۲ گیگ,۶۴ گیگ,۱۲۸ گیگ,۲۵۶ گیگ,۵۱۲ گیگ,۱ ترابایت', false, 2),
    ('رم', 'select', '۳ گیگ,۴ گیگ,۶ گیگ,۸ گیگ,۱۲ گیگ', false, 3),
    ('وضعیت', 'select', 'نو,در حد نو,کارکرده', true, 4),
    ('گارانتی دارد', 'bool', NULL, false, 5)
) AS v(name, type, options, req, ord) ON c.slug = 'mobile'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های لپ‌تاپ و کامپیوتر ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('برند', 'select', 'اپل,ایسوس,لنوو,اچ‌پی,دل,ایسر,سایر', true, 1),
    ('پردازنده', 'select', 'Core i3,Core i5,Core i7,Ryzen 5,Ryzen 7,Apple M1,Apple M2,سایر', false, 2),
    ('رم', 'select', '۴ گیگ,۸ گیگ,۱۶ گیگ,۳۲ گیگ', false, 3),
    ('حافظه', 'select', '۲۵۶ گیگ SSD,۵۱۲ گیگ SSD,۱ ترابایت SSD,۱ ترابایت HDD', false, 4),
    ('وضعیت', 'select', 'نو,در حد نو,کارکرده', true, 5)
) AS v(name, type, options, req, ord) ON c.slug = 'computer'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های لوازم خانگی ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('نوع کالا', 'select', 'یخچال و فریزر,ماشین لباسشویی,ماشین ظرفشویی,تلویزیون,جاروبرقی,مایکروویو,سایر', true, 1),
    ('برند', 'text', NULL, false, 2),
    ('مدت استفاده (سال)', 'number', NULL, false, 3),
    ('وضعیت', 'select', 'نو,در حد نو,کارکرده', true, 4)
) AS v(name, type, options, req, ord) ON c.slug = 'home-appliances'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های استخدام ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('عنوان شغلی', 'text', NULL, true, 1),
    ('نوع همکاری', 'select', 'تمام‌وقت,پاره‌وقت,دورکاری,پروژه‌ای', true, 2),
    ('حقوق', 'select', 'توافقی,حداقل حقوق,بالای حداقل حقوق', false, 3),
    ('سابقه کار لازم (سال)', 'number', NULL, false, 4)
) AS v(name, type, options, req, ord) ON c.slug = 'jobs'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های خدمات ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('نوع خدمت', 'select', 'نظافتی,تعمیرات,حمل‌ونقل,آموزش و تدریس,زیبایی و سلامت,سایر', true, 1),
    ('سابقه (سال)', 'number', NULL, false, 2)
) AS v(name, type, options, req, ord) ON c.slug = 'services'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);

-- ---------- ویژگی‌های لوازم شخصی ----------
INSERT INTO classified_attributes (category_id, name, type, options, is_required, sort_order)
SELECT c.id, v.name, v.type, v.options, v.req, v.ord
FROM classified_categories c
JOIN (VALUES
    ('نوع', 'select', 'آرایشی و بهداشتی,پوشاک و کیف و کفش,ساعت و زیورآلات,ورزش و سرگرمی,سایر', true, 1),
    ('وضعیت', 'select', 'نو,در حد نو,کارکرده', false, 2)
) AS v(name, type, options, req, ord) ON c.slug = 'personal'
WHERE NOT EXISTS (SELECT 1 FROM classified_attributes a WHERE a.category_id = c.id AND a.name = v.name);
