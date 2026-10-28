// پنل مدیریت — اسکریپت سبک
(function () {
    var toggle = document.getElementById('menuToggle');
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');

    if (toggle && sidebar) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('open');
            if (overlay) overlay.classList.toggle('show');
        });
        if (overlay) {
            overlay.addEventListener('click', function () {
                sidebar.classList.remove('open');
                overlay.classList.remove('show');
            });
        }
    }

    // تأیید حذف
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm(form.dataset.confirm || 'مطمئن هستید؟')) e.preventDefault();
        });
    });
})();
