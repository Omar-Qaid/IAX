import React from 'react';
import { Box, Typography, Stack } from '@mui/material';

export interface ActionPaneGroupProps {
  label?: string;
  children: React.ReactNode;
}

export const ActionPaneGroup: React.FC<ActionPaneGroupProps> = ({ label, children }) => {
  return (
    <Box sx={{ display: 'inline-flex', flexDirection: 'column', paddingInlineEnd: 0.75, marginInlineEnd: 0.5, borderInlineEnd: (t) => `1px solid ${t.palette.divider}` }}>
      <Stack direction="row" spacing={0} sx={{ alignItems: 'center' }}>
        {children}
      </Stack>
      {label && (
        <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.65rem', mt: 0.25, textAlign: 'center' }}>
          {label}
        </Typography>
      )}
    </Box>
  );
};
