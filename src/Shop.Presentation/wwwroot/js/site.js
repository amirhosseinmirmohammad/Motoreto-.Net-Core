/* ============================================================
   فروشگاه شاپ — اسکریپت اصلی (vanilla JS، بدون وابستگی)
   ============================================================ */
(function () {
    'use strict';

    // ---------- توکن Antiforgery ----------
    function antiforgeryToken() {
        var hidden = document.querySelector('input[name="__RequestVerificationToken"]');
        if (hidden) return hidden.value;
        var meta = document.querySelector('meta[name="csrf-token"]');
        return meta ? meta.getAttribute('content') : '';
    }

    // افزودن به سبد از کارت‌های Blazor — Promise برمی‌گرداند
    window.shopAddToCart = function (productId, variantId, quantity) {
        return fetch('/api/cart/add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiforgeryToken(),
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ productId: productId, variantId: variantId || null, quantity: quantity || 1 })
        }).then(function (r) { return r.json(); });
    };

    window.ajaxPost = function (url, data, onDone, onFail) {
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiforgeryToken(),
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(data)
        })
            .then(function (r) {
                if (r.status === 401) { window.location.href = '/account/login?returnUrl=' + encodeURIComponent(location.pathname); throw new Error('unauthorized'); }
                return r.json();
            })
            .then(onDone)
            .catch(function (e) { if (e.message !== 'unauthorized' && onFail) onFail(e); });
    };

    // ---------- API مبتنی بر Promise برای Blazor (توکن خودکار از endpoint داینامیک) ----------
    function fetchAntiforgeryToken() {
        return fetch('/api/antiforgery/token', { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.json(); })
            .then(function (d) { return d.token || ''; })
            .catch(function () { return ''; });
    }

    window.shopApi = {
        post: function (url, data) {
            return fetchAntiforgeryToken().then(function (token) {
                return fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token || antiforgeryToken(),
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify(data)
                }).then(function (r) { return r.json(); });
            });
        }
    };

    // ---------- توست ----------
    window.showToast = function (message, type) {
        type = type || 'success';
        var container = document.getElementById('toastContainer');
        if (!container) return;

        var icons = { success: 'check-circle', danger: 'x-circle', warning: 'exclamation-triangle', info: 'info-circle' };
        var el = document.createElement('div');
        el.className = 'toast align-items-center text-bg-' + type + ' border-0';
        el.setAttribute('role', 'alert');
        el.innerHTML =
            '<div class="d-flex"><div class="toast-body"><i class="bi bi-' + (icons[type] || 'info-circle') + ' me-2"></i>' + message + '</div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        container.appendChild(el);
        var toast = new bootstrap.Toast(el, { delay: 3500 });
        toast.show();
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
    };

    // ---------- فرمت قیمت ----------
    window.formatToman = function (rial) {
        var toman = Math.round(rial / 10);
        return toman.toLocaleString('fa-IR') + ' تومان';
    };

    // ---------- سبد خرید ----------
    window.refreshCartBadge = function (count) {
        var badge = document.getElementById('cartBadge');
        if (!badge) return;
        if (count > 0) {
            badge.textContent = count.toLocaleString('fa-IR');
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    };

    var addToCartBtn = document.getElementById('addToCartBtn');
    if (addToCartBtn) {
        addToCartBtn.addEventListener('click', function () {
            var productId = parseInt(this.dataset.id);
            var qty = parseInt((document.getElementById('qtyInput') || {}).value || 1);
            var btn = this;
            btn.disabled = true;
            ajaxPost('/api/cart/add', { productId: productId, quantity: qty, variantId: window.__selectedVariantId || null }, function (res) {
                btn.disabled = false;
                if (res.success) {
                    showToast(res.message, 'success');
                    if (res.cart) refreshCartBadge(res.cart.totalCount);
                } else {
                    showToast(res.message, 'danger');
                }
            }, function () { btn.disabled = false; showToast('خطا در ارتباط با سرور', 'danger'); });
        });
    }

    var favBtn = document.getElementById('favBtn');
    if (favBtn) {
        favBtn.addEventListener('click', function () {
            var btn = this;
            ajaxPost('/api/favorites/toggle', { productId: parseInt(btn.dataset.id) }, function (res) {
                var icon = btn.querySelector('i');
                if (res.added) {
                    icon.className = 'bi bi-heart-fill text-danger';
                    showToast(res.message, 'success');
                } else {
                    icon.className = 'bi bi-heart';
                    showToast(res.message, 'info');
                }
            });
        });
    }

    var commentBtn = document.getElementById('commentBtn');
    if (commentBtn) {
        commentBtn.addEventListener('click', function () {
            var text = document.getElementById('commentText').value.trim();
            if (text.length < 10) { showToast('متن نظر باید حداقل ۱۰ کاراکتر باشد.', 'warning'); return; }
            ajaxPost('/api/comments', { productId: parseInt(this.dataset.id), rating: window.selectedRating || 5, text: text }, function (res) {
                showToast(res.message, res.success ? 'success' : 'warning');
                if (res.success) document.getElementById('commentText').value = '';
            });
        });
    }

    // ---------- صفحه سبد خرید ----------
    document.querySelectorAll('.cart-qty').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var row = this.closest('[data-product-id]');
            var id = parseInt(this.dataset.id);
            var display = row.querySelector('.qty-display');
            var current = parseInt(display.textContent.replace(/[۰-۹]/g, function (d) { return '۰۱۲۳۴۵۶۷۸۹'.indexOf(d); })) || 1;
            var next = this.dataset.action === 'plus' ? current + 1 : current - 1;

            ajaxPost('/api/cart/update', { productId: id, quantity: next }, function (res) {
                if (res.success) { location.reload(); }
                else showToast(res.message, 'warning');
            });
        });
    });

    document.querySelectorAll('.cart-remove').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (!confirm('این محصول از سبد حذف شود؟')) return;
            ajaxPost('/api/cart/remove', { productId: parseInt(this.dataset.id) }, function (res) {
                if (res.success) { location.reload(); }
            });
        });
    });

    // ---------- جستجوی زنده ----------
    var searchInput = document.querySelector('.search-input');
    var suggestBox = document.getElementById('searchSuggest');
    var searchTimer = null;

    if (searchInput && suggestBox) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            var q = this.value.trim();
            if (q.length < 2) { suggestBox.classList.add('d-none'); return; }

            searchTimer = setTimeout(function () {
                fetch('/api/search/suggest?q=' + encodeURIComponent(q))
                    .then(function (r) { return r.json(); })
                    .then(function (items) {
                        if (!items.length) { suggestBox.classList.add('d-none'); return; }
                        suggestBox.innerHTML = items.map(function (p) {
                            return '<a class="suggest-item" href="' + p.url + '">' +
                                '<img src="' + p.image + '" alt="" loading="lazy" />' +
                                '<div class="flex-grow-1"><div class="small fw-bold text-truncate" style="max-width:260px">' + p.name + '</div>' +
                                '<small class="text-muted">' + window.formatToman(p.price) + '</small></div></a>';
                        }).join('');
                        suggestBox.classList.remove('d-none');
                    });
            }, 250);
        });

        document.addEventListener('click', function (e) {
            if (!suggestBox.contains(e.target) && e.target !== searchInput) {
                suggestBox.classList.add('d-none');
            }
        });
    }

    // ---------- اعداد فارسی ----------
    window.faNum = function (n) {
        return String(n).replace(/\d/g, d => '۰۱۲۳۴۵۶۷۸۹'[d]);
    };

    // ---------- شمارش معکوس‌ها ----------
    function initCountdowns() {
        document.querySelectorAll('.countdown[data-expires]').forEach(function (el) {
            if (el.dataset.cdInit) return; // جلوگیری از دوبار بایند
            el.dataset.cdInit = '1';

            var expires = new Date(el.dataset.expires).getTime();
            if (isNaN(expires)) return;

            var hEl = el.querySelector('[data-cd="h"]') || document.getElementById('cdHours');
            var mEl = el.querySelector('[data-cd="m"]') || document.getElementById('cdMinutes');
            var sEl = el.querySelector('[data-cd="s"]') || document.getElementById('cdSeconds');

            var timer = setInterval(function () {
                var diff = Math.max(0, expires - Date.now());
                var h = Math.floor(diff / 3600000);
                var m = Math.floor((diff % 3600000) / 60000);
                var s = Math.floor((diff % 60000) / 1000);

                if (hEl) hEl.textContent = faNum(String(h).padStart(2, '0'));
                if (mEl) mEl.textContent = faNum(String(m).padStart(2, '0'));
                if (sEl) sEl.textContent = faNum(String(s).padStart(2, '0'));

                if (diff <= 0) {
                    clearInterval(timer);
                    // پایان شگفت‌انگیز → تازه‌سازی بخش
                    if (el.closest('.amazing-box') && el.closest('.page-content')) {
                        window.location.reload();
                    }
                    // پایان رزرو تسویه → بازگشت به سبد
                    if (el.id === 'checkoutCountdown') {
                        showToast('مهلت رزرو موجودی به پایان رسید.', 'warning');
                        setTimeout(function () { location.href = '/cart'; }, 1500);
                    }
                }
            }, 1000);
        });
    }
    initCountdowns();
    window.shopInitPage = initCountdowns;

    // ---------- بازگشت آنلاین: رفرش خودکار بعد قطعی ----------
    var wasOffline = !navigator.onLine;
    window.addEventListener('online', function () {
        if (wasOffline) { window.location.reload(); }
    });
    window.addEventListener('offline', function () { wasOffline = true; });

    // ---------- خبرنامه ----------
    var newsForm = document.getElementById('newsletterForm');
    if (newsForm) {
        newsForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var email = this.email.value.trim();
            ajaxPost('/api/newsletter', { email: email }, function (res) {
                showToast(res.message, res.success ? 'success' : 'warning');
                if (res.success) newsForm.reset();
            });
        });
    }

    // ---------- Dark mode ----------
    (function () {
        var html = document.documentElement;
        var saved = localStorage.getItem('theme');
        if (saved) html.setAttribute('data-bs-theme', saved);
        function refresh() {
            var dark = html.getAttribute('data-bs-theme') === 'dark';
            var ic = document.getElementById('themeIcon');
            if (ic) ic.className = dark ? 'bi bi-sun fs-5' : 'bi bi-moon-stars fs-5';
        }
        refresh();
        var t = document.getElementById('themeToggle');
        if (t) t.addEventListener('click', function () {
            var dark = html.getAttribute('data-bs-theme') === 'dark';
            var next = dark ? 'light' : 'dark';
            html.setAttribute('data-bs-theme', next);
            localStorage.setItem('theme', next);
            refresh();
        });
    })();

    // ---------- PWA + اعلان بروزرسانی نسخه ----------
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function () {
            navigator.serviceWorker.register('/service-worker.js').then(function (reg) {
                // نسخه جدید در حالت انتظار است؟ (دپلوی قبلی)
                if (reg.waiting && navigator.serviceWorker.controller) notifyUpdate(reg.waiting);
                // نسخه جدید همین حالا نصب شد؟ (دپلوی جدید)
                reg.addEventListener('updatefound', function () {
                    var nw = reg.installing;
                    if (!nw) return;
                    nw.addEventListener('statechange', function () {
                        if (nw.state === 'installed' && navigator.serviceWorker.controller) notifyUpdate(nw);
                    });
                });
            }).catch(function () { /* offline naming */ });

            // پس از فعال‌شدن نسخه جدید (کاربر «بروزرسانی» زده) → رفرش کامل
            var refreshing = false;
            navigator.serviceWorker.addEventListener('controllerchange', function () {
                if (refreshing) return;
                refreshing = true;
                window.location.reload();
            });
        });
    }

    var updatePrompted = false;
    function notifyUpdate(worker) {
        if (updatePrompted || !worker) return;
        updatePrompted = true;
        window.__pendingWorker = worker;
        var banner = document.getElementById('updateBanner');
        if (banner) {
            banner.style.display = 'flex';
            var btn = document.getElementById('updateBtn');
            if (btn) btn.addEventListener('click', function () {
                banner.style.display = 'none';
                if (window.__pendingWorker) window.__pendingWorker.postMessage({ type: 'SKIP_WAITING' });
            });
            var closeBtn = document.getElementById('updateDismiss');
            if (closeBtn) closeBtn.addEventListener('click', function () { banner.style.display = 'none'; });
        } else {
            // بدون بنر: بروزرسانی بی‌صدا
            worker.postMessage({ type: 'SKIP_WAITING' });
        }
    }
    window.notifyUpdate = notifyUpdate;

    // ---------- اعلان‌ها (زنگوله هدر) ----------
    function faTime(s) {
        try { return new Date(s).toLocaleDateString('fa-IR') + ' ' + new Date(s).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' }); }
        catch (e) { return ''; }
    }
    function loadNotifications() {
        var badge = document.getElementById('notifBadge');
        if (!badge) return;
        fetch('/api/notifications/summary').then(function (r) { return r.ok ? r.json() : null; }).then(function (res) {
            if (!res) return;
            badge.textContent = res.unread > 99 ? '+99' : res.unread;
            badge.classList.toggle('d-none', res.unread === 0);
            var list = document.getElementById('notifList');
            if (!list) return;
            list.innerHTML = res.items.length === 0
                ? '<div class="px-3 py-3 small text-muted">اعلانی ندارید.</div>'
                : res.items.map(function (n) {
                    return '<a href="' + (n.link || '/panel/notifications') + '" class="list-group-item list-group-item-action small py-2' + (n.isRead ? ' text-muted' : ' fw-bold') + '" data-nid="' + n.id + '">' +
                        '<i class="bi ' + (n.icon || 'bi-bell') + ' me-1 text-primary"></i>' + n.title +
                        (n.body ? '<span class="d-block fw-normal text-muted mt-1">' + n.body + '</span>' : '') +
                        '<span class="d-block fw-normal text-muted small mt-1">' + faTime(n.createdAt) + '</span></a>';
                }).join('');
            list.querySelectorAll('[data-nid]').forEach(function (el) {
                el.addEventListener('click', function () { fetch('/api/notifications/read/' + el.dataset.nid); });
            });
        }).catch(function () { });
    }
    window.loadNotifications = loadNotifications;
    window.addEventListener('load', loadNotifications);
    setInterval(loadNotifications, 45000);
    var bell = document.getElementById('notifBell');
    if (bell) bell.addEventListener('click', loadNotifications);
    var readAll = document.getElementById('notifReadAll');
    if (readAll) readAll.addEventListener('click', function (e) {
        e.preventDefault();
        fetch('/api/notifications/read-all').then(loadNotifications);
    });

    // ---------- SignalR: اعلان زنده ----------
    if (window.signalR && document.getElementById('notifBell')) {
        var notifConn = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/app')
            .withAutomaticReconnect()
            .build();
        notifConn.on('notify', function (n) {
            loadNotifications();
            if (window.showToast) window.showToast(n.title || 'اعلان جدید', 'info');
        });
        notifConn.start().catch(function () { /* polling ۴۵ ثانیه‌ای پشتیبان است */ });
    }

    // ---------- Web Push: فعال‌سازی اعلان مرورگر ----------
    window.enableWebPush = async function () {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) return 'مرورگر شما Push را پشتیبانی نمی‌کند.';
        var res = await fetch('/api/push/public-key');
        var cfg = await res.json();
        if (!cfg.key) return 'اعلان مرورگر تنظیم نشده است.';
        var permission = await Notification.requestPermission();
        if (permission !== 'granted') return 'اجازه اعلان داده نشد.';
        var reg = await navigator.serviceWorker.ready;
        var sub = await reg.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(cfg.key)
        });
        var raw = sub.toJSON();
        var post = await fetch('/api/push/subscribe', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': antiforgeryToken() },
            body: JSON.stringify({ endpoint: sub.endpoint, p256dh: raw.keys.p256dh, auth: raw.keys.auth })
        });
        var r = await post.json();
        return r.ok ? 'اعلان مرورگر فعال شد ✓ حتی با بسته‌بودن سایت پیام می‌گیرید.' : 'خطا در ذخیره اشتراک.';
    };
    function urlBase64ToUint8Array(base64String) {
        var padding = '='.repeat((4 - base64String.length % 4) % 4);
        var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        var raw = atob(base64);
        var arr = new Uint8Array(raw.length);
        for (var i = 0; i < raw.length; i++) arr[i] = raw.charCodeAt(i);
        return arr;
    }
    window.initUserHub = function (dotnetRef) {
        if (!window.signalR) return;
        var c = new signalR.HubConnectionBuilder().withUrl('/hubs/app').withAutomaticReconnect().build();
        c.on('notify', function () { dotnetRef.invokeMethodAsync('OnRemoteNotify'); });
        c.start().catch(function () { });
        window.__userHub = c;
    };
    window.disposeUserHub = function () {
        if (window.__userHub) { try { window.__userHub.stop(); } catch (e) { } window.__userHub = null; }
    };

    window.initChatHub = function (adId, dotnetRef) {
        function start() {
            if (!window.signalR) return;
            var c = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/app?adId=' + adId)
                .withAutomaticReconnect()
                .build();
            c.on('chatRefresh', function () { dotnetRef.invokeMethodAsync('OnRemoteMessage'); });
            c.start().catch(function () { });
            window.__chatConn = c;
        }
        if (window.signalR) start();
        else { var t = setInterval(function () { if (window.signalR) { clearInterval(t); start(); } }, 300); setTimeout(function () { clearInterval(t); }, 10000); }
    };
    window.disposeChatHub = function () {
        if (window.__chatConn) { try { window.__chatConn.stop(); } catch (e) { } window.__chatConn = null; }
    };

    // ---------- انیمیشن reveal با اسکرول (variant + stagger) ----------
    function initReveal() {
        var els = document.querySelectorAll('.reveal:not(.visible)');
        if (!els.length) return;

        if (!window.IntersectionObserver) {
            els.forEach(function (el) { el.classList.add('visible'); });
            return;
        }

        // تأخیر پله‌ای برای فرزندان ظرف‌های data-reveal-stagger
        document.querySelectorAll('[data-reveal-stagger]').forEach(function (group) {
            group.querySelectorAll(':scope .reveal:not(.visible)').forEach(function (el, i) {
                el.style.transitionDelay = (i * 70) + 'ms';
            });
        });

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) {
                if (!en.isIntersecting) return;
                var el = en.target;
                el.classList.add('visible');
                io.unobserve(el);
                var d = parseFloat(el.style.transitionDelay || '0');
                if (d > 0) { setTimeout(function () { el.style.transitionDelay = ''; }, d + 750); }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });
        els.forEach(function (el) { io.observe(el); });
    }
    initReveal();
    var _oldInit = window.shopInitPage;
    window.shopInitPage = function () { if (_oldInit) _oldInit(); initReveal(); };
    // هوک برای Blazor — بعد از هر رندر مجدد DOM فراخوانی می‌شود
    window.shopReveal = { init: initReveal };

    // ---------- زوم عکس محصول + لایت‌باکس ----------
    var stage = document.querySelector('.zoom-stage');
    if (stage) {
        var img = stage.querySelector('img');
        stage.addEventListener('mousemove', function (e) {
            var rect = stage.getBoundingClientRect();
            var x = ((e.clientX - rect.left) / rect.width) * 100;
            var y = ((e.clientY - rect.top) / rect.height) * 100;
            img.style.transformOrigin = x + '% ' + y + '%';
        });
        stage.addEventListener('mouseenter', function () { stage.classList.add('zooming'); });
        stage.addEventListener('mouseleave', function () { stage.classList.remove('zooming'); });
        stage.addEventListener('click', function () {
            var lb = document.createElement('div');
            lb.className = 'lightbox open';
            lb.innerHTML = '<img src="' + img.src + '" alt="" />';
            document.body.appendChild(lb);
            lb.addEventListener('click', function () { lb.remove(); });
        });
    }

    // ---------- PWA Install prompt ----------
    var deferredPrompt = null;
    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;
        var btn = document.getElementById('installPwaBtn');
        if (btn) { btn.hidden = false; }
    });
    var installBtn = document.getElementById('installPwaBtn');
    if (installBtn) {
        installBtn.addEventListener('click', function () {
            if (!deferredPrompt) return;
            deferredPrompt.prompt();
            deferredPrompt = null;
            installBtn.hidden = true;
        });
    }
})();

/* ---------- Infinite Scroll (Blazor) ---------- */
window.shopInfiniteScroll = {
  _obs: null,
  init: function (dotnetRef) {
    this.dispose();
    var sentinel = document.getElementById('loadMoreSentinel');
    if (!sentinel || !window.IntersectionObserver) return;
    this._obs = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (e.isIntersecting) dotnetRef.invokeMethodAsync('LoadMore');
      });
    }, { rootMargin: '600px 0px' });
    this._obs.observe(sentinel);
  },
  dispose: function () {
    if (this._obs) { this._obs.disconnect(); this._obs = null; }
  }
};

/* ---------- درگ افقی ردیف‌های اسکرولی با موس ---------- */
(function(){
  var dragEl=null,dragX=0,dragLeft=0,moved=0;
  document.addEventListener('mousedown',function(e){
    var row=e.target.closest?e.target.closest('.scroll-row'):null;
    if(!row)return;
    dragEl=row;dragX=e.pageX;dragLeft=row.scrollLeft;moved=0;
    row.classList.add('dragging');
  });
  document.addEventListener('mousemove',function(e){
    if(!dragEl)return;
    var dx=e.pageX-dragX;
    if(Math.abs(dx)>5)moved=1;
    dragEl.scrollLeft=dragLeft-dx;
  });
  document.addEventListener('mouseup',function(){
    if(dragEl){dragEl.classList.remove('dragging');dragEl=null;}
  });
  document.addEventListener('click',function(e){
    if(!moved)return;
    var row=e.target.closest?e.target.closest('.scroll-row'):null;
    if(row){e.preventDefault();e.stopPropagation();moved=0;}
  },true);
})();

/* ---------- سوایپ/درگ اسلایدر اصلی ---------- */
window.shopSlider = {
  init: function () {
    var car = document.getElementById('mainSlider');
    if (!car || car.dataset.swipeInit === '1') return;
    car.dataset.swipeInit = '1';
    car.classList.add('swipeable');

    var startX = 0, dx = 0, dragging = false;

    function down(x) { startX = x; dx = 0; dragging = true; car.classList.add('dragging'); }
    function move(x) { if (!dragging) return; dx = x - startX; if (Math.abs(dx) > 8) car.classList.add('drag-far'); }
    function up() {
      if (!dragging) return;
      dragging = false;
      car.classList.remove('dragging', 'drag-far');
      if (Math.abs(dx) > 60) {
        var inst = (window.bootstrap && bootstrap.Carousel) ? bootstrap.Carousel.getOrCreateInstance(car) : null;
        if (inst) { if (dx < 0) inst.next(); else inst.prev(); }
        else if (dx < 0) { (car.querySelector('.carousel-control-next') || {}).click(); }
        else { (car.querySelector('.carousel-control-prev') || {}).click(); }
      }
      dx = 0;
    }

    car.addEventListener('mousedown', function (e) { if (e.button === 0) { e.preventDefault(); down(e.clientX); } });
    window.addEventListener('mousemove', function (e) { if (dragging) move(e.clientX); });
    window.addEventListener('mouseup', up);
    car.addEventListener('touchstart', function (e) { down(e.touches[0].clientX); }, { passive: true });
    car.addEventListener('touchmove', function (e) { if (dragging) move(e.touches[0].clientX); }, { passive: true });
    car.addEventListener('touchend', up);
    car.addEventListener('click', function (e) {
      if (Math.abs(dx) > 60) { e.preventDefault(); e.stopPropagation(); dx = 0; }
    }, true);
  }
};

/* ---------- سلکت سرچ‌دار (MVC — فقط سلکت‌های data-search) ---------- */
function shopEnhanceSelects() {
    document.querySelectorAll('select[data-search]').forEach(function (sel) {
        if (sel.getAttribute('data-ss') === '1' || sel.closest('.ss-wrap')) return;
        sel.setAttribute('data-ss', '1');
        var wrap = document.createElement('div');
        wrap.className = 'ss-wrap';
        sel.parentNode.insertBefore(wrap, sel);
        var input = document.createElement('input');
        input.type = 'text'; input.className = 'form-control'; input.autocomplete = 'off';
        input.placeholder = 'جستجو و انتخاب...';
        input.value = sel.options[sel.selectedIndex] ? sel.options[sel.selectedIndex].text : '';
        wrap.appendChild(input);
        var list = document.createElement('div');
        list.className = 'ss-list'; list.style.display = 'none';
        wrap.appendChild(list);
        sel.style.display = 'none';
        function render() {
            var q = input.value.trim().toLowerCase();
            list.innerHTML = '';
            var opts = Array.prototype.filter.call(sel.options, function (o) {
                return !q || o.text.toLowerCase().indexOf(q) !== -1;
            }).slice(0, 50);
            opts.forEach(function (o) {
                var d = document.createElement('div');
                d.className = 'ss-item' + (o.selected ? ' active' : '');
                d.textContent = o.text;
                d.addEventListener('mousedown', function (ev) {
                    ev.preventDefault();
                    sel.value = o.value;
                    input.value = o.text;
                    list.style.display = 'none';
                    sel.dispatchEvent(new Event('change', { bubbles: true }));
                });
                list.appendChild(d);
            });
            if (!opts.length) {
                var e = document.createElement('div');
                e.className = 'ss-item text-muted'; e.textContent = 'موردی یافت نشد';
                list.appendChild(e);
            }
        }
        input.addEventListener('focus', function () { list.style.display = 'block'; render(); input.select(); });
        input.addEventListener('input', render);
        input.addEventListener('blur', function () {
            setTimeout(function () {
                list.style.display = 'none';
                input.value = sel.options[sel.selectedIndex] ? sel.options[sel.selectedIndex].text : '';
            }, 150);
        });
    });
}
document.addEventListener('DOMContentLoaded', shopEnhanceSelects);
window.shopEnhanceSelects = shopEnhanceSelects;
/* ---------- Rich Editor (Quill) ---------- */
window.richEditor = {
    refs: {},
    init: function (id, value, minHeight, dotnetRef) {
        var el = document.getElementById(id);
        if (!el || !window.Quill) return;
        var q = new Quill(el, {
            theme: 'snow',
            direction: 'rtl',
            placeholder: 'متن را وارد کنید...',
            modules: {
                toolbar: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['blockquote', 'link'],
                    [{ 'align': [] }],
                    ['clean']
                ]
            }
        });
        el.style.minHeight = (minHeight || 140) + 'px';
        if (value) q.root.innerHTML = value;
        var t = null;
        q.on('text-change', function () {
            clearTimeout(t);
            t = setTimeout(function () {
                dotnetRef.invokeMethodAsync('OnHtml', q.root.innerHTML);
            }, 400);
        });
        window.richEditor.refs[id] = q;
    },
    destroy: function (id) { delete window.richEditor.refs[id]; }
};
/* ---------- PWA Splash hide after load ---------- */
window.addEventListener('load', function () {
    var splash = document.getElementById('pwaSplash');
    if (splash) {
        setTimeout(function () { splash.classList.add('hide'); }, 800);
    }
});
/* اگر Blazor دیر لود شد، حداکثر ۳ ثانیه */
setTimeout(function () {
    var s = document.getElementById('pwaSplash');
    if (s) s.classList.add('hide');
}, 3000);