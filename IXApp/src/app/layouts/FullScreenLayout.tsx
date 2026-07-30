import React from 'react';
import { Box } from '@mui/material';

export const FullScreenLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <Box sx={{ width: '100vw', height: '100vh', overflow: 'hidden', bgcolor: 'background.default' }}>
      {children}
    </Box>
  );
};
