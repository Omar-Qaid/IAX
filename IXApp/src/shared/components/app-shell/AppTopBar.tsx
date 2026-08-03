import React, { memo, useCallback, useState, useMemo } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
    AppBar, Toolbar, Typography, IconButton, Box,
    InputBase, Tooltip, Badge, useMediaQuery, useTheme,
    Menu, MenuItem, ListItemIcon, ListItemText, Divider,
    Popover, Button
} from '@mui/material';
import {
    Menu as MenuIcon,
    Apps as WaffleIcon,
    Search as SearchIcon,
    Notifications as NotificationsIcon,
    Settings as SettingsIcon,
    HelpOutlined as HelpIcon,
    AccountCircle as AccountIcon,
    Logout as LogoutIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAuth } from '@core/auth/useAuth';
import { usePermissions } from '@core/auth/usePermissions';
import { MODULE_NAV_CONFIGS } from '@app/configuration/navigation';
import { LAYOUT } from '@app/configuration/constants';

// Static sx objects - moved outside render to prevent re-creation
const appBarSx = {
    bgcolor: 'primary.dark',
    color: 'common.white',
    borderBottom: 'none',
    zIndex: (theme: { zIndex: { drawer: number } }) => theme.zIndex.drawer + 1,
} as const;

const toolbarSx = {
    minHeight: LAYOUT.TOPBARHEIGHT,
    px: { xs: 0.5, sm: 1.5 },
    gap: 0.5,
} as const;

const iconBtnSx = {
    color: 'rgba(255,255,255,0.85)',
    p: { xs: '8px', sm: '6px' },
} as const;

const hamburgerSx = { ...iconBtnSx, display: { xs: 'inline-flex', md: 'none' } } as const;
const waffleSx = { ...iconBtnSx, mr: 0.5, display: { xs: 'none', md: 'inline-flex' } } as const;
const searchMobileSx = { ...iconBtnSx, display: { xs: 'inline-flex', sm: 'none' } } as const;
const helpSx = { ...iconBtnSx, display: { xs: 'none', sm: 'inline-flex' } } as const;

const titleSx = {
    color: 'common.white',
    fontWeight: 700,
    fontSize: { xs: '0.8125rem', sm: '0.875rem' },
    mr: { xs: 0.5, sm: 2 },
    letterSpacing: '0.02em',
    flexShrink: 0,
    display: { xs: 'none', sm: 'block' },
} as const;

const searchWrapperSx = {
    flex: 1,
    display: { xs: 'none', sm: 'flex' },
    justifyContent: 'center',
    maxWidth: { sm: 300, md: 600 },
    mx: 'auto',
} as const;

const searchBarSx = {
    display: 'flex',
    alignItems: 'center',
    bgcolor: 'rgba(255,255,255,0.12)',
    borderRadius: 0,
    px: 1.5,
    py: 0.25,
    width: '100%',
    maxWidth: 480,
    '&:hover': { bgcolor: 'rgba(255,255,255,0.18)' },
    '&:focus-within': { bgcolor: 'rgba(255,255,255,0.22)' },
    transition: 'background-color 0.2s',
} as const;

const inputSx = {
    flex: 1,
    color: '#ffffff',
    fontSize: '0.8125rem',
} as const;

const actionsSx = {
    display: 'flex',
    alignItems: 'center',
    gap: { xs: 0, sm: 0.25 },
    flexShrink: 0,
} as const;

const badgeSx = {
    '& .MuiBadge-badge': { fontSize: '0.625rem', minWidth: 16, height: 16 },
} as const;

export const AppTopBar: React.FC = memo(() => {
    const theme = useTheme();
    const navigate = useNavigate();
    const location = useLocation();
    const { t } = useTranslation();
    const isTablet = useMediaQuery(theme.breakpoints.between('sm', 'md'));
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));

    // Zustand selectors
    const setCommandPaletteOpen = useNavigationStore((s) => s.setCommandPaletteOpen);
    const setSettingsPanelOpen = useNavigationStore((s) => s.setSettingsPanelOpen);
    const setNotificationDrawerOpen = useNavigationStore((s) => s.setNotificationDrawerOpen);
    const setSidebarOpen = useNavigationStore((s) => s.setSidebarOpen);
    const navLayout = usePreferenceStore((s) => s.navLayout);
    
    // Auth & Permissions
    const { user, logout } = useAuth();
    const userName = (user as any)?.name || (user as any)?.firstName || user?.email;
    const { canView, isAdmin } = usePermissions();

    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const isMenuOpen = Boolean(anchorEl);

    const [activeModule, setActiveModule] = useState<string | null>(null);
    const [anchorElNav, setAnchorElNav] = useState<null | HTMLElement>(null);

    const handleProfileMenuOpen = useCallback((event: React.MouseEvent<HTMLElement>) => {
        setAnchorEl(event.currentTarget);
    }, []);

    const handleMenuClose = useCallback(() => {
        setAnchorEl(null);
    }, []);

    const handleLogout = useCallback(() => {
        handleMenuClose();
        logout();
    }, [logout, handleMenuClose]);

    const handleModuleClick = useCallback((event: React.MouseEvent<HTMLElement>, moduleId: string) => {
        setActiveModule(moduleId);
        setAnchorElNav(event.currentTarget);
    }, []);

    const handleNavClose = useCallback(() => {
        setActiveModule(null);
        setAnchorElNav(null);
    }, []);

    // Stable callbacks
    const openSidebar = useCallback(() => setSidebarOpen(true), [setSidebarOpen]);
    const openCommandPalette = useCallback(() => setCommandPaletteOpen(true), [setCommandPaletteOpen]);
    const openNotifications = useCallback(() => setNotificationDrawerOpen(true), [setNotificationDrawerOpen]);
    const openSettings = useCallback(() => setSettingsPanelOpen(true), [setSettingsPanelOpen]);

    const titleText = isTablet ? t('nav.app_title_short', 'IX App') : t('nav.app_title', 'IX App');
    const userTooltip = userName || t('common.account', 'Account');

    const isHorizontal = navLayout === 'horizontal';

    const currentSearchWrapperSx = useMemo(() => ({
        ...searchWrapperSx,
        maxWidth: isHorizontal ? { sm: 140, md: 200, lg: 280 } : { sm: 300, md: 600 },
        mr: isHorizontal ? 2 : 'auto',
        ml: isHorizontal ? 2 : 'auto',
    }), [isHorizontal]);

    return (
        <AppBar position="fixed" elevation={0} sx={appBarSx}>
            <Toolbar variant="dense" sx={toolbarSx}>
                {/* Hamburger - mobile/tablet only */}
                <IconButton size="small" onClick={openSidebar} sx={hamburgerSx}>
                    <MenuIcon sx={{ fontSize: 22 }} />
                </IconButton>

                {/* Waffle icon - desktop only */}
                <IconButton size="small" sx={waffleSx}>
                    <WaffleIcon sx={{ fontSize: 22 }} />
                </IconButton>

                {/* App title - hidden on mobile */}
                <Typography variant="subtitle2" noWrap sx={titleSx}>
                    {titleText}
                </Typography>

                {/* Horizontal navigation tabs */}
                {isHorizontal && !isMobile && (
                    <Box sx={{ display: 'flex', gap: 0.5, ml: 2, alignItems: 'center', height: '100%' }}>
                        <Button
                            size="small"
                            onClick={() => navigate('/dashboard')}
                            sx={{
                                color: location.pathname === '/dashboard' || location.pathname === '/' ? '#fff' : 'rgba(255,255,255,0.75)',
                                bgcolor: location.pathname === '/dashboard' || location.pathname === '/' ? 'rgba(255,255,255,0.1)' : 'transparent',
                                textTransform: 'none',
                                fontWeight: location.pathname === '/dashboard' || location.pathname === '/' ? 600 : 500,
                                fontSize: '0.8125rem',
                                borderRadius: '2px',
                                px: 1.25,
                                py: 0.5,
                                minWidth: 'auto',
                                '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' }
                            }}
                        >
                            {t('nav.home', 'Home')}
                        </Button>
                        {Object.entries(MODULE_NAV_CONFIGS).map(([key, config]) => {
                            const hasAccess = (() => {
                                if (isAdmin) return true;
                                return config.sections.some(section =>
                                    section.links.some(link =>
                                        link.permission !== undefined && canView(link.permission.module, link.permission.resource)
                                    )
                                );
                            })();
                            
                            if (!hasAccess) return null;
                            
                            const isActive = location.pathname.startsWith(config.matchPath);
                            
                            return (
                                <Button
                                    key={key}
                                    size="small"
                                    onClick={(e) => handleModuleClick(e, key)}
                                    sx={{
                                        color: isActive || activeModule === key ? '#fff' : 'rgba(255,255,255,0.75)',
                                        bgcolor: isActive || activeModule === key ? 'rgba(255,255,255,0.1)' : 'transparent',
                                        textTransform: 'none',
                                        fontWeight: isActive || activeModule === key ? 600 : 500,
                                        fontSize: '0.8125rem',
                                        borderRadius: '2px',
                                        px: 1.25,
                                        py: 0.5,
                                        minWidth: 'auto',
                                        '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' }
                                    }}
                                >
                                    {t(config.label)}
                                </Button>
                            );
                        })}
                    </Box>
                )}

                {/* Search bar - hidden on mobile, shown on sm+ */}
                <Box sx={currentSearchWrapperSx}>
                    <Box sx={searchBarSx}>
                        <SearchIcon sx={{ fontSize: 18, color: 'rgba(255,255,255,0.7)', mr: 1 }} />
                        <InputBase
                            placeholder={t('nav.search_page', 'Search...')}
                            onClick={openCommandPalette}
                            onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                                if (e.key === 'Enter' && e.currentTarget.value) {
                                    e.preventDefault();
                                    navigate(`/search?q=${encodeURIComponent(e.currentTarget.value)}`);
                                }
                            }}
                            sx={inputSx}
                            readOnly
                        />
                    </Box>
                </Box>

                {/* Spacer on mobile */}
                <Box sx={{ flex: 1, display: { xs: 'block', sm: 'none' } }} />

                {/* Action icons */}
                <Box sx={actionsSx}>
                    {/* Search icon - mobile only */}
                    <Tooltip title={t('common.search', 'Search')}>
                        <IconButton size="small" onClick={openCommandPalette} sx={searchMobileSx}>
                            <SearchIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>

                    {/* Notifications */}
                    <Tooltip title={t('common.notifications', 'Notifications')}>
                        <IconButton size="small" onClick={openNotifications} sx={iconBtnSx}>
                            <Badge badgeContent={0} color="error" sx={badgeSx}>
                                <NotificationsIcon sx={{ fontSize: 20 }} />
                            </Badge>
                        </IconButton>
                    </Tooltip>

                    {/* Settings */}
                    <Tooltip title={t('common.settings', 'Settings')}>
                        <IconButton size="small" onClick={openSettings} sx={iconBtnSx}>
                            <SettingsIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>

                    {/* Help - hidden on mobile */}
                    <Tooltip title={t('common.help', 'Help')}>
                        <IconButton size="small" sx={helpSx}>
                            <HelpIcon sx={{ fontSize: 20 }} />
                        </IconButton>
                    </Tooltip>

                    {/* User */}
                    <Tooltip title={userTooltip}>
                        <IconButton 
                            size="small" 
                            sx={iconBtnSx}
                            onClick={handleProfileMenuOpen}
                            aria-controls={isMenuOpen ? 'account-menu' : undefined}
                            aria-haspopup="true"
                            aria-expanded={isMenuOpen ? 'true' : undefined}
                        >
                            <AccountIcon sx={{ fontSize: 22 }} />
                        </IconButton>
                    </Tooltip>
                    
                    {/* Profile Menu */}
                    <Menu
                        id="account-menu"
                        anchorEl={anchorEl}
                        open={isMenuOpen}
                        onClose={handleMenuClose}
                        onClick={handleMenuClose}
                        slotProps={{
                            paper: {
                                elevation: 0,
                                sx: {
                                    overflow: 'visible',
                                    filter: 'drop-shadow(0px 2px 8px rgba(0,0,0,0.32))',
                                    mt: 1.5,
                                    minWidth: 150,
                                    '& .MuiAvatar-root': {
                                        width: 32,
                                        height: 32,
                                        ml: -0.5,
                                        mr: 1,
                                    },
                                    '&:before': {
                                        content: '""',
                                        display: 'block',
                                        position: 'absolute',
                                        top: 0,
                                        right: 14,
                                        width: 10,
                                        height: 10,
                                        bgcolor: 'background.paper',
                                        transform: 'translateY(-50%) rotate(45deg)',
                                        zIndex: 0,
                                    },
                                },
                            }
                        }}
                        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                    >
                        <MenuItem onClick={handleMenuClose}>
                            <ListItemIcon>
                                <AccountIcon fontSize="small" color="primary" />
                            </ListItemIcon>
                            <ListItemText primary={userTooltip} />
                        </MenuItem>
                        <Divider />
                        <MenuItem onClick={handleLogout}>
                            <ListItemIcon>
                                <LogoutIcon fontSize="small" color="error" />
                            </ListItemIcon>
                            <ListItemText primary={t('common.logout', 'Logout')} sx={{ color: 'error.main' }} />
                        </MenuItem>
                    </Menu>
                </Box>
            </Toolbar>

            {/* Popover Mega-menu for Horizontal Layout */}
            {isHorizontal && !isMobile && activeModule && (
                <Popover
                    open={Boolean(anchorElNav)}
                    anchorEl={anchorElNav}
                    onClose={handleNavClose}
                    anchorOrigin={{
                        vertical: 'bottom',
                        horizontal: 'left',
                    }}
                    transformOrigin={{
                        vertical: 'top',
                        horizontal: 'left',
                    }}
                    slotProps={{
                        paper: {
                            sx: {
                                mt: 1,
                                borderRadius: '4px',
                                boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
                                border: '1px solid',
                                borderColor: 'divider',
                                p: 3,
                                bgcolor: 'background.paper',
                                maxHeight: '80vh',
                                overflowY: 'auto',
                            }
                        }
                    }}
                >
                    <Box sx={{ display: 'flex', gap: 4, minWidth: 280 }}>
                        {MODULE_NAV_CONFIGS[activeModule]?.sections?.map((section) => {
                            const visibleLinks = section.links.filter(link => {
                                if (isAdmin) return true;
                                if (!link.permission) return false;
                                return canView(link.permission.module, link.permission.resource);
                            });
                            
                            if (visibleLinks.length === 0) return null;
                            
                            return (
                                <Box key={section.id} sx={{ display: 'flex', flexDirection: 'column', gap: 0.75, minWidth: 160 }}>
                                    <Typography 
                                        sx={{ 
                                            fontSize: '0.6875rem', 
                                            fontWeight: 700, 
                                            color: 'text.secondary', 
                                            textTransform: 'uppercase', 
                                            letterSpacing: '0.06em', 
                                            mb: 1,
                                            borderBottom: '1px solid',
                                            borderColor: 'divider',
                                            pb: 0.5
                                        }}
                                    >
                                        {t(section.title)}
                                    </Typography>
                                    {visibleLinks.map((link) => (
                                        <Box
                                            key={link.label}
                                            onClick={() => {
                                                if (link.path) {
                                                    navigate(link.path);
                                                    handleNavClose();
                                                }
                                            }}
                                            sx={{
                                                fontSize: '0.8125rem',
                                                color: 'primary.main',
                                                cursor: 'pointer',
                                                py: 0.5,
                                                px: 0.5,
                                                borderRadius: '2px',
                                                transition: 'all 0.15s',
                                                '&:hover': {
                                                    bgcolor: 'rgba(99,102,241,0.06)',
                                                    textDecoration: 'underline',
                                                    color: 'primary.dark'
                                                }
                                            }}
                                        >
                                            {t(link.label)}
                                        </Box>
                                    ))}
                                </Box>
                            );
                        })}
                    </Box>
                </Popover>
            )}
        </AppBar>
    );
});

AppTopBar.displayName = 'AppTopBar';
