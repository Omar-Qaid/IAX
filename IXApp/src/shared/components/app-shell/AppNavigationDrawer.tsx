import React from 'react';
import { Drawer, Box, useMediaQuery, useTheme } from '@mui/material';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { ModuleNavigation } from './ModuleNavigation';

const DRAWER_WIDTH = 260;

export const AppNavigationDrawer: React.FC = () => {
  const isDrawerOpen = useNavigationStore((s) => s.isDrawerOpen);
  const setDrawerOpen = useNavigationStore((s) => s.setDrawerOpen);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  if (isMobile) {
    return (
      <Drawer
        variant="temporary"
        open={isDrawerOpen}
        onClose={() => setDrawerOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
          },
        }}
      >
        <ModuleNavigation />
      </Drawer>
    );
  }

  return (
    <Drawer
      variant="persistent"
      anchor="left"
      open={isDrawerOpen}
      sx={{
        width: isDrawerOpen ? DRAWER_WIDTH : 0,
        flexShrink: 0,
        transition: theme.transitions.create('width', {
          easing: theme.transitions.easing.sharp,
          duration: theme.transitions.duration.enteringScreen,
        }),
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
          top: 45, // Top bar height
          height: 'calc(100vh - 45px)',
          borderRight: `1px solid ${theme.palette.divider}`,
        },
      }}
    >
      <Box sx={{ overflowY: 'auto', height: '100%' }}>
        <ModuleNavigation />
      </Box>
    </Drawer>
  );
};
