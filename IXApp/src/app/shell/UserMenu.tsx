import React, { useState } from 'react';
import {
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  Typography,
} from '@mui/material';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import TranslateIcon from '@mui/icons-material/Translate';
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined';
import LightModeOutlinedIcon from '@mui/icons-material/LightModeOutlined';
import LogoutIcon from '@mui/icons-material/Logout';
import { useAuth } from '@core/auth/useAuth';
import { usePreferenceStore } from '@app/store/usePreferenceStore';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';

export const UserMenu: React.FC = () => {
  const { user, logout } = useAuth();
  const { themeMode, toggleThemeMode, setRtl } = usePreferenceStore();
  const { currentLanguage, changeLanguage, t } = useAppTranslation();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleOpen = (e: React.MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget);
  const handleClose = () => setAnchorEl(null);

  const handleToggleLang = () => {
    const nextLanguage = currentLanguage.code === 'en' ? 'ar' : 'en';
    setRtl(nextLanguage === 'ar');
    void changeLanguage(nextLanguage);
    handleClose();
  };

  const handleOpenSettings = () => {
    handleClose();
    navigate(ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS);
  };

  return (
    <>
      <IconButton onClick={handleOpen} size="small" color="inherit">
        <Avatar
          sx={{
            width: 28,
            height: 28,
            bgcolor: 'primary.main',
            fontSize: '0.75rem',
            fontWeight: 700,
          }}
        >
          {user?.displayName.charAt(0) || 'U'}
        </Avatar>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        <MenuItem disabled sx={{ opacity: '1 !important' }}>
          <ListItemText
            primary={<Typography variant="subtitle2">{user?.displayName}</Typography>}
            secondary={
              <Typography variant="caption" color="text.secondary">
                {user?.email}
              </Typography>
            }
          />
        </MenuItem>
        <Divider />
        <MenuItem onClick={handleOpenSettings}>
          <ListItemIcon>
            <SettingsOutlinedIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>{t('nav.settings') || 'Settings'}</ListItemText>
        </MenuItem>
        <MenuItem onClick={handleToggleLang}>
          <ListItemIcon>
            <TranslateIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>
            {currentLanguage.code === 'en' ? 'العربية (Arabic)' : 'English'}
          </ListItemText>
        </MenuItem>
        <MenuItem onClick={toggleThemeMode}>
          <ListItemIcon>
            {themeMode === 'light' ? (
              <DarkModeOutlinedIcon fontSize="small" />
            ) : (
              <LightModeOutlinedIcon fontSize="small" />
            )}
          </ListItemIcon>
          <ListItemText>
            {themeMode === 'light'
              ? t('common.dark') || 'Dark Mode'
              : t('common.light') || 'Light Mode'}
          </ListItemText>
        </MenuItem>
        <Divider />
        <MenuItem onClick={() => void logout()}>
          <ListItemIcon>
            <LogoutIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText sx={{ color: 'error.main' }}>{t('common.logout')}</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
};
