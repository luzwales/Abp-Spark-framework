// Vristo theme & layout helper for Blazor with Morphicons & Lucide
import { createMorph } from 'morphicons/dom';
import { Sun, Moon } from 'lucide';

(function () {
    const ThemeManager = {
        morph: null,
        getStoredTheme() {
            return localStorage.getItem('spark_theme') || 'system';
        },
        setTheme(theme) {
            localStorage.setItem('spark_theme', theme);
            this.applyTheme(theme);
            this.updateCustomizerActiveState();
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
            this.updateThemeMorph(isDark);
        },
        initThemeMorph() {
            const pathEl = document.getElementById('theme-morph-path');
            if (!pathEl) return;
            const isDark = document.documentElement.classList.contains('dark');
            const targetIcon = isDark ? Sun : Moon;
            if (!this.morph) {
                this.morph = createMorph(pathEl, targetIcon, { strokeWidth: 2, spring: 'snappy' });
            } else {
                this.morph.set(targetIcon);
            }
        },
        updateThemeMorph(isDark) {
            const pathEl = document.getElementById('theme-morph-path');
            if (!pathEl) return;
            const targetIcon = isDark ? Sun : Moon;
            if (!this.morph) {
                this.morph = createMorph(pathEl, targetIcon, { strokeWidth: 2, spring: 'snappy' });
            } else {
                this.morph.morphTo(targetIcon);
            }
        },
        getStoredLanguage() {
            return localStorage.getItem('spark_locale') || 'zh';
        },
        setLanguage(lang) {
            localStorage.setItem('spark_locale', lang);
            document.cookie = `spark_locale=${lang};path=/;max-age=31536000;SameSite=Lax`;
            this.updateLanguageUI(lang);
            const dd = document.getElementById('lang-dropdown');
            if (dd) {
                dd.removeAttribute('open');
            }
            if (document.activeElement && typeof document.activeElement.blur === 'function') {
                document.activeElement.blur();
            }
            window.location.reload();
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
            // Restore desktop sidebar collapsed state
            if (window.innerWidth >= 1024 && localStorage.getItem('spark_sidebar_collapsed') === 'true') {
                document.body.classList.add('toggle-sidebar');
            }
            // Restore navbar type
            if (localStorage.getItem('spark_navbar') === 'static') {
                const header = document.querySelector('header');
                if (header) header.classList.remove('sticky', 'top-0');
            }
            this.initScroll();
            this.initThemeMorph();
            this.updateLanguageUI();
            this.updateCustomizerActiveState();
        }
    };

    // Apply stored theme immediately on parse to prevent FOUC
    ThemeManager.applyTheme(ThemeManager.getStoredTheme());

    window.themeManager = ThemeManager;
    document.addEventListener('DOMContentLoaded', () => {
        ThemeManager.initLayout();
    });
})();
