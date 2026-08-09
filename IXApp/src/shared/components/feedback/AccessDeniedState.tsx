import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export interface AccessDeniedStateProps {
  title?: string;
  message?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export const AccessDeniedState: React.FC<AccessDeniedStateProps> = ({
  title,
  message,
  actionLabel,
  onAction,
}) => {
  const { t } = useAppTranslation();

  return (
    <Box sx={{ p: 4, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper
        elevation={0}
        sx={{
          p: 4,
          textAlign: 'center',
          border: (t) => `1px solid ${t.palette.divider}`,
          maxWidth: 450,
          borderRadius: 1,
        }}
      >
        <LockOutlinedIcon color="error" sx={{ fontSize: 52, mb: 1 }} />
        <Typography variant="h6" color="error" sx={{ fontWeight: 700, mb: 1 }}>
          {title ?? t('pages.accessDenied.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message ?? t('pages.accessDenied.message')}
        </Typography>
        {actionLabel && onAction ? (
          <Button variant="contained" size="small" color="primary" onClick={onAction}>
            {actionLabel}
          </Button>
        ) : null}
      </Paper>
    </Box>
  );
};
