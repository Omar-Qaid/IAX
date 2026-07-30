import React from 'react';
import { Box, Typography, useTheme, Button } from '@mui/material';
import { ErrorOutline as ErrorIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';

interface DataGridErrorStateProps {
  error: string | Error;
  onRetry?: () => void;
}

export function DataGridErrorState({ error, onRetry }: DataGridErrorStateProps) {
  const theme = useTheme();
  const { t } = useTranslation();

  const errorMessage = typeof error === 'string' ? error : error.message;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 10, px: 4, bgcolor: 'background.paper' }}>
      <Box sx={{ width: 72, height: 72, borderRadius: '50%', bgcolor: theme.palette.error.light, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
        <ErrorIcon sx={{ fontSize: 34, color: theme.palette.error.main }} />
      </Box>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: 'text.primary', mb: 0.5 }}>
        {t('common.error_occurred')}
      </Typography>
      <Typography variant="body2" sx={{ color: 'text.secondary', textAlign: 'center', maxWidth: 400, lineHeight: 1.6, mb: 2 }}>
        {errorMessage}
      </Typography>
      {onRetry && (
        <Button variant="outlined" color="primary" onClick={onRetry} size="small">
          {t('common.retry')}
        </Button>
      )}
    </Box>
  );
}
