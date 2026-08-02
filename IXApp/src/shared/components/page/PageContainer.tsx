import React from 'react';
import { Box, Paper, type SxProps, type Theme } from '@mui/material';

export const PageContainer: React.FC<{ children: React.ReactNode; sx?: SxProps<Theme> }> = ({ children, sx }) => {
  return (
    <Box sx={[{ width: '100%', display: 'flex', flexDirection: 'column', gap: 1.5 }, ...(Array.isArray(sx) ? sx : [sx])]}>
      {children}
    </Box>
  );
};

export const PageContent: React.FC<{ children: React.ReactNode; sx?: object }> = ({ children, sx }) => {
  return (
    <Paper elevation={0} sx={{ p: 2, borderRadius: 1, border: (t) => `1px solid ${t.palette.divider}`, ...sx }}>
      {children}
    </Paper>
  );
};

export const PageSection: React.FC<{ children: React.ReactNode; title?: string; sx?: object }> = ({ children, title, sx }) => {
  return (
    <Box sx={{ mb: 2, ...sx }}>
      {title && (
        <Box sx={{ pb: 1, mb: 1.5, borderBottom: (t) => `1px solid ${t.palette.divider}` }}>
          <Box component="span" sx={{ fontSize: '0.875rem', fontWeight: 700, color: 'primary.main' }}>
            {title}
          </Box>
        </Box>
      )}
      {children}
    </Box>
  );
};
