import React from 'react';
import { Box } from '@mui/material';
import { AppTopBar } from './AppTopBar';
import { AppNavigationDrawer } from './AppNavigationDrawer';
import { PageBreadcrumbs } from '@shared/components/page/PageBreadcrumbs';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { useTheme, useMediaQuery } from '@mui/material';

const DRAWER_WIDTH = 260;

export const AppShell: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isDrawerOpen = useNavigationStore((s) => s.isDrawerOpen);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh', bgcolor: 'background.default' }}>
      <AppTopBar />
      <Box sx={{ display: 'flex', flex: 1, position: 'relative' }}>
        <AppNavigationDrawer />
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: 2,
            width: isMobile || !isDrawerOpen ? '100%' : `calc(100% - ${DRAWER_WIDTH}px)`,
            transition: theme.transitions.create('margin', {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
          }}
        >
          <PageBreadcrumbs />
          {children}
        </Box>
      </Box>
    </Box>
  );
};
