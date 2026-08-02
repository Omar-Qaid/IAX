import React from 'react';
import { Box } from '@mui/material';
import { AppTopBar } from './AppTopBar';
import { AppSidebar } from './AppSidebar';
import { AppCommandPalette } from './AppCommandPalette';
import { AppNotificationDrawer } from './AppNotificationDrawer';
import { AppSettingsDrawer } from './AppSettingsDrawer';
import { PageBreadcrumbs } from '@shared/components/page/PageBreadcrumbs';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { useTheme, useMediaQuery } from '@mui/material';
import { SIDEBARWIDTH, SIDEBARCOLLAPSEDWIDTH } from './AppSidebar';
import { LAYOUT } from '@app/configuration/constants';

export const AppShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isDrawerOpen = useNavigationStore((s) => s.sidebarOpen);
  const navLayout = useNavigationStore((s) => s.navLayout);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const isMini = navLayout === 'mini';
  const isHorizontal = navLayout === 'horizontal';
  const finalSidebarWidth = isHorizontal ? 0 : (isMini ? SIDEBARCOLLAPSEDWIDTH : (isDrawerOpen ? SIDEBARWIDTH : SIDEBARCOLLAPSEDWIDTH));

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', overflow: 'hidden', bgcolor: 'background.default' }}>
      <AppTopBar />
      <Box sx={{ display: 'flex', flex: 1, overflow: 'hidden', position: 'relative', pt: `${LAYOUT.TOPBARHEIGHT}px` }}>
        {!isHorizontal && <AppSidebar />}
        <Box
          component="main"
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
