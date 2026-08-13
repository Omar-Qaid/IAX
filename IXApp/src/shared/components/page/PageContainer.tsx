import React from 'react';
import { Box, type SxProps, type Theme } from '@mui/material';

export const PageContainer: React.FC<{ children: React.ReactNode; sx?: SxProps<Theme> }> = ({
  children,
  sx,
}) => {
  return (
    <Box
      sx={[
        { width: '100%', display: 'flex', flexDirection: 'column', gap: 1 },
        ...(Array.isArray(sx) ? sx : [sx]),
      ]}
    >
      {children}
    </Box>
  );
};
