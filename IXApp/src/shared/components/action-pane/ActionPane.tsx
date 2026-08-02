import React from 'react';
import { Paper, Box } from '@mui/material';

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
        p: variant === 'flat' ? '3px 8px' : '6px 12px',
        mb: variant === 'flat' ? 0.5 : 1.5,
        borderRadius: variant === 'flat' ? 0.75 : 1,
        border: (t) => `1px solid ${t.palette.divider}`,
        bgcolor: variant === 'flat' ? 'background.paper' : (t) => (t.palette.mode === 'light' ? '#f8f9fa' : '#222222'),
        boxShadow: variant === 'flat' ? '0 1px 3px rgba(0,0,0,0.10)' : 'none',
        display: 'flex',
        alignItems: 'center',
        overflowX: 'auto',
        whiteSpace: 'nowrap',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', minWidth: 0 }}>{children}</Box>
      {endActions && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.25, ml: 'auto', pl: 1.5, position: 'sticky', right: 0, bgcolor: 'inherit' }}>{endActions}</Box>}
    </Paper>
  );
};
