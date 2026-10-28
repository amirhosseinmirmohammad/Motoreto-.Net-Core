/* ============================================================
   Service Worker فروشگاه شاپ — استراتژی کش هوشمند:
   - App shell + استاتیک: Cache First
   - صفحات: Network First با fallback آفلاین
   ============================================================ */
const VERSION = 'shop-v1';
const STATIC_CACHE = VERSION + '-static';
const PAGE_CACHE = VERSION + '-pages';
const OFFLINE_URL = '/offline.html';

const PRECACHE = [
    OFFLINE_URL,
    '/lib/bootstrap/css/bootstrap.rtl.min.css',
    '/lib/bootstrap/js/bootstrap.bundle.min.js',
    '/lib/icons/bootstrap-icons.min.css',
    '/lib/icons/fonts/bootstrap-icons.woff2',
    '/css/site.css',
    '/js/site.js',
    '/fonts/Vazirmatn/Vazirmatn.woff2',
    '/images/icons/icon-192.png',
    '/images/icons/icon-512.png'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(STATIC_CACHE).then((cache) => cache.addAll(PRECACHE)).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(keys.filter((k) => !k.startsWith(VERSION)).map((k) => caches.delete(k)))
        ).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    const { request } = event;
    if (request.method !== 'GET') return;

    const url = new URL(request.url);
    if (url.origin !== location.origin) return;
    // API ها و درگاه پرداخت هرگز کش نمی‌شوند
    if (url.pathname.startsWith('/api/') || url.pathname.startsWith('/payment')) return;

    // استاتیک: Cache First
    if (url.pathname.startsWith('/lib/') || url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') || url.pathname.startsWith('/fonts/') ||
        url.pathname.startsWith('/images/')) {
        event.respondWith(
            caches.match(request).then((cached) => cached ||
                fetch(request).then((response) => {
                    const clone = response.clone();
                    caches.open(STATIC_CACHE).then((cache) => cache.put(request, clone));
                    return response;
                })
            )
        );
        return;
    }

    // صفحات: Network First + fallback آفلاین
    if (request.mode === 'navigate') {
        event.respondWith(
            fetch(request)
                .then((response) => {
                    const clone = response.clone();
                    caches.open(PAGE_CACHE).then((cache) => cache.put(request, clone));
                    return response;
                })
                .catch(() =>
                    caches.match(request).then((cached) => cached || caches.match(OFFLINE_URL))
                )
        );
    }
});
