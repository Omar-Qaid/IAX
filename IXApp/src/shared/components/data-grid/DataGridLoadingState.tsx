import React from 'react';
import { Box, Typography, CircularProgress } from '@mui/material';
import { useTranslation } from 'react-i18next';

export function DataGridLoadingState() {
  const { t } = useTranslation();

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 1, py: 2, bgcolor: 'background.paper' }}>
      <CircularProgress size={16} thickness={4} />
      <Typography variant="caption" color="text.disabled">{t('grid.loading_more')}</Typography>
    </Box>
  );
}
