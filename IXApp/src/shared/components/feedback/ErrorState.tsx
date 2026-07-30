import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import ErrorIcon from '@mui/icons-material/Error';

export const ErrorState: React.FC<{ title?: string; message?: string; onRetry?: () => void }> = ({
  title = 'Failed to load data',
  message = 'An error occurred while fetching resources.',
  onRetry,
}) => {
  return (
    <Box sx={{ p: 3, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center', border: (t) => `1px solid ${t.palette.divider}`, maxWidth: 450, borderRadius: 1 }}>
        <ErrorIcon color="error" sx={{ fontSize: 44, mb: 1 }} />
        <Typography variant="h6" color="error" sx={{ fontWeight: 700, mb: 1 }}>
          {title}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message}
        </Typography>
        {onRetry && (
          <Button variant="outlined" size="small" color="primary" onClick={onRetry}>
            Retry Request
          </Button>
        )}
      </Paper>
    </Box>
  );
};
