/* ============================================================
   SPA Navigation — بدون رفرش کامل (pjax)
   لینک‌ها و فرم‌های داخلی → fetch → سواپ محتوا
   ============================================================ */
(function () {
    'use strict';

    const CONTAINER_ID = 'pageContent';
    const EXCLUDE_SELECTORS = [
        '[target="_blank"]',
        '[data-no-pjax]',
        '[download]',
        'a[href^="mailto:"]',
        'a[href^="tel:"]',
        'a[href^="javascript"]',
        'a[href^="#"]',
        '.no-pjax'
    ];
    const EXCLUDE_PATHS = ['/payment/', '/admin/', '/api/'];

    function shouldPjax(el) {
        if (!el) return false;
        if (el.origin !== location.origin) return false;
        if (el.pathname === location.pathname && el.hash) return false;
        for (var sel of EXCLUDE_SELECTORS) {
            if (el.matches(sel) || el.closest(sel)) return false;
        }
        for (var path of EXCLUDE_PATHS) {
            if (el.pathname.startsWith(path)) return false;
        }
        return true;
    }

    function showLoading() {
        var bar = document.getElementById('pjaxBar');
        if (!bar) {
            bar = document.createElement('div');
            bar.id = 'pjaxBar';
            bar.innerHTML = '<div class="pjax-bar-inner"></div>';
            document.body.appendChild(bar);
        }
        bar.style.display = 'block';
        var inner = bar.querySelector('.pjax-bar-inner');
        inner.style.width = '0%';
        inner.classList.add('loading');
        requestAnimationFrame(function () { inner.style.width = '70%'; });
    }

    function hideLoading() {
        var bar = document.getElementById('pjaxBar');
        if (bar) {
            var inner = bar.querySelector('.pjax-bar-inner');
            inner.style.width = '100%';
            setTimeout(function () { bar.style.display = 'none'; }, 300);
        }
    }

    function scrollToTop() {
        window.scrollTo({ top: 0, behavior: 'instant' });
    }

    function updateCartBadge(count) {
        var badge = document.getElementById('cartBadge');
        if (!badge) return;
        if (count > 0) {
            badge.textContent = window.faNum ? window.faNum(count) : count;
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }

    async function fetchPage(url) {
        var resp = await fetch(url, {
            headers: { 'X-Pjax': 'true', 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (resp.redirected) {
            window.location.href = resp.url;
            return null;
        }
        if (!resp.ok) {
            window.location.href = url;
            return null;
        }

        var html = await resp.text();
        var doc = new DOMParser().parseFromString(html, 'text/html');

        var newContent = doc.getElementById(CONTAINER_ID);
        var newTitle = doc.querySelector('title');
        var newBadge = doc.getElementById('cartBadge');

        if (!newContent) {
            window.location.href = url;
            return null;
        }

        return {
            content: newContent.innerHTML,
            title: newTitle ? newTitle.textContent : document.title,
            cartCount: newBadge && !newBadge.classList.contains('d-none') ? newBadge.textContent : null
        };
    }

    function swapContent(data, url, push) {
        var container = document.getElementById(CONTAINER_ID);
        if (!container) return;

        // سواپ محتوا
        container.innerHTML = data.content;

        // عنوان
        document.title = data.title;

        // URL
        if (push) {
            history.pushState({ pjax: true }, '', url);
        }

        // badge سبد
        if (data.cartCount) updateCartBadge(parseInt(data.cartCount.replace(/[۰-۹]/g, function (d) { return '۰۱۲۳۴۵۶۷۸۹'.indexOf(d); })) || 0);

        // اسکرول به بالا
        scrollToTop();

        // Re-init صفحه
        if (window.shopInitPage) window.shopInitPage();
    }

    async function navigate(url, push) {
        showLoading();
        try {
            var data = await fetchPage(url);
            if (data) {
                swapContent(data, url, push !== false);
            }
        } catch (e) {
            window.location.href = url;
        } finally {
            hideLoading();
        }
    }

    async function submitPjaxForm(form) {
        showLoading();
        try {
            var formData = new FormData(form);
            var method = form.method.toUpperCase() || 'GET';
            var url = form.action;

            if (method === 'GET') {
                var params = new URLSearchParams(formData).toString();
                if (params) url += (url.includes('?') ? '&' : '?') + params;
                hideLoading();
                return navigate(url, true);
            }

            // POST: fetch و بررسی response type
            var resp = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: { 'X-Pjax': 'true' }
            });

            var contentType = resp.headers.get('Content-Type') || '';

            if (contentType.includes('application/json')) {
                // JSON response (مثل cart add)
                var json = await resp.json();
                if (json.cart) {
                    updateCartBadge(json.cart.totalCount);
                    if (window.showToast && json.message) window.showToast(json.message, json.success ? 'success' : 'danger');
                }
                hideLoading();
                return;
            }

            // HTML response (redirect بعد POST)
            var finalUrl = resp.url || url;
            var data = await fetchPage(finalUrl);
            if (data) {
                swapContent(data, finalUrl, false);
                history.replaceState({ pjax: true }, '', finalUrl);
            }
        } catch (e) {
            form.submit(); // fallback
        } finally {
            hideLoading();
        }
    }

    // ---- Global click interceptor ----
    document.addEventListener('click', function (e) {
        if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

        var link = e.target.closest('a');
        if (!link || !shouldPjax(link)) return;

        e.preventDefault();
        navigate(link.href, true);
    });

    // ---- Global form interceptor ----
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form.matches('form') || form.hasAttribute('data-no-pjax')) return;
        if (form.target === '_blank') return;
        if (form.enctype === 'multipart/form-data') return;

        // فرم‌های احراز هویت → full reload (کوکی نیاز داره)
        if (form.action && (form.action.includes('/account/login') || form.action.includes('/account/register') || form.action.includes('/account/logout')))
            return;

        // فرم‌های پرداخت → full reload
        if (form.action && form.action.includes('/payment/'))
            return;

        var method = (form.method || 'get').toLowerCase();
        if (method !== 'get' && method !== 'post') return;

        // فقط اگر داخل container اصلی باشد
        if (!form.closest('#' + CONTAINER_ID)) return;

        e.preventDefault();
        submitPjaxForm(form);
    });

    // ---- Back/Forward ----
    window.addEventListener('popstate', function (e) {
        navigate(location.href, false);
    });

    // ---- اولین لود: جایگزین initPage ----
    document.addEventListener('DOMContentLoaded', function () {
        if (window.shopInitPage) window.shopInitPage();
    });
})();
