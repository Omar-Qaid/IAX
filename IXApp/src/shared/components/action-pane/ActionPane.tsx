import React from 'react';
import { Paper, Box } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export interface ActionPaneProps {
  children: React.ReactNode;
  variant?: 'default' | 'flat';
  endActions?: React.ReactNode;
}

export const ActionPane: React.FC<ActionPaneProps> = ({ children, variant = 'default', endActions }) => {
  return (
    <Paper
      elevation={0}
      sx={{
        p: '3px 6px',
        minHeight: d365.toolbarHeight,
        boxSizing: 'border-box',
        mx: 0,
        mt: 0,
        mb: '6px',
        borderRadius: variant === 'flat' ? '9px' : 1,
        border: (t) => `1px solid ${t.palette.divider}`,
        bgcolor: variant === 'flat' ? 'background.paper' : (t) => (t.palette.mode === 'light' ? '#f8f9fa' : '#222222'),
        boxShadow: variant === 'flat' ? '0 2px 7px rgba(0,0,0,0.16)' : 'none',
        display: 'flex',
        alignItems: 'center',
        overflowX: 'auto',
        overflowY: 'hidden',
        maxWidth: '100%',
        WebkitOverflowScrolling: 'touch',
        whiteSpace: 'nowrap',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', minWidth: 'max-content', flexShrink: 0 }}>{children}</Box>
      {endActions && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.25, marginInlineStart: 'auto', paddingInlineStart: 1.5, position: 'sticky', insetInlineEnd: 0, flexShrink: 0, bgcolor: 'inherit' }}>{endActions}</Box>}
    </Paper>
  );
};
