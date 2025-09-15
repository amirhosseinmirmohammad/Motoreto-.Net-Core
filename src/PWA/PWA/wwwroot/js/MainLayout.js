/* wwwroot/js/_layout.js */
"use strict";

/* -----------------------
   Helpers (Global funcs)
------------------------*/
window.updateCartBadge = function (count) {
    // هدر
    var $header = $("#shoppingCount");
    $header.text(count).addClass("update");
    setTimeout(function () { $header.removeClass("update"); }, 400);

    // موبایل
    var $mobile = $("#shoppingCountMobile");
    if ($mobile.length) {
        $mobile.text(count).addClass("update");
        setTimeout(function () { $mobile.removeClass("update"); }, 400);
    }

    // سایدمنو
    var $side = $("#shoppingCountSide");
    if ($side.length) {
        $side.text(count);
        $side.toggleClass("hidden", parseInt(count, 10) <= 0);
    }
};

window.openSearch = function () {
    var el = document.getElementById("myOverlay");
    if (el) el.style.display = "block";
};
window.closeSearch = function () {
    var el = document.getElementById("myOverlay");
    if (el) el.style.display = "none";
};
window.myFunction = function () {
    var x = document.getElementById("menu-wrapper");
    if (!x) return;
    x.style.display = (x.style.display === "block") ? "none" : "block";
};
window.func2 = function () {
    var x = document.getElementById("menu-wrapper");
    if (x) x.style.display = "block";
};

/* -----------------------
   DOM Ready (jQuery)
------------------------*/
$(function () {

    // افزودن به سبد خرید
    $(".btncard").on("click", function (e) {
        e.preventDefault();
        var id = $(this).attr("productid");
        $.ajax({
            url: "/Home/AddToShoppingCart",
            data: { Id: id },
            type: "POST",
            dataType: "json",
            success: function (result) {
                if (result && result.Success) {
                    window.updateCartBadge(result.CountHtml);
                    $("#success-modal").modal("show");
                }
            },
            error: function () { alert("خطایی رخ داده است !"); }
        });
    });

    // فلش‌های Owl Carousel
    $(".owl-prev").html('<i class="fa fa-chevron-right"></i>');
    $(".owl-next").html('<i class="fa fa-chevron-left"></i>');

    // خبرنامه ۱
    $("#btntext").on("click", function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Home/NewsLetter",
            contentType: "application/json; charset=utf-8",
            data: { text: $("#textnews").val() },
            dataType: "json",
            type: "GET",
            success: function (Data) {
                var $msg = $("#msg");
                if (!$msg.length) return;
                var ok = Data && Data.status == "1";
                $msg.text(Data.text).css({ color: ok ? "green" : "red", "font-size": "larger" })
                    .parent("h6").css("padding-top", "20px");
            },
            error: function () { alert("خطایی رخ داده است لطفا دوباره امتحان کنید ."); }
        });
    });

    // خبرنامه ۲
    $("#btntext2").on("click", function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Home/NewsLetter",
            contentType: "application/json; charset=utf-8",
            data: { text: $("#textnews2").val() },
            dataType: "json",
            type: "GET",
            success: function (Data) {
                var $msg = $("#msg2");
                if (!$msg.length) return;
                var ok = Data && Data.status == "1";
                $msg.text(Data.text).css({ color: ok ? "green" : "red", "font-size": "larger" })
                    .parent("h6").css("padding-top", "20px");
            },
            error: function () { alert("خطایی رخ داده است لطفا دوباره امتحان کنید ."); }
        });
    });

    // دکمه منوی موبایل
    $(".btn-nav").on("click tap", function () {
        $(".nav-container").toggleClass("showNav hideNav").removeClass("hidden");
        $(this).toggleClass("animated");
    });

    // اسکرول نرم
    $(".scroll").on("click", function (event) {
        event.preventDefault();
        var target = $(this.hash);
        if (target.length) $('html,body').animate({ scrollTop: target.offset().top }, 1000);
    });

    // Tooltip (وجود پلاگین)
    var ttSel = '[data-toggle="tooltip"], [data-bs-toggle="tooltip"]';
    if ($.fn.tooltip) $(ttSel).tooltip();

    // Scroll-Up
    (function () {
        var $btn = $('.scroll-up');
        var $anchor = $('#totop');
        var anchorTop = ($anchor.length && $anchor.offset()) ? ($anchor.offset().top || 0) : 0;
        var lastShown = null;
        function update() {
            var shouldShow = $(window).scrollTop() > anchorTop + 5;
            if (shouldShow === lastShown) return;
            lastShown = shouldShow;
            if (shouldShow) $btn.stop(true, true).fadeIn(200);
            else $btn.stop(true, true).fadeOut(200);
        }
        $(window).on('scroll', update);
        update();
    })();

    // Persian Number (اگر لود شده)
    if ($.fn.persiaNumber) $('body').persiaNumber();

    // موقعیت FAB نسبت به تب‌های موبایل
    (function placeFabInit() {
        function placeFab() {
            var fab = document.querySelector('.fab-call');
            var tabs = document.getElementById('mobile-tabs');
            if (!fab) return;
            if (window.innerWidth <= 768 && tabs) {
                var gap = 20;
                fab.style.bottom = (tabs.offsetHeight + gap) + 'px';
            } else {
                fab.style.bottom = '20px';
            }
        }
        $(window).on('load resize', placeFab);
        document.addEventListener('DOMContentLoaded', placeFab);
        placeFab();
    })();

    // هایلایت تب‌های موبایل
    (function () {
        $("#mobile-tabs .mobile-tab-wrapper").removeClass("active");
        var url = window.location.href.toLowerCase();
        if (url.includes("#categories")) {
            $("#mobile-tabs .mobile-tab-wrapper[href*='#categories']").addClass("active");
        } else if (url.includes("cart")) {
            $("#mobile-tabs .mobile-tab-wrapper[href*='cart']").addClass("active");
        } else if (url.includes("contactus")) {
            $("#mobile-tabs .mobile-tab-wrapper[href*='contactus']").addClass("active");
        } else {
            $("#mobile-tabs .mobile-tab-wrapper[href='/']").addClass("active");
        }
    })();

    // سایدمنو
    (function () {
        var sideMenu = document.getElementById("sideMenu");
        var overlay = document.getElementById("menuOverlay");
        var openBtn = document.getElementById("openMenuBtn");
        var closeBtn = document.getElementById("closeMenuBtn");
        if (!sideMenu || !overlay || !openBtn || !closeBtn) return;

        function openMenu() {
            sideMenu.classList.add("open");
            overlay.classList.add("show");
        }
        function closeMenu() {
            sideMenu.classList.remove("open");
            overlay.classList.remove("show");
        }
        openBtn.addEventListener("click", openMenu);
        closeBtn.addEventListener("click", closeMenu);
        overlay.addEventListener("click", closeMenu);

        // ساب‌منوها
        document.querySelectorAll(".submenu-toggle").forEach(function (toggle) {
            toggle.addEventListener("click", function (e) {
                e.preventDefault();
                var parent = this.parentElement;
                var submenu = parent.querySelector(".submenu");
                var arrow = this.querySelector(".submenu-arrow");
                document.querySelectorAll(".submenu").forEach(function (s) {
                    if (s !== submenu) s.style.display = "none";
                });
                if (!submenu) return;
                if (submenu.style.display === "block") {
                    submenu.style.display = "none";
                    if (arrow) arrow.style.transform = "rotate(0deg)";
                } else {
                    submenu.style.display = "block";
                    if (arrow) arrow.style.transform = "rotate(180deg)";
                }
            });
        });
    })();

}); // end of DOM ready

/* -----------------------
   Service Worker
------------------------*/
if ("serviceWorker" in navigator) {
    window.addEventListener("load", function () {
        navigator.serviceWorker.register("/sw.js")
            .catch(function (err) { console.warn("SW register failed", err); });
    });
}

/* -----------------------
   PWA Install Modal
------------------------*/
(function () {
    var MODAL = '#pwa-install-modal';
    var BTN_YES = '#pwa-install-yes';
    var BTN_NO = '#pwa-install-no';
    var IOS_STEPS = '#pwa-ios-steps';

    var DISMISS_COOKIE = 'pwa_prompt_dismissed';
    var INSTALLED_COOKIE = 'pwa_installed';
    var DISMISS_DAYS = 2;
    var deferredPrompt = null;

    function setCookie(name, value, days) {
        var d = new Date(); d.setTime(d.getTime() + (days * 864e5));
        document.cookie = name + "=" + encodeURIComponent(value) + ";expires=" + d.toUTCString() + ";path=/";
    }
    function getCookie(name) {
        var v = ("; " + document.cookie).split("; " + name + "=");
        return v.length === 2 ? decodeURIComponent(v.pop().split(";").shift()) : null;
    }

    function isStandalone() {
        return window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true;
    }
    function isIOS() { return /iphone|ipad|ipod/i.test(navigator.userAgent.toLowerCase()); }
    function isSafari() { return /^((?!chrome|android).)*safari/i.test(navigator.userAgent.toLowerCase()); }
    function isIOSSafari() { return isIOS() && isSafari(); }

    function shouldAsk() {
        if (isStandalone()) return false;
        if (getCookie(INSTALLED_COOKIE)) return false;
        if (getCookie(DISMISS_COOKIE)) return false;
        return true;
    }

    function openModal() {
        if (!shouldAsk()) return;

        if (isIOSSafari()) {
            var btnYes = document.querySelector(BTN_YES);
            if (btnYes) btnYes.style.display = 'none';
            var btnNo = document.querySelector(BTN_NO);
            if (btnNo) btnNo.textContent = 'متوجه شدم';
            var iosSteps = document.querySelector(IOS_STEPS);
            if (iosSteps) iosSteps.style.display = 'block';
        }

        if (!deferredPrompt && !isIOSSafari()) return;

        if (window.jQuery && jQuery.fn && jQuery.fn.modal) {
            jQuery(MODAL).modal('show');
        } else {
            window.addEventListener('load', function () { jQuery(MODAL).modal('show'); }, { once: true });
        }
    }

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferredPrompt = e;
        setTimeout(openModal, 600);
    });

    window.addEventListener('load', function () {
        setTimeout(openModal, 800); // برای iOS
    });

    document.addEventListener('click', function (e) {
        var yes = e.target.closest(BTN_YES);
        var no = e.target.closest(BTN_NO);

        if (yes && !isIOSSafari() && deferredPrompt) {
            deferredPrompt.prompt();
            deferredPrompt.userChoice.then(function (choice) {
                deferredPrompt = null;
                if (choice && choice.outcome === 'accepted') {
                    setCookie(INSTALLED_COOKIE, '1', 3650);
                } else {
                    setCookie(DISMISS_COOKIE, '1', DISMISS_DAYS);
                }
                if (window.jQuery) jQuery(MODAL).modal('hide');
            }).catch(function () {
                setCookie(DISMISS_COOKIE, '1', DISMISS_DAYS);
                if (window.jQuery) jQuery(MODAL).modal('hide');
            });
        }

        if (no) {
            setCookie(DISMISS_COOKIE, '1', DISMISS_DAYS);
            if (window.jQuery) jQuery(MODAL).modal('hide');
        }
    });

    window.addEventListener('appinstalled', function () {
        setCookie(INSTALLED_COOKIE, '1', 3650);
        if (window.jQuery) jQuery(MODAL).modal('hide');
    });
})();

/* -----------------------
   Desktop Splash (once/day)
------------------------*/
(function () {
    var SPLASH_ID = 'pwa-splash';
    var SEEN_COOKIE = 'pwa_splash_seen';
    var SEEN_DAYS = 1;
    var SHOW_MS = 5000;

    function setCookie(n, v, d) {
        var t = new Date(); t.setTime(t.getTime() + d * 864e5);
        document.cookie = n + "=" + encodeURIComponent(v) + ";expires=" + t.toUTCString() + ";path=/";
    }
    function getCookie(n) {
        var m = ("; " + document.cookie).split("; " + n + "=");
        return m.length === 2 ? decodeURIComponent(m.pop().split(";").shift()) : null;
    }

    var isDesktop = window.matchMedia('(min-width: 769px)').matches &&
        !/Android|iPhone|iPad|iPod|IEMobile|Opera Mini/i.test(navigator.userAgent);
    if (!isDesktop) return;

    function showSplash() {
        var el = document.getElementById(SPLASH_ID);
        if (!el || getCookie(SEEN_COOKIE)) return;
        el.hidden = false;
        document.documentElement.classList.add('pwa-noscroll');
        document.body.classList.add('pwa-noscroll');

        function hide() {
            el.classList.add('hide');
            document.documentElement.classList.remove('pwa-noscroll');
            document.body.classList.remove('pwa-noscroll');
            setCookie(SEEN_COOKIE, '1', SEEN_DAYS);
        }

        window.addEventListener('load', function () { setTimeout(hide, SHOW_MS); }, { once: true });

        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then(function () {
                setTimeout(function () { if (!el.classList.contains('hide')) hide(); }, 600);
            });
        }

        setTimeout(function () { if (!el.classList.contains('hide')) hide(); }, SHOW_MS + 2500);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", showSplash);
    } else {
        showSplash();
    }
})();

/* -----------------------
   Mobile Loader
------------------------*/
(function () {
    var el = document.getElementById('mb-loader');
    if (!el) return;

    var isMobile = window.matchMedia('(max-width: 768px)').matches ||
        /Android|iPhone|iPad|iPod|IEMobile|Opera Mini/i.test(navigator.userAgent);
    if (!isMobile) { if (el.parentNode) el.parentNode.removeChild(el); return; }

    document.documentElement.style.overflow = 'hidden';
    document.body.style.overflow = 'hidden';

    function hideLoader() {
        if (!el) return;
        el.classList.add('hide');
        setTimeout(function () {
            document.documentElement.style.overflow = '';
            document.body.style.overflow = '';
            if (el && el.parentNode) { el.parentNode.removeChild(el); el = null; }
        }, 380);
    }

    window.addEventListener('load', hideLoader, { once: true });
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.ready.then(function () { setTimeout(hideLoader, 400); });
    }
    setTimeout(hideLoader, 7000);
})();
