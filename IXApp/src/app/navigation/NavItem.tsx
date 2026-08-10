import React from 'react';
import {
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Tooltip,
  Box,
  useTheme,
  Badge,
} from '@mui/material';
import FavoritesIcon from '@mui/icons-material/StarBorder';
import StarFilledIcon from '@mui/icons-material/Star';
import { navigationTokens as nav } from './navigationTokens';
import { usePreferenceStore } from '@app/store/usePreferenceStore';

export interface NavItemProps {
  icon?: React.ReactNode;
  label: string;
  collapsed?: boolean;
  active?: boolean;
  onClick?: () => void;
  isFavorite?: boolean;
  onToggleFavorite?: (e: React.MouseEvent) => void;
  showFavorite?: boolean;
  indent?: boolean;
  badge?: string | number;
}

export const NavItem = React.memo<NavItemProps>(
  ({
    icon,
    label,
    collapsed = false,
    active = false,
    onClick,
    isFavorite,
    onToggleFavorite,
    showFavorite,
    indent,
    badge,
  }) => {
    const theme = useTheme();
    const isDark = theme.palette.mode === 'dark';
    const apparent = usePreferenceStore((s) => s.navColor === 'apparent');
    const useDarkNavigation = isDark || apparent;

    // Color definitions
    const itemColor = active ? (useDarkNavigation ? '#ffffff' : nav.text) : useDarkNavigation ? '#f3f2f1' : nav.text;
    const iconColor = active ? (useDarkNavigation ? '#ffffff' : nav.icon) : useDarkNavigation ? '#c8c6c4' : nav.icon;
    const hoverBgColor = useDarkNavigation ? 'rgba(255,255,255,0.08)' : nav.hover;
    const activeBgColor = active ? (useDarkNavigation ? 'rgba(255,255,255,0.12)' : nav.selected) : 'transparent';

    return (
      <ListItemButton
        onClick={onClick}
        sx={{
          minHeight: indent ? 36 : nav.itemHeight,
          height: indent ? 36 : nav.itemHeight,
          px: collapsed ? 0 : indent ? 5.75 : `${nav.horizontalPadding}px`,
          justifyContent: collapsed ? 'center' : 'initial',
          bgcolor: activeBgColor,
          color: itemColor,
          '&:hover': {
            bgcolor: hoverBgColor,
            '& .fav-btn': { opacity: 1 },
            '& .item-label': { textDecoration: indent ? 'underline' : 'none' },
          },
          borderRadius: 0,
          fontFamily: nav.fontFamily,
          borderInlineStart: active && !indent ? '3px solid' : '3px solid transparent',
          borderColor: active ? nav.selectedBar : 'transparent',
        }}
      >
        {icon && (
          <Tooltip title={collapsed ? label : ''} placement="right">
            <ListItemIcon
              sx={{
                minWidth: 0,
                mr: collapsed ? 0 : `${nav.iconTextGap}px`,
                justifyContent: 'center',
                color: iconColor,
                '& svg': { fontSize: nav.iconSize },
              }}
            >
              {badge ? (
                <Badge badgeContent={badge} color="error">
                  {icon}
                </Badge>
              ) : (
                icon
              )}
            </ListItemIcon>
          </Tooltip>
        )}
        {!collapsed && (
          <>
            <ListItemText
              primary={label}
              slotProps={{
                primary: {
                  className: 'item-label',
                  sx: {
                    fontFamily: nav.fontFamily,
                    fontSize: nav.fontSize,
                    lineHeight: '20px',
                    fontWeight: nav.fontWeight,
                    color: 'inherit',
                  },
                },
              }}
            />
            {badge && !icon && <Badge badgeContent={badge} color="error" sx={{ mr: 2 }} />}
            {showFavorite && onToggleFavorite && (
              <Box
                className="fav-btn"
                onClick={onToggleFavorite}
                sx={{
                  opacity: isFavorite ? 1 : 0,
                  transition: 'opacity 0.2s',
                  color: isFavorite
                    ? 'warning.main'
                    : 'text.disabled',
                  '&:hover': { color: 'warning.dark' },
                }}
              >
                {isFavorite ? (
                  <StarFilledIcon sx={{ fontSize: 16 }} />
                ) : (
                  <FavoritesIcon sx={{ fontSize: 16 }} />
                )}
              </Box>
            )}
          </>
        )}
      </ListItemButton>
    );
  }
);

NavItem.displayName = 'NavItem';
