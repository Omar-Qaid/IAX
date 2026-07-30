import React from 'react';
import { Paper, Box } from '@mui/material';

export interface ActionPaneProps {
  children: React.ReactNode;
}

export const ActionPane: React.FC<ActionPaneProps> = ({ children }) => {
  return (
    <Paper
      elevation={0}
      sx={{
        p: '6px 12px',
        mb: 1.5,
        borderRadius: 1,
        border: (t) => `1px solid ${t.palette.divider}`,
        bgcolor: (t) => (t.palette.mode === 'light' ? '#f8f9fa' : '#222222'),
        display: 'flex',
        alignItems: 'center',
        overflowX: 'auto',
        whiteSpace: 'nowrap',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center' }}>{children}</Box>
    </Paper>
  );
};
