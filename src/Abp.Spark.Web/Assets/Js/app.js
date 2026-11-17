// Vristo Theme & Sidebar Accordion Layout Controller
(function () {
    function getCookie(name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    }

    const ThemeManager = {
        getStoredTheme() {
            return localStorage.getItem('spark_theme') || getCookie('spark_theme') || 'system';
        },
        setTheme(theme) {
            localStorage.setItem('spark_theme', theme);
            document.cookie = `spark_theme=${theme};path=/;max-age=31536000;SameSite=Lax`;
            this.applyTheme(theme);
            this.updateCustomizerActiveState();
        },
        toggleTheme() {
            const currentIsDark = document.documentElement.classList.contains('dark');
            this.setTheme(currentIsDark ? 'light' : 'dark');
        },
        applyTheme(theme) {
            const html = document.documentElement;
            let isDark = false;
            if (theme === 'dark') {
                isDark = true;
            } else if (theme === 'light') {
                isDark = false;
            } else {
                isDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            }

            if (isDark) {
                html.classList.add('dark');
                html.setAttribute('data-theme', 'vristo-dark');
            } else {
                html.classList.remove('dark');
                html.setAttribute('data-theme', 'vristo-light');
            }
        },
        getStoredLanguage() {
            return localStorage.getItem('spark_locale') || getCookie('spark_locale') || 'zh';
        },
        setLanguage(lang) {
            localStorage.setItem('spark_locale', lang);
            document.cookie = `spark_locale=${lang};path=/;max-age=31536000;SameSite=Lax`;
            const currentUrl = window.location.pathname + window.location.search;
            window.location.href = `/change-language?culture=${lang}&returnUrl=${encodeURIComponent(currentUrl)}`;
        },
        updateLanguageUI(lang) {
            const current = (lang || this.getStoredLanguage()).toLowerCase();
            const code = current.toUpperCase();
            document.querySelectorAll('.current-lang-text').forEach(el => {
                el.textContent = code;
            });
            ['zh', 'en'].forEach(l => {
                const item = document.getElementById('lang-item-' + l);
                if (item) {
                    if (l === current) {
                        item.classList.add('bg-primary/10', 'text-primary', 'font-bold');
                    } else {
                        item.classList.remove('bg-primary/10', 'text-primary', 'font-bold');
                    }
                }
            });
        },
        toggleSidebar() {
            document.body.classList.toggle('toggle-sidebar');
            const isToggled = document.body.classList.contains('toggle-sidebar');
            localStorage.setItem('spark_sidebar_collapsed', isToggled ? 'true' : 'false');
        },
        closeSidebarMobile() {
            if (window.innerWidth < 1024) {
                document.body.classList.remove('toggle-sidebar');
            }
        },
        toggleCustomizer() {
            const drawer = document.getElementById('spark-theme-customizer');
            const overlay = document.getElementById('spark-customizer-overlay');
            if (!drawer) return;
            const isClosed = drawer.classList.contains('translate-x-full');
            if (isClosed) {
                drawer.classList.remove('translate-x-full');
                if (overlay) overlay.classList.remove('hidden');
                this.updateCustomizerActiveState();
            } else {
                drawer.classList.add('translate-x-full');
                if (overlay) overlay.classList.add('hidden');
            }
        },
        closeCustomizer() {
            const drawer = document.getElementById('spark-theme-customizer');
            const overlay = document.getElementById('spark-customizer-overlay');
            if (drawer) drawer.classList.add('translate-x-full');
            if (overlay) overlay.classList.add('hidden');
        },
        updateCustomizerActiveState() {
            const current = this.getStoredTheme();
            ['light', 'dark', 'system'].forEach(t => {
                const btn = document.getElementById('theme-btn-' + t);
                if (btn) {
                    if (t === current) {
                        btn.classList.add('border-primary', 'text-primary', 'bg-primary/10');
                    } else {
                        btn.classList.remove('border-primary', 'text-primary', 'bg-primary/10');
                    }
                }
            });
        },
        setNavbarType(type) {
            const header = document.querySelector('header');
            if (!header) return;
            if (type === 'static') {
                header.classList.remove('sticky', 'top-0');
                localStorage.setItem('spark_navbar', 'static');
            } else {
                header.classList.add('sticky', 'top-0');
                localStorage.setItem('spark_navbar', 'sticky');
            }
        },
        scrollToTop() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        },
        initScroll() {
            const btn = document.getElementById('btn-scroll-to-top');
            if (!btn) return;
            const onScroll = () => {
                if (window.scrollY > 150) {
                    btn.classList.remove('hidden');
                    btn.classList.add('flex');
                } else {
                    btn.classList.add('hidden');
                    btn.classList.remove('flex');
                }
            };
            window.removeEventListener('scroll', onScroll);
            window.addEventListener('scroll', onScroll, { passive: true });
            onScroll();
        },
        initLayout() {
            // 恢复侧边栏折叠状态
            if (window.innerWidth >= 1024 && localStorage.getItem('spark_sidebar_collapsed') === 'true') {
                document.body.classList.add('toggle-sidebar');
            }
            // 恢复导航栏模式
            if (localStorage.getItem('spark_navbar') === 'static') {
                const header = document.querySelector('header');
                if (header) header.classList.remove('sticky', 'top-0');
            }
            this.initScroll();
            this.updateLanguageUI();
            this.updateCustomizerActiveState();
        }
    };

    // Sidebar 手风琴折叠控制器 (Accordion Mode)
    const SidebarAccordion = {
        toggle(id) {
            const targetContainer = document.getElementById('group-container-' + id);
            const targetChevron = document.getElementById('group-chevron-' + id);
            if (!targetContainer) return;

            const isCurrentlyOpen = !targetContainer.classList.contains('hidden');

            // 手风琴模式：折叠其他所有分组
            document.querySelectorAll('.sidebar-group-children').forEach(el => {
                el.classList.add('hidden');
            });
            document.querySelectorAll('.sidebar-group-chevron').forEach(el => {
                el.classList.remove('rotate-90');
            });

            // 切换当前分组
            if (!isCurrentlyOpen) {
                targetContainer.classList.remove('hidden');
                if (targetChevron) targetChevron.classList.add('rotate-90');
            }
        },
        init() {
            const currentPath = window.location.pathname.toLowerCase();
            let matchedGroupId = null;

            document.querySelectorAll('.sidebar-group-children').forEach(groupEl => {
                const links = groupEl.querySelectorAll('a');
                links.forEach(a => {
                    const href = a.getAttribute('href');
                    if (href && href !== '/' && currentPath.startsWith(href.toLowerCase())) {
                        matchedGroupId = groupEl.getAttribute('data-group-id');
                    }
                });
            });

            // 默认收起全部，仅展开匹配当前路由的分组；若无匹配则展开第一个分组
            const allGroups = document.querySelectorAll('.sidebar-group-children');
            if (allGroups.length > 0) {
                allGroups.forEach(el => el.classList.add('hidden'));
                document.querySelectorAll('.sidebar-group-chevron').forEach(el => el.classList.remove('rotate-90'));

                const targetId = matchedGroupId || allGroups[0].getAttribute('data-group-id');
                if (targetId) {
                    const targetEl = document.getElementById('group-container-' + targetId);
                    const targetChev = document.getElementById('group-chevron-' + targetId);
                    if (targetEl) targetEl.classList.remove('hidden');
                    if (targetChev) targetChev.classList.add('rotate-90');
                }
            }
        }
    };

    // 立即执行深色模式防 FOUC
    ThemeManager.applyTheme(ThemeManager.getStoredTheme());

    window.themeManager = ThemeManager;
    window.sidebarAccordion = SidebarAccordion;

    function refreshSystemState() {
        // 关键：在 Blazor 切换路由后强力保持当前主题，杜绝回退到 light 模式
        ThemeManager.applyTheme(ThemeManager.getStoredTheme());
        ThemeManager.initLayout();
        SidebarAccordion.init();
    }

    document.addEventListener('DOMContentLoaded', refreshSystemState);
    // 监听 Blazor 增强页面跳转事件 (Enhanced Navigation)
    document.addEventListener('blazor:enhancedload', refreshSystemState);
    window.addEventListener('popstate', refreshSystemState);
})();
