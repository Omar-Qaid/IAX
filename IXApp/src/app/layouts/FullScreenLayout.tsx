import React from 'react';
import { Box } from '@mui/material';

export const FullScreenLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <Box sx={{ width: '100%', height: '100vh', minHeight: 0, overflow: 'hidden', bgcolor: 'background.default', '@supports (height: 100dvh)': { height: '100dvh' } }}>
      {children}
    </Box>
  );
};
