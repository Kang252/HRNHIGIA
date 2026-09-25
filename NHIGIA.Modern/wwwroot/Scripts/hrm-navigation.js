(function () {
    'use strict';
    var body = document.body;
    var toggle = document.querySelector('.hrm-menu-toggle');
    var overlay = document.querySelector('.hrm-sidebar-overlay');
    var sidebar = document.getElementById('leftsidebar');
    if (!toggle || !overlay || !sidebar) return;
    function read(key) { try { return localStorage.getItem(key); } catch (_) { return null; } }
    function write(key, value) { try { localStorage.setItem(key, value); } catch (_) { } }
    function isDrawer() { return window.innerWidth < 1280; }
    function update() {
        var open = isDrawer() ? body.classList.contains('hrm-menu-open') : !body.classList.contains('hrm-menu-collapsed');
        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Ẩn menu' : 'Mở menu');
        toggle.querySelector('.material-icons').textContent = open ? 'menu_open' : 'menu';
        sidebar.inert = !open;
        sidebar.setAttribute('aria-hidden', String(!open));
    }
    function closeDrawer() {
        if (isDrawer() && sidebar.contains(document.activeElement)) toggle.focus();
        body.classList.remove('hrm-menu-open');
        update();
    }
    if (read('hrm-menu-collapsed') === 'true') body.classList.add('hrm-menu-collapsed');
    update();
    toggle.addEventListener('click', function () {
        if (isDrawer()) body.classList.toggle('hrm-menu-open');
        else {
            body.classList.toggle('hrm-menu-collapsed');
            write('hrm-menu-collapsed', String(body.classList.contains('hrm-menu-collapsed')));
        }
        update();
    });
    overlay.addEventListener('click', closeDrawer);
    window.addEventListener('resize', closeDrawer);
    document.addEventListener('keydown', function (event) { if (event.key === 'Escape') closeDrawer(); });
    var sectionKey = 'hrm-menu-sections';
    var state;
    try { state = JSON.parse(read(sectionKey) || '{}'); } catch (_) { state = {}; }
    if (!state || typeof state !== 'object' || Array.isArray(state)) state = {};
    function setSection(section, open, persist) {
        section.classList.toggle('is-collapsed', !open);
        section.querySelector('.hrm-menu-section-toggle').setAttribute('aria-expanded', String(open));
        var item = section.nextElementSibling;
        while (item && !item.classList.contains('hrm-menu-section')) {
            item.classList.add('hrm-menu-section-item');
            item.classList.toggle('is-hidden', !open);
            item.hidden = !open;
            item.setAttribute('aria-hidden', String(!open));
            item = item.nextElementSibling;
        }
        if (persist) { state[section.dataset.menuSection] = open; write(sectionKey, JSON.stringify(state)); }
    }
    sidebar.querySelectorAll('.hrm-menu-section').forEach(function (section) {
        setSection(section, state[section.dataset.menuSection] !== false, false);
    });
    sidebar.addEventListener('click', function (event) {
        var button = event.target.closest('.hrm-menu-section-toggle');
        if (button) {
            event.preventDefault();
            event.stopPropagation();
            setSection(button.closest('.hrm-menu-section'), button.getAttribute('aria-expanded') !== 'true', true);
        } else if (event.target.closest('a') && isDrawer()) closeDrawer();
    });
})();
