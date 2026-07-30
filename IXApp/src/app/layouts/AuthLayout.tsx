import React from 'react';
import { Box, Paper, Typography } from '@mui/material';

export const AuthLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'background.default',
      }}
    >
      <Paper elevation={3} sx={{ p: 4, width: '100%', maxWidth: 400, borderRadius: 2 }}>
        <Typography variant="h5" color="primary" sx={{ textAlign: 'center', mb: 2, fontWeight: 700 }}>
          IXApp Enterprise
        </Typography>
        {children}
      </Paper>
    </Box>
  );
};
