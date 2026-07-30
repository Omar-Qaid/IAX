import React from 'react';
import { ListItemButton, ListItemIcon, ListItemText, Typography } from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';

export interface NavigationItemProps {
  label: string;
  path: string;
  icon?: React.ReactNode;
  badge?: string;
  onClick?: () => void;
}

export const NavigationItem: React.FC<NavigationItemProps> = ({ label, path, icon, onClick }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const isSelected = location.pathname === path || (path !== '/' && location.pathname.startsWith(path));

  const handleClick = () => {
    navigate(path);
    if (onClick) onClick();
  };

  return (
    <ListItemButton
      selected={isSelected}
      onClick={handleClick}
      sx={{
        py: 0.5,
        px: 2,
        borderRadius: 1,
        mb: 0.25,
        '&.Mui-selected': {
          bgcolor: (theme) => (theme.palette.mode === 'light' ? '#e5f3ff' : '#004e8c'),
          color: 'primary.main',
          fontWeight: 700,
          borderLeft: (theme) => `3px solid ${theme.palette.primary.main}`,
        },
      }}
    >
      {icon && (
        <ListItemIcon sx={{ minWidth: 28, color: isSelected ? 'primary.main' : 'text.secondary' }}>
          {icon}
        </ListItemIcon>
      )}
      <ListItemText
        primary={
          <Typography
            variant="body2"
            sx={{ fontWeight: isSelected ? 700 : 400, fontSize: '0.8125rem' }}
          >
            {label}
          </Typography>
        }
      />
    </ListItemButton>
  );
};
