/* Motoreto PWA Service Worker */
const CACHE_NAME = "motoreto-cache-v3";

const PRECACHE_URLS = [
    "/",                 // صفحه اصلی
    "/offline.html",     // اگر ندارید موقتاً این خط را حذف کنید
    "/css/site.min.css", // CSS bundle
    "/js/site.min.js",   // JS bundle
    "/images/sadr_logo.png"
];

// ----------------------
// نصب (Precache) — مقاوم به خطا
// ----------------------
self.addEventListener("install", (event) => {
    event.waitUntil((async () => {
        const cache = await caches.open(CACHE_NAME);

        for (const url of PRECACHE_URLS) {
            try {
                const req = new Request(url, { cache: "reload" });
                const res = await fetch(req);
                if (!res.ok) throw new Error(`${res.status} ${url}`);
                await cache.put(req, res.clone());
                // console.log("[SW] precached:", url);
            } catch (err) {
                console.warn("[SW] precache skip:", url, err.message);
            }
        }
        self.skipWaiting();
    })());
});

// ----------------------
// فعال‌سازی (پاکسازی کش‌های قدیمی)
// ----------------------
self.addEventListener("activate", (event) => {
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(
            keys
                .filter(k => k.startsWith("motoreto-") && k !== CACHE_NAME)
                .map(k => caches.delete(k))
        );
        await self.clients.claim();
    })());
});

// ----------------------
// Fetch Strategy
// ----------------------
self.addEventListener("fetch", (event) => {
    const req = event.request;
    const url = new URL(req.url);

    // فقط http/https — درخواست‌های chrome-extension و ... را نادیده بگیر
    if (url.protocol !== "http:" && url.protocol !== "https:") return;

    // صفحات (HTML): شبکه‌اول با فالبک آفلاین
    if (req.mode === "navigate") {
        event.respondWith((async () => {
            try {
                const fresh = await fetch(req);
                const cache = await caches.open(CACHE_NAME);
                cache.put(req, fresh.clone());
                return fresh;
            } catch {
                return (await caches.match(req)) || (await caches.match("/offline.html"));
            }
        })());
        return;
    }

    // استاتیک‌ها: stale-while-revalidate
    if (/\.(css|js|png|jpg|jpeg|webp|svg|woff2?)$/i.test(url.pathname)) {
        event.respondWith((async () => {
            const cache = await caches.open(CACHE_NAME);
            const cached = await cache.match(req, { ignoreSearch: true });
            const fetchPromise = fetch(req).then(res => {
                cache.put(req, res.clone());
                return res;
            }).catch(() => cached);
            return cached || fetchPromise;
        })());
    }
});
