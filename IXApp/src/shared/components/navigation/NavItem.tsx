import React from 'react';
import { ListItemButton, ListItemIcon, ListItemText, Tooltip, Box, useTheme, Badge } from '@mui/material';
import { StarBorder as FavoritesIcon, Star as StarFilledIcon } from '@mui/icons-material';
import { useNavigationStore } from '@app/store/useNavigationStore';

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

export const NavItem = React.memo<NavItemProps>(({
    icon, label, collapsed = false, active = false, onClick, isFavorite, onToggleFavorite, showFavorite, indent, badge
}) => {
    const theme = useTheme();
    const navColor = useNavigationStore((s) => s.navColor);
    const isApparent = navColor === 'apparent';
    const isDark = theme.palette.mode === 'dark';

    // Color definitions
    let itemColor = 'text.primary';
    let iconColor = 'text.secondary';
    let hoverBgColor = isDark ? 'rgba(255, 255, 255, 0.03)' : 'rgba(0, 0, 0, 0.02)';
    let activeBgColor = 'transparent';

    if (isApparent) {
        itemColor = active ? '#ffffff' : 'rgba(255, 255, 255, 0.7)';
        iconColor = active ? '#ffffff' : 'rgba(255, 255, 255, 0.5)';
        hoverBgColor = 'rgba(255, 255, 255, 0.06)';
        activeBgColor = 'rgba(255, 255, 255, 0.08)';
    } else {
        if (indent) {
            itemColor = active ? 'primary.main' : 'text.secondary';
        } else {
            itemColor = active ? 'primary.main' : 'text.primary';
        }
        iconColor = active ? 'primary.main' : 'text.secondary';
        activeBgColor = active ? (isDark ? 'rgba(255,255,255,0.02)' : 'rgba(99,102,241,0.04)') : 'transparent';
    }

    const borderLeftColor = isApparent ? 'primary.light' : 'primary.main';

    return (
        <ListItemButton
            onClick={onClick}
            sx={{
                minHeight: indent ? 36 : 44,
                px: collapsed ? 1.5 : (indent ? 6 : 2),
                justifyContent: collapsed ? 'center' : 'initial',
                bgcolor: activeBgColor,
                color: itemColor,
                '&:hover': {
                    bgcolor: hoverBgColor,
                    '& .fav-btn': { opacity: 1 },
                    '& .item-label': { textDecoration: indent ? 'underline' : 'none' },
                },
                borderRadius: 0,
                borderLeft: (active && !indent) ? '3px solid' : '3px solid transparent',
                borderColor: borderLeftColor,
            }}
        >
            {icon && (
                <Tooltip title={collapsed ? label : ''} placement="right">
                    <ListItemIcon
                        sx={{
                            minWidth: 0,
                            mr: collapsed ? 0 : 2,
                            justifyContent: 'center',
                            color: iconColor,
                            '& svg': { fontSize: 20 }
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
                                    fontSize: '0.875rem',
                                    fontWeight: (active && !indent) ? 600 : 400,
                                    color: 'inherit'
                                }
                            }
                        }}
                    />
                    {badge && !icon && (
                        <Badge badgeContent={badge} color="error" sx={{ mr: 2 }} />
                    )}
                    {showFavorite && onToggleFavorite && (
                        <Box
                            className="fav-btn"
                            onClick={onToggleFavorite}
                            sx={{
                                opacity: isFavorite ? 1 : 0,
                                transition: 'opacity 0.2s',
                                color: isFavorite ? 'warning.main' : (isApparent ? 'rgba(255,255,255,0.4)' : 'text.disabled'),
                                '&:hover': { color: 'warning.dark' }
                            }}
                        >
                            {isFavorite ? <StarFilledIcon sx={{ fontSize: 16 }} /> : <FavoritesIcon sx={{ fontSize: 16 }} />}
                        </Box>
                    )}
                </>
            )}
        </ListItemButton>
    );
});

NavItem.displayName = 'NavItem';
