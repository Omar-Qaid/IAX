import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export const EmptyState: React.FC<{ title?: string; message?: string; actionLabel?: string; onAction?: () => void }> = ({
  title,
  message,
  actionLabel,
  onAction,
}) => {
  const { t } = useAppTranslation();
  return (
    <Box
      sx={{
        p: 4,
        display: 'flex',
        flex: 1,
        alignItems: 'center',
        justifyContent: 'center',
        width: '100%',
        minWidth: 0,
        minHeight: 0,
        boxSizing: 'border-box',
      }}
    >
      <Paper
        elevation={0}
        sx={{
          width: 'min(100%, 400px)',
          p: 4,
          textAlign: 'center',
          border: (theme) => `1px dashed ${theme.palette.divider}`,
          borderRadius: 1,
          bgcolor: 'transparent',
          boxSizing: 'border-box',
        }}
      >
        <InboxOutlinedIcon color="action" sx={{ fontSize: 48, mb: 1, opacity: 0.7 }} />
        <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
          {title ?? t('common.noData')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message ?? t('grid.no_records_msg')}
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
