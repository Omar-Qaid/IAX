import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';

export const EmptyState: React.FC<{ title?: string; message?: string; actionLabel?: string; onAction?: () => void }> = ({
  title = 'No records found',
  message = 'There are no items to display right now.',
  actionLabel,
  onAction,
}) => {
  return (
    <Box sx={{ p: 4, display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Paper elevation={0} sx={{ p: 4, textAlign: 'center', border: (t) => `1px dashed ${t.palette.divider}`, maxWidth: 400, borderRadius: 1, bgcolor: 'transparent' }}>
        <InboxOutlinedIcon color="action" sx={{ fontSize: 48, mb: 1, opacity: 0.7 }} />
        <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
          {title}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message}
        </Typography>
        {actionLabel && onAction && (
          <Button variant="contained" size="small" color="primary" onClick={onAction}>
            {actionLabel}
          </Button>
        )}
      </Paper>
    </Box>
  );
};
