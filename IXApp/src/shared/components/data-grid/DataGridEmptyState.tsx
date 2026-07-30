import React from 'react';
import { Box, Typography, useTheme } from '@mui/material';
import { Inbox as InboxIcon, SearchOff as SearchOffIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';

interface DataGridEmptyStateProps {
  hasActiveFilters?: boolean;
}

export function DataGridEmptyState({ hasActiveFilters }: DataGridEmptyStateProps) {
  const theme = useTheme();
  const { t } = useTranslation();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 10, px: 4, userSelect: 'none', bgcolor: 'background.paper' }}>
      <Box sx={{ width: 72, height: 72, borderRadius: '50%', bgcolor: theme.palette.action.hover, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
        {hasActiveFilters ? <SearchOffIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} /> : <InboxIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} />}
      </Box>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: 'text.secondary', mb: 0.5 }}>
        {hasActiveFilters ? t('grid.no_records') : t('grid.no_data')}
      </Typography>
      <Typography variant="body2" sx={{ color: 'text.disabled', textAlign: 'center', maxWidth: 280, lineHeight: 1.6 }}>
        {hasActiveFilters ? t('grid.no_results_msg') : t('grid.no_records_msg')}
      </Typography>
    </Box>
  );
}
