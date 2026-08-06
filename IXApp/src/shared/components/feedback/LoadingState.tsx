import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const LoadingState: React.FC<{ message?: string }> = ({ message }) => {
  const { t } = useAppTranslation();
  return (
    <Box
      role="status"
      aria-live="polite"
      aria-busy="true"
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
      <CircularProgress size={32} color="primary" aria-hidden="true" sx={{ mb: 1.5 }} />
      <Typography variant="body2" color="text.secondary">
        {message ?? t('common.loading')}
      </Typography>
    </Box>
  );
};
