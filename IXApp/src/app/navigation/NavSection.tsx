import React from 'react';
import { Box, Typography, Collapse, useTheme } from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { navigationTokens as nav } from './navigationTokens';
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
    const isDark = theme.palette.mode === 'dark';
    const apparent = usePreferenceStore((s) => s.navColor === 'apparent');
    const useDarkNavigation = isDark || apparent;

    const headerColor = useDarkNavigation ? '#f3f2f1' : nav.text;
    const iconColor = useDarkNavigation ? '#c8c6c4' : nav.icon;
    const headerHoverBg = useDarkNavigation ? 'rgba(255,255,255,0.08)' : nav.hover;

    if (collapsed) {
      // In collapsed state, show only the section header icon (no children)
      return (
        <Box
          onClick={onToggle}
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            height: nav.itemHeight,
            cursor: onToggle ? 'pointer' : 'default',
            color: iconColor,
            '&:hover': onToggle ? { bgcolor: headerHoverBg } : {},
            '& svg': { fontSize: nav.iconSize },
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
            height: nav.itemHeight,
            px: `${nav.horizontalPadding}px`,
            py: 0,
            cursor: onToggle ? 'pointer' : 'default',
            color: headerColor,
            bgcolor: 'transparent',
            fontFamily: nav.fontFamily,
            '&:hover': onToggle ? { bgcolor: headerHoverBg } : {},
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: `${nav.iconTextGap}px` }}>
            {icon && (
              <Box sx={{ color: iconColor, display: 'flex', '& svg': { fontSize: nav.iconSize } }}>
                {icon}
              </Box>
            )}
            <Typography sx={{ fontFamily: nav.fontFamily, fontSize: nav.fontSize, lineHeight: '20px', fontWeight: nav.fontWeight, color: headerColor }}>
              {label}
            </Typography>
          </Box>
          {onToggle && (
            <Box sx={{ display: 'flex', color: iconColor, mr: '-7px' }}>
              {expanded ? (
                <ExpandLess sx={{ fontSize: nav.chevronSize }} />
              ) : (
                <ExpandMore sx={{ fontSize: nav.chevronSize }} />
              )}
            </Box>
          )}
        </Box>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Box>{children}</Box>
        </Collapse>
      </Box>
    );
  }
);

NavSection.displayName = 'NavSection';
