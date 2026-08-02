import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const AccessDeniedState: React.FC<{ title?: string; message?: string }> = ({ title, message }) => {
  const navigate = useNavigate();
  const { t } = useAppTranslation();

  return (
    <Box sx={{ p: 4, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center', border: (t) => `1px solid ${t.palette.divider}`, maxWidth: 450, borderRadius: 1 }}>
        <LockOutlinedIcon color="error" sx={{ fontSize: 52, mb: 1 }} />
        <Typography variant="h6" color="error" sx={{ fontWeight: 700, mb: 1 }}>
          {title ?? t('pages.accessDenied.title')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message ?? t('pages.accessDenied.message')}
        </Typography>
        <Button variant="contained" size="small" color="primary" onClick={() => navigate(ROUTE_PATHS.DASHBOARD)}>
          {t('actions.backToDashboard')}
        </Button>
      </Paper>
    </Box>
  );
};
