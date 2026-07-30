import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

export const LoadingState: React.FC<{ message?: string }> = ({ message = 'Loading...' }) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
        minHeight: 200,
        width: '100%',
      }}
    >
      <CircularProgress size={32} color="primary" sx={{ mb: 1.5 }} />
      <Typography variant="body2" color="text.secondary">
        {message}
      </Typography>
    </Box>
  );
};
