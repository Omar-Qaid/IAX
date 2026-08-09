import React from 'react';
import { Box, Typography, Collapse, useTheme } from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { usePreferenceStore } from '@app/store/usePreferenceStore';

export interface NavSectionProps {
  label: string;
  icon?: React.ReactNode;
  collapsed?: boolean;
  expanded?: boolean;
  onToggle?: () => void;
  children?: React.ReactNode;
}

export const NavSection = React.memo<NavSectionProps>(
  ({ label, icon, collapsed = false, expanded = true, onToggle, children }) => {
    const theme = useTheme();
    const navColor = usePreferenceStore((s) => s.navColor);
    const isApparent = navColor === 'apparent';
    const isDark = theme.palette.mode === 'dark';

    const headerColor = isApparent ? 'rgba(255, 255, 255, 0.7)' : 'text.primary';
    const iconColor = isApparent ? 'rgba(255, 255, 255, 0.6)' : 'text.secondary';
    const labelColor = isApparent ? '#ffffff' : 'primary.main';
    const headerHoverBg = isApparent
      ? 'rgba(255, 255, 255, 0.06)'
      : isDark
        ? 'rgba(255,255,255,0.02)'
        : '#f5f5f5';
    const headerActiveBg = isApparent
      ? 'rgba(255, 255, 255, 0.03)'
      : isDark
        ? 'rgba(255,255,255,0.02)'
        : '#f5f5f5';
    const borderBottomColor = isApparent
      ? 'rgba(255, 255, 255, 0.08)'
      : isDark
        ? 'rgba(255,255,255,0.05)'
        : '#f3f2f1';

    if (collapsed) {
      // In collapsed state, show only the section header icon (no children)
      return (
        <Box
          onClick={onToggle}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            height: 40,
            cursor: onToggle ? 'pointer' : 'default',
            color: iconColor,
            '&:hover': onToggle ? { bgcolor: headerHoverBg } : {},
            '& svg': { fontSize: 20 },
          }}
        >
          {icon}
        </Box>
      );
    }

    return (
      <Box>
        <Box
          onClick={onToggle}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            px: 2,
            py: 1.25,
            cursor: onToggle ? 'pointer' : 'default',
            color: headerColor,
            borderBottom: '1px solid',
            borderColor: borderBottomColor,
            bgcolor: expanded ? headerActiveBg : 'transparent',
            '&:hover': onToggle ? { bgcolor: headerHoverBg } : {},
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {icon && (
              <Box sx={{ color: iconColor, display: 'flex', '& svg': { fontSize: 20 } }}>
                {icon}
              </Box>
            )}
            <Typography sx={{ fontSize: '0.9rem', fontWeight: 500, color: labelColor }}>
              {label}
            </Typography>
          </Box>
          {onToggle && (
            <Box sx={{ display: 'flex', color: labelColor }}>
              {expanded ? (
                <ExpandLess sx={{ fontSize: 20 }} />
              ) : (
                <ExpandMore sx={{ fontSize: 20 }} />
              )}
            </Box>
          )}
        </Box>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Box sx={{ py: 0.5 }}>{children}</Box>
        </Collapse>
      </Box>
    );
  }
);

NavSection.displayName = 'NavSection';
