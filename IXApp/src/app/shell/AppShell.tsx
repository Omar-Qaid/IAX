import React from 'react';
import { Box } from '@mui/material';
import { AppTopBar } from './AppTopBar';
import { AppSidebar } from './AppSidebar';
import { AppCommandPalette } from './AppCommandPalette';
import { AppNotificationDrawer } from './AppNotificationDrawer';
import { AppSettingsDrawer } from './AppSettingsDrawer';
import { PageBreadcrumbs } from '@app/navigation/PageBreadcrumbs';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useTheme, useMediaQuery } from '@mui/material';
import { SIDEBARWIDTH, SIDEBARCOLLAPSEDWIDTH } from './AppSidebar';
import { LAYOUT } from '@app/configuration/constants';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const AppShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isDrawerOpen = useNavigationStore((s) => s.sidebarOpen);
  const navLayout = usePreferenceStore((s) => s.navLayout);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { t } = useAppTranslation();

  const isMini = navLayout === 'mini';
  const isHorizontal = navLayout === 'horizontal';
  const finalSidebarWidth = isHorizontal ? 0 : (isMini ? SIDEBARCOLLAPSEDWIDTH : (isDrawerOpen ? SIDEBARWIDTH : SIDEBARCOLLAPSEDWIDTH));

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', overflow: 'hidden', bgcolor: 'background.default' }}>
      <Box
        component="a"
        href="#main-content"
        sx={{ position: 'fixed', insetInlineStart: 8, top: -48, zIndex: 'tooltip', bgcolor: 'background.paper', color: 'primary.main', px: 2, py: 1, border: 1, borderColor: 'primary.main', '&:focus': { top: 8 } }}
      >
        {t('accessibility.skipToContent', 'Skip to main content')}
      </Box>
      <AppTopBar />
      <Box sx={{ display: 'flex', flex: 1, overflow: 'hidden', position: 'relative', pt: `${LAYOUT.TOPBARHEIGHT}px` }}>
        {!isHorizontal && <AppSidebar />}
        <Box
          component="main"
          id="main-content"
          tabIndex={-1}
          sx={{
            flexGrow: 1,
            minHeight: 0,
            p: 2,
            overflow: 'hidden',
            display: 'flex',
            flexDirection: 'column',
            width: isMobile || isHorizontal ? '100%' : `calc(100% - ${finalSidebarWidth}px)`,
            transition: theme.transitions.create(['margin', 'width'], {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
          }}
        >
          <PageBreadcrumbs />
          <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', position: 'relative' }}>
            {children}
          </Box>
        </Box>
      </Box>

      {/* Global Drawers & Dialogs */}
      <AppCommandPalette />
      <AppNotificationDrawer />
      <AppSettingsDrawer />
    </Box>
  );
};
