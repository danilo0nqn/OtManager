(function () {
    const STORAGE_THEME_KEY = 'ot-theme';
    const STORAGE_SIDEBAR_KEY = 'ot-sidebar-collapsed';

    const doc = document.documentElement;
    function applyTheme(theme) {
        if (theme === 'dark') {
            doc.classList.add('dark');
        } else {
            doc.classList.remove('dark');
        }
        updateThemeIcons(theme);
    }

    function updateThemeIcons(theme) {
        document.querySelectorAll('[data-theme-icon="moon"]').forEach(el => {
            el.classList.toggle('hidden', theme === 'dark');
        });
        document.querySelectorAll('[data-theme-icon="sun"]').forEach(el => {
            el.classList.toggle('hidden', theme !== 'dark');
        });
    }

    function currentTheme() {
        return doc.classList.contains('dark') ? 'dark' : 'light';
    }

    function initTheme() {
        const stored = localStorage.getItem(STORAGE_THEME_KEY);
        if (stored === 'light' || stored === 'dark') {
            applyTheme(stored);
        } else if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
            applyTheme('dark');
        } else {
            applyTheme('light');
        }
    }

    function toggleTheme() {
        const next = currentTheme() === 'dark' ? 'light' : 'dark';
        applyTheme(next);
        localStorage.setItem(STORAGE_THEME_KEY, next);
    }

    function initThemeToggles() {
        document.querySelectorAll('[data-theme-toggle]').forEach(btn => {
            btn.addEventListener('click', toggleTheme);
        });
    }

    function initSidebar() {
        const sidebar = document.querySelector('[data-sidebar]');
        const toggle = document.querySelector('[data-sidebar-toggle]');
        if (!sidebar || !toggle) {
            return;
        }

        const stored = localStorage.getItem(STORAGE_SIDEBAR_KEY);
        if (stored === 'true') {
            sidebar.classList.add('collapsed');
        }

        toggle.addEventListener('click', () => {
            sidebar.classList.toggle('collapsed');
            localStorage.setItem(STORAGE_SIDEBAR_KEY, sidebar.classList.contains('collapsed'));
        });
    }

    function initTabs() {
        document.querySelectorAll('[data-tabs]').forEach(tabRoot => {
            const buttons = Array.from(tabRoot.querySelectorAll('.tab-button'));
            const panels = Array.from(tabRoot.querySelectorAll('[data-tab-panel]'));
            if (!buttons.length || !panels.length) {
                return;
            }

            function activate(target) {
                buttons.forEach(btn => btn.classList.toggle('active', btn.dataset.tab === target));
                panels.forEach(panel => {
                    panel.classList.toggle('hidden', panel.dataset.tabPanel !== target);
                });
            }

            buttons.forEach(btn => {
                btn.addEventListener('click', () => activate(btn.dataset.tab));
            });

            activate(buttons[0].dataset.tab);
        });
    }

    function showToast(message, variant = 'info') {
        if (!message) {
            return;
        }
        const root = document.getElementById('toast-root');
        if (!root) {
            return;
        }
        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.dataset.variant = variant;
        toast.innerHTML = `<div class="font-semibold capitalize">${variant}</div><div>${message}</div>`;
        root.appendChild(toast);
        setTimeout(() => {
            toast.classList.add('opacity-0');
            setTimeout(() => toast.remove(), 300);
        }, 3200);
    }

    function initToastFromPayload() {
        document.querySelectorAll('[data-toast-payload]').forEach(el => {
            const message = el.getAttribute('data-message');
            const variant = el.getAttribute('data-variant') ?? 'info';
            if (message) {
                showToast(message, variant);
            }
            el.remove();
        });
    }

    function initLucide() {
        if (window.lucide && typeof window.lucide.createIcons === 'function') {
            window.lucide.createIcons();
        }
        window.addEventListener('ot-refresh-icons', () => {
            if (window.lucide && typeof window.lucide.createIcons === 'function') {
                window.lucide.createIcons();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        initTheme();
        initThemeToggles();
        initSidebar();
        initTabs();
        initToastFromPayload();
        initLucide();
    });
})();
