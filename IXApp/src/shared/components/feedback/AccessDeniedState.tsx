import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { useNavigate } from 'react-router-dom';
import { ROUTE_PATHS } from '@app/routes/routePaths';

export const AccessDeniedState: React.FC<{ message?: string }> = ({
  message = 'You do not have administrative permission to view this page.',
}) => {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 4, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center', border: (t) => `1px solid ${t.palette.divider}`, maxWidth: 450, borderRadius: 1 }}>
        <LockOutlinedIcon color="error" sx={{ fontSize: 52, mb: 1 }} />
        <Typography variant="h6" color="error" sx={{ fontWeight: 700, mb: 1 }}>
          Access Denied
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message}
        </Typography>
        <Button variant="contained" size="small" color="primary" onClick={() => navigate(ROUTE_PATHS.DASHBOARD)}>
          Back to Dashboard
        </Button>
      </Paper>
    </Box>
  );
};
