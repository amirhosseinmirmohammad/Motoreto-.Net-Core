/* ============================================================
   فارسی‌سازی اعداد متن صفحات — سراسری (فروشگاه + ادمین)
   ورودی‌ها (input/textarea)، کد، و متن‌های dir=ltr دست‌نخورده می‌مانند.
   ============================================================ */
(function () {
    'use strict';

    var FA = '۰۱۲۳۴۵۶۷۸۹';

    function convert(text) {
        return text.replace(/[0-9]/g, function (d) { return FA[+d]; });
    }

    function isSkippedTextNode(node) {
        var p = node.parentElement;
        if (!p) return true;
        var tag = p.tagName;
        if (tag === 'SCRIPT' || tag === 'STYLE' || tag === 'CODE' || tag === 'PRE' ||
            tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || tag === 'OPTION') return true;
        if (p.closest && p.closest('[dir="ltr"],[data-no-fa],input,textarea,code,pre')) return true;
        return !/[0-9]/.test(node.nodeValue);
    }

    function isSkippedElement(el) {
        if (!el || !el.closest) return true;
        return !!el.closest('[dir="ltr"],[data-no-fa],input,textarea,script,style,code,pre');
    }

    function walk(root) {
        var walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
            acceptNode: function (n) {
                return isSkippedTextNode(n) ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT;
            }
        });
        var nodes = [];
        while (walker.nextNode()) nodes.push(walker.currentNode);
        for (var i = 0; i < nodes.length; i++) nodes[i].nodeValue = convert(nodes[i].nodeValue);
    }

    // اجرای اولیه
    function run() {
        if (document.body) walk(document.body);
    }
    run();

    // پس از هر تغییر DOM (Blazor / infinite scroll / AJAX) کل صفحه دوباره پیموده می‌شود.
    // متن‌های فارسی‌شده دیگر [0-9] ندارند، پس پیمایش مجدد بی‌هزینه و بدون اثر جانبی است.
    var scheduled = false;
    var mo = new MutationObserver(function () {
        if (scheduled) return;
        scheduled = true;
        requestAnimationFrame(function () {
            scheduled = false;
            if (document.body) walk(document.body);
        });
    });

    function startObserver() {
        if (document.body) mo.observe(document.body, { childList: true, subtree: true, characterData: true });
        else document.addEventListener('DOMContentLoaded', function () { mo.observe(document.body, { childList: true, subtree: true, characterData: true }); });
    }
    startObserver();
})();
