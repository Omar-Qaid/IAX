import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
    Box, useTheme, useMediaQuery, SwipeableDrawer,
    IconButton, Divider
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import {
    Menu as MenuIcon,
    Home as HomeIcon,
    StarBorder as StarIcon,
    AccessTime as RecentIcon,
    ViewList as ModulesIcon,
    PushPin as PinIcon,
    Receipt as ReceiptIcon,
    Payments as PaymentsIcon,
    CorporateFare as CorporateIcon,
    Inventory as InventoryIcon,
    AccountTree as WorkflowIcon,
    AdminPanelSettings as AdminIcon,
    AccountBalance as LedgerIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { MODULE_NAV_CONFIGS } from '@app/configuration/navigation';
import { useAuth } from '@core/auth/useAuth';
import type { SvgIconComponent } from '@mui/icons-material';

const SIDEBAR_ICON_MAP: Record<string, SvgIconComponent> = {
    receipt: ReceiptIcon,
    payments: PaymentsIcon,
    ledger: LedgerIcon,
    corporate: CorporateIcon,
    inventory: InventoryIcon,
    workflow: WorkflowIcon,
    admin: AdminIcon,
    default: ModulesIcon
};

import ModuleNavPanel from './ModuleNavPanel';
import { NavItem, NavSection } from '../navigation';

export const SIDEBARWIDTH = 260;
export const SIDEBARCOLLAPSEDWIDTH = 40;

const PATH_LABELS: Record<string, string> = {
    '/dashboard':       'nav.dashboard',
    '/customers':       'nav.customers',
    '/customer-groups': 'nav.customer_groups',
    '/vendors':         'nav.vendors',
    '/vendor-groups':   'nav.vendor_groups',
    '/pointOfSale':     'nav.pos',
    '/workflow':        'nav.workflow',
    '/inventory':       'nav.inventory',
    '/departments':     'nav.organization',
    '/occupations':     'nav.organization',
    '/nationalities':   'nav.organization',
    '/genders':         'nav.organization',
};

function getNormalizedPath(path: string): string {
    const parts = path.split('/');
    if (parts.length > 2 && parts[1]) {
        return '/' + parts[1];
    }
    return path;
}

function getLabelForPath(path: string, t: (s: string) => string): string {
    const norm = getNormalizedPath(path);
    const key = PATH_LABELS[norm];
    return key ? t(key) : norm;
}

const SidebarContent = React.memo(({ collapsed, onToggle, onClose, pinned, onTogglePin, isMobileView = false, onModulePanel }: {
    collapsed: boolean;
    onToggle: () => void;
    onClose?: () => void;
    pinned: boolean;
    onTogglePin: () => void;
    isMobileView?: boolean;
    onModulePanel?: (moduleId: string | null) => void;
}) => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const favorites = useNavigationStore((s) => s.favorites);
    const recentPages = useNavigationStore((s) => s.recentPages);
    const toggleFavorite = useNavigationStore((s) => s.toggleFavorite);
    const addRecentPage = useNavigationStore((s) => s.addRecentPage);
    const { user, hasPermission } = useAuth();
    const isAdmin = user?.roles.includes('SystemAdmin') ?? false;

    const hasModuleAccess = (moduleId: string): boolean => {
        if (isAdmin) return true;
        const config = MODULE_NAV_CONFIGS[moduleId];
        if (!config) return false;
        return config.sections.some(section =>
            section.links.some(link =>
                link.permission !== undefined && hasPermission(link.permission)
            )
        );
    };

    const [expanded, setExpanded] = useState<Record<string, boolean>>({ modules: true, favorites: false, recent: false, workspaces: false });

    useEffect(() => {
        if (location.pathname !== '/login' && location.pathname !== '/') {
            const normalizedPath = getNormalizedPath(location.pathname);
            addRecentPage(normalizedPath, getLabelForPath(normalizedPath, t));
        }
    }, [location.pathname, addRecentPage, t]);

    const handleNavigate = (path: string, moduleId?: string) => {
        if (moduleId && MODULE_NAV_CONFIGS[moduleId]) {
            onModulePanel?.(moduleId);
            return;
        }
        navigate(path);
        if (isMobileView || !pinned) {
            onClose?.();
        }
    };

    const toggleSection = (id: string) => {
        if (collapsed) {
            // Clicking a section icon in collapsed mode: expand the sidebar and open that section
            onToggle();
            setExpanded((prev) => ({ ...prev, [id]: true }));
            return;
        }
        setExpanded((prev) => ({ ...prev, [id]: !prev[id] }));
    };

    const isFavorite = (path: string) => favorites.some((f) => f.path === path);

    const navColor = usePreferenceStore((s) => s.navColor);
    const theme = useTheme();
    const isApparent = navColor === 'apparent';
    const isDark = theme.palette.mode === 'dark';
    const sidebarBg = isApparent ? (isDark ? '#090d16' : '#0B1220') : 'background.paper';
    const borderRightColor = isApparent ? 'rgba(255, 255, 255, 0.08)' : 'divider';

    return (
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', bgcolor: sidebarBg, borderRight: '1px solid', borderColor: borderRightColor }}>
            {/* Hamburger + pin row */}
            <Box
                sx={{
                    height: 40,
                    display: 'flex',
                    alignItems: 'center',
                    px: collapsed ? 0 : 1,
                    justifyContent: collapsed ? 'center' : 'space-between',
                }}
            >
                <IconButton size="small" onClick={onToggle} sx={{ color: isApparent ? 'rgba(255, 255, 255, 0.75)' : 'text.primary' }}>
                    <MenuIcon sx={{ fontSize: 20 }} />
                </IconButton>
                {!isMobileView && !collapsed && (
                    <IconButton size="small" onClick={onTogglePin} sx={{ color: pinned ? 'primary.main' : (isApparent ? 'rgba(255, 255, 255, 0.5)' : 'text.secondary') }}>
                        <PinIcon sx={{ fontSize: 18, transform: pinned ? 'none' : 'rotate(45deg)' }} />
                    </IconButton>
                )}
            </Box>

            <Divider sx={{ borderColor: isApparent ? 'rgba(255, 255, 255, 0.08)' : 'divider' }} />

            <Box sx={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
                <Box sx={{ borderBottom: '1px solid', borderColor: isApparent ? 'rgba(255, 255, 255, 0.08)' : 'divider' }}>
                    <NavItem
                        icon={<HomeIcon />}
                        label={t('nav.home', 'Home')}
                        collapsed={collapsed}
                        active={location.pathname === '/dashboard' || location.pathname === '/'}
                        onClick={() => {
                            if (collapsed) {
                                onToggle();
                                return;
                            }
                            handleNavigate('/');
                        }}
                    />
                </Box>

                <NavSection label={t('nav.favorites', 'Favorites')} icon={<StarIcon />} collapsed={collapsed} expanded={!!expanded.favorites} onToggle={() => toggleSection('favorites')}>
                    {favorites.map((fav) => (
                        <NavItem
                            key={fav.path}
                            icon={<StarIcon sx={{ color: 'warning.main' }} />}
                            label={fav.label}
                            collapsed={collapsed}
                            active={location.pathname === fav.path}
                            onClick={() => handleNavigate(fav.path)}
                            isFavorite={true}
                            onToggleFavorite={(e) => { e.stopPropagation(); toggleFavorite(fav.path, fav.label); }}
                            showFavorite={!collapsed}
                            indent
                        />
                    ))}
                </NavSection>

                <NavSection label={t('nav.recent', 'Recent')} icon={<RecentIcon />} collapsed={collapsed} expanded={!!expanded.recent} onToggle={() => toggleSection('recent')}>
                    {recentPages.map((page) => (
                        <NavItem
                            key={page.path}
                            icon={<RecentIcon />}
                            label={page.label}
                            collapsed={collapsed}
                            active={location.pathname === page.path}
                            onClick={() => handleNavigate(page.path)}
                            isFavorite={isFavorite(page.path)}
                            onToggleFavorite={(e) => { e.stopPropagation(); toggleFavorite(page.path, page.label); }}
                            showFavorite={!collapsed}
                            indent
                        />
                    ))}
                </NavSection>

                <NavSection label={t('nav.modules', 'Modules')} icon={<ModulesIcon />} collapsed={collapsed} expanded={!!expanded.modules} onToggle={() => toggleSection('modules')}>
                    {Object.values(MODULE_NAV_CONFIGS).map(config => {
                        if (!hasModuleAccess(config.moduleId)) return null;
                        const Icon = SIDEBAR_ICON_MAP[config.icon] || SIDEBAR_ICON_MAP['default'];
                        return (
                            <NavItem
                                key={config.moduleId}
                                icon={<Icon />}
                                label={t(config.label)}
                                collapsed={collapsed}
                                active={location.pathname.startsWith(config.matchPath)}
                                onClick={() => handleNavigate(config.defaultPath, config.moduleId)}
                                indent
                            />
                        );
                    })}
                </NavSection>
            </Box>
        </Box>
    );
});

SidebarContent.displayName = 'SidebarContent';

export const AppSidebar = () => {
    const { t } = useTranslation();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    const sidebarOpen = useNavigationStore((s) => s.sidebarOpen);
    const sidebarPinned = useNavigationStore((s) => s.sidebarPinned);
    const setSidebarOpen = useNavigationStore((s) => s.setSidebarOpen);
    const toggleSidebarPinned = useNavigationStore((s) => s.toggleSidebarPinned);
    // Must be read before any early return below — hooks cannot run conditionally.
    const navLayout = usePreferenceStore((s) => s.navLayout);

    const [activeModulePanel, setActiveModulePanel] = useState<string | null>(null);
    const navRef = useRef<HTMLElement | null>(null);

    const toggleSidebar = useCallback(() => setSidebarOpen(!sidebarOpen), [sidebarOpen, setSidebarOpen]);

    useEffect(() => {
        if (isMobile || !sidebarOpen || sidebarPinned) return;
        const handlePointerDown = (e: MouseEvent | TouchEvent) => {
            const target = e.target as Node | null;
            if (!target) return;
            if (navRef.current && navRef.current.contains(target)) return;
            setSidebarOpen(false);
            setActiveModulePanel(null);
        };
        document.addEventListener('mousedown', handlePointerDown);
        document.addEventListener('touchstart', handlePointerDown);
        return () => {
            document.removeEventListener('mousedown', handlePointerDown);
            document.removeEventListener('touchstart', handlePointerDown);
        };
    }, [isMobile, sidebarOpen, sidebarPinned, setSidebarOpen]);

    if (isMobile) {
        return (
            <>
                <SwipeableDrawer
                    anchor={theme.direction === 'rtl' ? 'right' : 'left'}
                    open={sidebarOpen}
                    onOpen={() => setSidebarOpen(true)}
                    onClose={() => setSidebarOpen(false)}
                    sx={{ '& .MuiDrawer-paper': { width: '85%', maxWidth: 320, borderRadius: 0 } }}
                >
                    <SidebarContent
                        collapsed={false}
                        onToggle={() => setSidebarOpen(false)}
                        onClose={() => setSidebarOpen(false)}
                        pinned={false}
                        onTogglePin={() => {}}
                        isMobileView={true}
                        onModulePanel={setActiveModulePanel}
                    />
                    {activeModulePanel && (
                        <ModuleNavPanel
                            title={t(MODULE_NAV_CONFIGS[activeModulePanel]?.label || activeModulePanel)}
                            sections={MODULE_NAV_CONFIGS[activeModulePanel]?.sections || []}
                            onClose={() => setActiveModulePanel(null)}
                            onBack={() => setActiveModulePanel(null)}
                            leftOffset={0}
                            isMobileView={true}
                        />
                    )}
                </SwipeableDrawer>
            </>
        );
    }

    if (navLayout === 'horizontal') {
        return null;
    }

    const isMini = navLayout === 'mini';
    const finalSidebarWidth = isMini ? SIDEBARCOLLAPSEDWIDTH : (sidebarOpen ? SIDEBARWIDTH : SIDEBARCOLLAPSEDWIDTH);
    const isSidebarCollapsed = isMini || !sidebarOpen;

    return (
        <Box
            component="nav"
            ref={navRef}
            sx={{
                width: finalSidebarWidth,
                flexShrink: 0,
                height: '100%',
                position: 'sticky',
                top: 0,
                transition: theme.transitions.create('width', {
                    easing: theme.transitions.easing.sharp,
                    duration: theme.transitions.duration.enteringScreen,
                }),
                zIndex: theme.zIndex.drawer,
            }}
        >
            <SidebarContent
                collapsed={isSidebarCollapsed}
                onToggle={toggleSidebar}
                pinned={sidebarPinned && !isMini}
                onTogglePin={toggleSidebarPinned}
                onModulePanel={setActiveModulePanel}
            />

            {activeModulePanel && (
                <ModuleNavPanel
                    title={t(MODULE_NAV_CONFIGS[activeModulePanel]?.label || activeModulePanel)}
                    sections={MODULE_NAV_CONFIGS[activeModulePanel]?.sections || []}
                    onClose={() => setActiveModulePanel(null)}
                    leftOffset={finalSidebarWidth}
                />
            )}
        </Box>
    );
};
