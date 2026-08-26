import React from 'react';
import { Box, Paper, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const AuthLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { t } = useAppTranslation();
  return (
    <Box
      sx={{
        minHeight: '100vh',
        '@supports (min-height: 100dvh)': { minHeight: '100dvh' },
        p: { xs: 1.5, sm: 3 },
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'background.default',
      }}
    >
      <Paper elevation={3} sx={{ p: { xs: 2, sm: 4 }, width: '100%', maxWidth: 400, minWidth: 0, borderRadius: 2 }}>
        <Typography variant="h5" color="primary" sx={{ textAlign: 'center', mb: 2, fontWeight: 700 }}>
          {t('nav.app_title')}
        </Typography>
        {children}
      </Paper>
    </Box>
  );
};
