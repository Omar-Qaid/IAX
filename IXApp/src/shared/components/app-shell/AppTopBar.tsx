import React from 'react';
import { AppBar, Toolbar, Typography, IconButton, Box, Stack } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import AppsIcon from '@mui/icons-material/Apps';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import { useNavigationStore } from '@app/store/useNavigationStore';
import { CompanySelector } from './CompanySelector';
import { GlobalSearch } from './GlobalSearch';
import { UserMenu } from './UserMenu';
import { NotificationMenu } from './NotificationMenu';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';

export const AppTopBar: React.FC = () => {
  const toggleDrawer = useNavigationStore((s) => s.toggleDrawer);
  const navigate = useNavigate();

  return (
    <AppBar position="sticky" color="default" sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
      <Toolbar variant="dense" sx={{ justifyContent: 'space-between', gap: 1 }}>
        {/* Left section: Drawer Toggle + App Branding */}
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <IconButton size="small" edge="start" color="inherit" onClick={toggleDrawer} aria-label="toggle drawer">
            <MenuIcon fontSize="small" />
          </IconButton>
          <Box
            onClick={() => navigate(ROUTE_PATHS.DASHBOARD)}
            sx={{
              display: 'flex',
              alignItems: 'center',
              cursor: 'pointer',
              userSelect: 'none',
              gap: 0.75,
            }}
          >
            <AppsIcon color="primary" fontSize="small" />
            <Typography variant="h6" color="primary" sx={{ fontWeight: 700, letterSpacing: 0.5 }}>
              IXApp
            </Typography>
            <Typography
              variant="caption"
              sx={{
                display: { xs: 'none', md: 'inline-block' },
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                px: 0.75,
                py: 0.2,
                borderRadius: 0.5,
                fontWeight: 700,
                fontSize: '0.65rem',
              }}
            >
              D365 Enterprise
            </Typography>
          </Box>
        </Stack>

        {/* Center section: Global Search */}
        <Box sx={{ flexGrow: 1, display: 'flex', justifyContent: 'center', maxWidth: 400 }}>
          <GlobalSearch />
        </Box>

        {/* Right section: Company + Notifications + Settings + User Menu */}
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <CompanySelector />
          <NotificationMenu />
          <IconButton
            size="small"
            color="inherit"
            onClick={() => navigate(ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS)}
          >
            <SettingsOutlinedIcon fontSize="small" />
          </IconButton>
          <UserMenu />
        </Stack>
      </Toolbar>
    </AppBar>
  );
};
