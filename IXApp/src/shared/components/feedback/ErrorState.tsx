import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import ErrorIcon from '@mui/icons-material/Error';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const ErrorState: React.FC<{ title?: string; message?: string; onRetry?: () => void }> = ({
  title,
  message,
  onRetry,
}) => {
  const { t } = useAppTranslation();
  return (
    <Box sx={{ p: 3, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center', border: (t) => `1px solid ${t.palette.divider}`, maxWidth: 450, borderRadius: 1 }}>
        <ErrorIcon color="error" sx={{ fontSize: 44, mb: 1 }} />
        <Typography variant="h6" color="error" sx={{ fontWeight: 700, mb: 1 }}>
          {title ?? t('errors.loadFailed')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message ?? t('errors.generic')}
        </Typography>
        {onRetry && (
          <Button variant="outlined" size="small" color="primary" onClick={onRetry}>
            {t('actions.retry')}
          </Button>
        )}
      </Paper>
    </Box>
  );
};
