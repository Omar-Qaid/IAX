import React, { memo, useCallback, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Box,
  Avatar,
  Tooltip,
  Badge,
  useMediaQuery,
  useTheme,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Popover,
  Button,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import WaffleIcon from '@mui/icons-material/Apps';
import SearchIcon from '@mui/icons-material/Search';
import NotificationsIcon from '@mui/icons-material/NotificationsNoneOutlined';
import SettingsIcon from '@mui/icons-material/Settings';
import HelpIcon from '@mui/icons-material/HelpOutlined';
import AccountIcon from '@mui/icons-material/AccountCircle';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import LogoutIcon from '@mui/icons-material/Logout';
import TranslateIcon from '@mui/icons-material/Translate';
import { useTranslation } from 'react-i18next';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAuth } from '@core/auth/useAuth';
import {
  AVAILABLE_MODULE_NAV_CONFIGS,
  getModuleNavLinkPermission,
} from '@app/configuration/navigation';
import { LAYOUT } from '@app/configuration/constants';
import { useAppStore } from '@app/store/useAppStore';
import { topBarTokens as topBar } from './topBarTokens';
import { useNotificationStore } from '@shared/services/notificationStore';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { legalEntityApiRepository } from '@modules/organization/api/legalEntityApiRepository';
import { getRouteBreadcrumbs } from '@app/routes/routeMetadata';
import { useAppTranslation } from '@core/localization/useAppTranslation';

// Static sx objects - moved outside render to prevent re-creation
const appBarSx = {
  bgcolor: topBar.background,
  color: 'common.white',
  borderBottom: 'none',
  zIndex: (theme: { zIndex: { drawer: number } }) => theme.zIndex.drawer + 1,
} as const;

const toolbarSx = {
  minHeight: `${LAYOUT.TOPBARHEIGHT}px !important`,
  height: LAYOUT.TOPBARHEIGHT,
  px: '0 !important',
  gap: 0,
  fontFamily: topBar.fontFamily,
} as const;

const iconBtnSx = {
  color: 'rgba(255,255,255,0.85)',
  width: topBar.actionWidth,
  height: topBar.height,
  p: 0,
  borderRadius: 0,
  '&:hover': { bgcolor: topBar.hover },
} as const;

const hamburgerSx = { ...iconBtnSx, display: { xs: 'inline-flex', md: 'none' } } as const;
const waffleSx = { ...iconBtnSx, display: { xs: 'none', md: 'inline-flex' } } as const;
const helpSx = { ...iconBtnSx, display: { xs: 'none', sm: 'inline-flex' } } as const;

const titleSx = {
  color: 'common.white',
  fontWeight: 700,
  fontSize: { xs: '0.8125rem', sm: '0.875rem' },
  marginInlineEnd: { xs: 0.5, sm: 2 },
  letterSpacing: '0.02em',
  flexShrink: 0,
  display: { xs: 'none', sm: 'block' },
} as const;

const actionsSx = {
  display: 'flex',
  alignItems: 'center',
  gap: 0,
  flexShrink: 0,
} as const;

const badgeSx = {
  '& .MuiBadge-badge': {
    top: 2,
    right: -2,
    fontFamily: topBar.fontFamily,
    fontSize: 10,
    minWidth: 18,
    height: 18,
    bgcolor: '#ffffff',
    color: '#201f1e',
    border: `1px solid ${topBar.background}`,
  },
} as const;

export const AppTopBar: React.FC = memo(() => {
  const theme = useTheme();
  const inlineStart = theme.direction === 'rtl' ? 'right' : 'left';
  const inlineEnd = theme.direction === 'rtl' ? 'left' : 'right';
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const location = useLocation();
  const { t } = useTranslation();
  const { currentLanguage, changeLanguage } = useAppTranslation();
  const isTablet = useMediaQuery(theme.breakpoints.between('sm', 'md'));
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  // Zustand selectors
  const setCommandPaletteOpen = useNavigationStore((s) => s.setCommandPaletteOpen);
  const setSettingsPanelOpen = useNavigationStore((s) => s.setSettingsPanelOpen);
  const setNotificationDrawerOpen = useNavigationStore((s) => s.setNotificationDrawerOpen);
  const setSidebarOpen = useNavigationStore((s) => s.setSidebarOpen);
  const navLayout = usePreferenceStore((s) => s.navLayout);
  const setRtl = usePreferenceStore((s) => s.setRtl);
  const currentCompany = useAppStore((s) => s.currentCompany);
  const setCompany = useAppStore((s) => s.setCompany);
  const notificationCount = useNotificationStore((s) => s.notifications.length);

  // Auth & Permissions
  const { user, logout, hasPermission } = useAuth();
  const userName = user?.displayName || user?.username || user?.email;
  const isAdmin = user?.roles.includes('SystemAdmin') ?? false;

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const isMenuOpen = Boolean(anchorEl);
  const [companyAnchorEl, setCompanyAnchorEl] = useState<null | HTMLElement>(null);
  const legalEntities = useQuery({
    queryKey: ['legal-entities', 'company-switcher'],
    queryFn: ({ signal }) => legalEntityApiRepository.list(signal),
    staleTime: 60_000,
  });
  const selectedLegalEntity = legalEntities.data?.find(
    (entity) => entity.dataArea.toLocaleUpperCase() === currentCompany.toLocaleUpperCase()
  );

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
    void logout();
  }, [logout, handleMenuClose]);

  const handleToggleLanguage = useCallback(() => {
    const nextLanguage = currentLanguage.code === 'en' ? 'ar' : 'en';
    setRtl(nextLanguage === 'ar');
    void changeLanguage(nextLanguage);
  }, [changeLanguage, currentLanguage.code, setRtl]);

  const handleCompanyChange = useCallback((companyCode: string) => {
    setCompany(companyCode);
    setCompanyAnchorEl(null);
    void queryClient.invalidateQueries();
  }, [queryClient, setCompany]);

  const handleModuleClick = useCallback(
    (event: React.MouseEvent<HTMLElement>, moduleId: string) => {
      setActiveModule(moduleId);
      setAnchorElNav(event.currentTarget);
    },
    []
  );

  const handleNavClose = useCallback(() => {
    setActiveModule(null);
    setAnchorElNav(null);
  }, []);

  // Stable callbacks
  const openSidebar = useCallback(() => setSidebarOpen(true), [setSidebarOpen]);
  const openCommandPalette = useCallback(
    () => setCommandPaletteOpen(true),
    [setCommandPaletteOpen]
  );
  const openNotifications = useCallback(
    () => setNotificationDrawerOpen(true),
    [setNotificationDrawerOpen]
  );
  const openSettings = useCallback(() => setSettingsPanelOpen(true), [setSettingsPanelOpen]);

  const titleText = isTablet
    ? t('nav.app_title_short', 'Finance and Operations')
    : t('nav.finance_operations');
  const userTooltip = userName || t('common.account', 'Account');
  const userInitials = (userName || 'User')
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('');
  const breadcrumbs = getRouteBreadcrumbs(location.pathname, location.search).slice(1);
  const isHorizontal = navLayout === 'horizontal';

  return (
    <AppBar position="fixed" elevation={0} sx={appBarSx}>
      <Toolbar variant="dense" sx={toolbarSx}>
        <Box
          sx={{
            width: { xs: 48, md: topBar.launcherWidth },
            height: topBar.height,
            bgcolor: topBar.launcherBackground,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
            borderInlineEnd: `1px solid ${topBar.divider}`,
          }}
        >
          <IconButton size="small" onClick={openSidebar} sx={hamburgerSx}>
            <MenuIcon sx={{ fontSize: topBar.iconSize }} />
          </IconButton>
          <IconButton size="small" onClick={openSidebar} sx={waffleSx} aria-label={t('nav.app_launcher')}>
            <WaffleIcon sx={{ fontSize: 24 }} />
          </IconButton>
        </Box>

        <Box
          sx={{
            width: { sm: topBar.productWidth },
            height: topBar.height,
            px: '15px',
            display: { xs: 'none', sm: 'flex' },
            alignItems: 'center',
            flexShrink: 0,
            borderInlineEnd: `1px solid ${topBar.divider}`,
          }}
        >
          <Typography noWrap sx={{ ...titleSx, m: 0, fontFamily: topBar.fontFamily, fontSize: topBar.productFontSize }}>
            {titleText}
          </Typography>
        </Box>

        {!isHorizontal && !isMobile && breadcrumbs.length > 0 && (
          <Box component="nav" aria-label={t('common.breadcrumbs', 'Breadcrumbs')} sx={{ height: topBar.height, display: 'flex', alignItems: 'center', minWidth: 0, maxWidth: { md: 260, xl: 390 }, px: 1.5, gap: 0.5, flexShrink: 1, overflow: 'hidden' }}>
            {breadcrumbs.map((item, index) => <React.Fragment key={`${item.labelKey}-${index}`}>
              {index > 0 && <ChevronRightIcon sx={{ fontSize: 17, color: topBar.mutedText, flexShrink: 0 }} />}
              <Typography component={item.path ? 'button' : 'span'} onClick={item.path ? () => navigate(item.path!) : undefined} noWrap sx={{ appearance: 'none', border: 0, bgcolor: 'transparent', p: 0, color: topBar.text, fontFamily: topBar.fontFamily, fontSize: 12, cursor: item.path ? 'pointer' : 'default', overflow: 'hidden', textOverflow: 'ellipsis', '&:hover': item.path ? { textDecoration: 'underline' } : undefined }}>
                {t(item.labelKey)}
              </Typography>
            </React.Fragment>)}
          </Box>
        )}

        {/* Horizontal navigation tabs */}
        {isHorizontal && !isMobile && (
          <Box sx={{ display: 'flex', gap: 0.5, marginInlineStart: 2, alignItems: 'center', height: '100%' }}>
            <Button
              size="small"
              onClick={() => navigate('/dashboard')}
              sx={{
                color:
                  location.pathname === '/dashboard' || location.pathname === '/'
                    ? '#fff'
                    : 'rgba(255,255,255,0.75)',
                bgcolor:
                  location.pathname === '/dashboard' || location.pathname === '/'
                    ? 'rgba(255,255,255,0.1)'
                    : 'transparent',
                textTransform: 'none',
                fontWeight:
                  location.pathname === '/dashboard' || location.pathname === '/' ? 600 : 500,
                fontSize: '0.8125rem',
                borderRadius: '2px',
                px: 1.25,
                py: 0.5,
                minWidth: 'auto',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' },
              }}
            >
              {t('nav.home', 'Home')}
            </Button>
            {Object.entries(AVAILABLE_MODULE_NAV_CONFIGS).map(([key, config]) => {
              const hasAccess = (() => {
                if (isAdmin) return true;
                return config.sections.some((section) =>
                  section.links.some((link) => {
                    const permission = getModuleNavLinkPermission(link);
                    return permission !== undefined && hasPermission(permission);
                  })
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
                    bgcolor:
                      isActive || activeModule === key ? 'rgba(255,255,255,0.1)' : 'transparent',
                    textTransform: 'none',
                    fontWeight: isActive || activeModule === key ? 600 : 500,
                    fontSize: '0.8125rem',
                    borderRadius: '2px',
                    px: 1.25,
                    py: 0.5,
                    minWidth: 'auto',
                    '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' },
                  }}
                >
                  {t(config.label)}
                </Button>
              );
            })}
          </Box>
        )}

        <Box sx={{ flex: 1, minWidth: 40, px: { xs: 0.5, md: 2 }, display: 'flex', justifyContent: 'center' }}>
          <Button
            onClick={openCommandPalette}
            startIcon={<SearchIcon sx={{ fontSize: 18 }} />}
            aria-label={t('nav.global_search')}
            sx={{ display: { xs: 'none', md: 'flex' }, width: 'min(100%, 510px)', height: 30, justifyContent: 'flex-start', px: 1.5, color: '#fff', bgcolor: topBar.searchBackground, borderRadius: '3px', textTransform: 'none', fontFamily: topBar.fontFamily, fontSize: 13, fontWeight: 400, '&:hover': { bgcolor: '#36557f' }, '& .MuiButton-startIcon': { marginInlineEnd: 1, marginInlineStart: 0, color: '#fff' } }}
          >
            {t('nav.global_search')}
          </Button>
        </Box>

        {/* Action icons */}
        <Box sx={actionsSx}>
          <Button
            onClick={(event) => setCompanyAnchorEl(event.currentTarget)}
            aria-label={t('common.company', 'Company')}
            aria-controls={companyAnchorEl ? 'company-menu' : undefined}
            aria-haspopup="true"
            aria-expanded={companyAnchorEl ? 'true' : undefined}
            sx={{
              display: { xs: 'none', lg: 'flex' },
              alignItems: 'center',
              height: 30,
              maxWidth: 290,
              px: 1,
              mx: 0.5,
              bgcolor: '#f3f7ff',
              color: '#003b78',
              fontFamily: topBar.fontFamily,
              textTransform: 'none',
              borderRadius: '3px',
              border: '1px solid #c7d7ee',
              '&:hover': { bgcolor: '#fff' },
            }}
          >
            <Typography noWrap sx={{ fontFamily: 'inherit', fontSize: 12, color: 'inherit' }}>
              {currentCompany}{selectedLegalEntity?.name ? ` | ${selectedLegalEntity.name}` : ''}
            </Typography>
          </Button>

          <Menu
            id="company-menu"
            anchorEl={companyAnchorEl}
            open={Boolean(companyAnchorEl)}
            onClose={() => setCompanyAnchorEl(null)}
            anchorOrigin={{ vertical: 'bottom', horizontal: inlineStart }}
            transformOrigin={{ vertical: 'top', horizontal: inlineStart }}
            slotProps={{ paper: { sx: { mt: 0.25, minWidth: 380, maxHeight: 360, borderRadius: 0, border: 1, borderColor: 'divider' } } }}
          >
            {legalEntities.isLoading && <MenuItem disabled>{t('common.loading', 'Loading...')}</MenuItem>}
            {legalEntities.isError && <MenuItem disabled>{t('errors.generic', 'Unable to load legal entities.')}</MenuItem>}
            {(legalEntities.data ?? []).map((entity) => {
              const selected = entity.dataArea.toLocaleUpperCase() === currentCompany.toLocaleUpperCase();
              return <MenuItem key={entity.recId} selected={selected} onClick={() => handleCompanyChange(entity.dataArea)} sx={{ display: 'grid', gridTemplateColumns: '88px minmax(210px, 1fr)', gap: 1.5, borderInlineStart: selected ? 3 : 0, borderInlineStartColor: 'primary.main', '&.Mui-selected': { bgcolor: '#d4e0f7' } }}>
                <Typography noWrap sx={{ fontSize: 13, color: selected ? 'primary.main' : 'text.primary' }}>{entity.dataArea}</Typography>
                <Typography noWrap sx={{ fontSize: 13 }}>{entity.name}</Typography>
              </MenuItem>;
            })}
            {!legalEntities.isLoading && !legalEntities.isError && !legalEntities.data?.length && <MenuItem disabled>{t('common.noData', 'No legal entities found.')}</MenuItem>}
          </Menu>

          {/* Notifications */}
          <Tooltip title={t('common.notifications', 'Notifications')}>
            <IconButton size="small" onClick={openNotifications} sx={iconBtnSx}>
              <Badge badgeContent={notificationCount} max={99} sx={badgeSx}>
                <NotificationsIcon sx={{ fontSize: topBar.iconSize }} />
              </Badge>
            </IconButton>
          </Tooltip>

          {/* Settings */}
          <Tooltip title={t('common.settings', 'Settings')}>
            <IconButton size="small" onClick={openSettings} sx={{ ...iconBtnSx, borderInlineStart: `1px solid ${topBar.divider}` }}>
              <SettingsIcon sx={{ fontSize: topBar.iconSize }} />
            </IconButton>
          </Tooltip>

          {/* Help - hidden on mobile */}
          <Tooltip title={t('common.help', 'Help')}>
            <IconButton size="small" sx={helpSx}>
              <HelpIcon sx={{ fontSize: 23 }} />
            </IconButton>
          </Tooltip>

          {/* User */}
          <Tooltip title={userTooltip}>
            <IconButton
              size="small"
              sx={{
                width: 48,
                height: topBar.height,
                borderInlineStart: `1px solid ${topBar.divider}`,
                borderRadius: 0,
                color: topBar.text,
                '&:hover': { bgcolor: topBar.hover },
              }}
              onClick={handleProfileMenuOpen}
              aria-label={t('common.account', 'Account')}
              aria-controls={isMenuOpen ? 'account-menu' : undefined}
              aria-haspopup="true"
              aria-expanded={isMenuOpen ? 'true' : undefined}
            >
              <Avatar
                src={user?.avatarUrl}
                sx={{ width: topBar.avatarSize, height: topBar.avatarSize, bgcolor: '#c7dbf6', color: '#172b4d', fontFamily: topBar.fontFamily, fontSize: 15 }}
              >
                {userInitials}
              </Avatar>
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
                    marginInlineStart: -0.5,
                    marginInlineEnd: 1,
                  },
                  '&:before': {
                    content: '""',
                    display: 'block',
                    position: 'absolute',
                    top: 0,
                    [inlineEnd]: 14,
                    width: 10,
                    height: 10,
                    bgcolor: 'background.paper',
                    transform: 'translateY(-50%) rotate(45deg)',
                    zIndex: 0,
                  },
                },
              },
            }}
            transformOrigin={{ horizontal: inlineEnd, vertical: 'top' }}
            anchorOrigin={{ horizontal: inlineEnd, vertical: 'bottom' }}
          >
            <MenuItem onClick={handleMenuClose}>
              <ListItemIcon>
                <AccountIcon fontSize="small" color="primary" />
              </ListItemIcon>
              <ListItemText primary={userTooltip} />
            </MenuItem>
            <Divider />
            <MenuItem onClick={handleToggleLanguage}>
              <ListItemIcon>
                <TranslateIcon fontSize="small" color="primary" />
              </ListItemIcon>
              <ListItemText
                primary={currentLanguage.code === 'en' ? 'العربية' : 'English'}
                secondary={t('common.language', 'Language')}
              />
            </MenuItem>
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
            horizontal: inlineStart,
          }}
          transformOrigin={{
            vertical: 'top',
            horizontal: inlineStart,
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
              },
            },
          }}
        >
          <Box sx={{ display: 'flex', gap: 4, minWidth: 280 }}>
            {AVAILABLE_MODULE_NAV_CONFIGS[activeModule]?.sections?.map((section) => {
              const visibleLinks = section.links.filter((link) => {
                if (isAdmin) return true;
                const permission = getModuleNavLinkPermission(link);
                return permission ? hasPermission(permission) : true;
              });

              if (visibleLinks.length === 0) return null;

              return (
                <Box
                  key={section.id}
                  sx={{ display: 'flex', flexDirection: 'column', gap: 0.75, minWidth: 160 }}
                >
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
                      pb: 0.5,
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
                          color: 'primary.dark',
                        },
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
