/* ===========================================
   Motoreto Progressive Web App Service Worker
   Version: 2.1  (safe precache + robust fetch)
   =========================================== */

const CACHE_VERSION = "v2.1";
const CACHE_NAME = `motoreto-cache-${CACHE_VERSION}`;

// مسیرها را همیشه نسبت به origin فعلی بساز
const RAW_ASSETS = [
    "/", "/offline.html", "/favicon.ico", "/manifest.webmanifest",
    "/css/style.css", "/js/site.min.js", "/images/sadr_logo.png",
    "/_framework/blazor.webassembly.js", "/_framework/dotnet.wasm"
];
const PRECACHE_URLS = RAW_ASSETS.map(u => new URL(u, self.location.origin).toString());

// ----------------------
// نصب (Precache) — با try/catch روی هر فایل
// ----------------------
self.addEventListener("install", (event) => {
    console.log("[SW] Installing and caching app shell...");

    event.waitUntil((async () => {
        const cache = await caches.open(CACHE_NAME);
        for (const url of PRECACHE_URLS) {
            try {
                const res = await fetch(url, { cache: "reload" });
                if (res && res.ok) {
                    await cache.put(url, res.clone());
                } else {
                    console.warn("[SW] Skip non-200 asset:", url, res && res.status);
                }
            } catch (err) {
                console.warn("[SW] Skip asset (fetch fail):", url, err);
            }
        }
        self.skipWaiting();
    })());
});

// ----------------------
// فعال‌سازی (حذف کش‌های قدیمی)
// ----------------------
self.addEventListener("activate", (event) => {
    console.log("[SW] Activating and cleaning old caches...");
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(
            keys.filter(k => k.startsWith("motoreto-cache-") && k !== CACHE_NAME)
                .map(k => { console.log("[SW] Deleting old cache:", k); return caches.delete(k); })
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

    // 1) صفحات (navigate) → Network First با fallback
    if (req.mode === "navigate") {
        event.respondWith((async () => {
            try {
                const online = await fetch(req);
                const cache = await caches.open(CACHE_NAME);
                cache.put(req, online.clone());
                return online;
            } catch (err) {
                console.warn("[SW] Offline, showing cached or offline page.");
                return (await caches.match(req)) || (await caches.match(new URL("/offline.html", self.location.origin).toString()));
            }
        })());
        return;
    }

    // 2) API → Network First با fallback کش
    if (url.pathname.startsWith("/api/")) {
        event.respondWith((async () => {
            try {
                const fresh = await fetch(req);
                const cache = await caches.open(CACHE_NAME);
                cache.put(req, fresh.clone());
                return fresh;
            } catch {
                return await caches.match(req);
            }
        })());
        return;
    }

    // 3) فایل‌های Blazor/MudBlazor → Cache First
    if (url.pathname.startsWith("/_framework") || url.pathname.startsWith("/_content")) {
        event.respondWith((async () => {
            const cache = await caches.open(CACHE_NAME);
            const cached = await cache.match(req);
            if (cached) return cached;
            const fetched = await fetch(req);
            cache.put(req, fetched.clone());
            return fetched;
        })());
        return;
    }

    // 4) استاتیک‌ها (CSS, JS, Images, Fonts) → Stale-While-Revalidate
    if (/\.(css|js|png|jpg|jpeg|webp|svg|woff2?|ttf|eot)$/i.test(url.pathname)) {
        event.respondWith((async () => {
            const cache = await caches.open(CACHE_NAME);
            const cached = await cache.match(req, { ignoreSearch: true });
            const fetching = fetch(req).then(res => {
                if (res && res.ok) cache.put(req, res.clone());
                return res;
            }).catch(() => null);
            return cached || (await fetching) || Response.error();
        })());
    }
});
