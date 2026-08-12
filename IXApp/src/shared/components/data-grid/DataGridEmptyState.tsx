import React from 'react';
import { Box, Typography, useTheme } from '@mui/material';
import SearchOffIcon from '@mui/icons-material/SearchOff';
import { useTranslation } from 'react-i18next';
import { EmptyDataWatermark } from '@shared/components/feedback/EmptyDataWatermark';

interface DataGridEmptyStateProps {
  hasActiveFilters?: boolean;
}

export function DataGridEmptyState({ hasActiveFilters }: DataGridEmptyStateProps) {
  const theme = useTheme();
  const { t } = useTranslation();

  if (!hasActiveFilters) {
    return (
      <Box sx={{ flex: 1, minWidth: 0, minHeight: 0, bgcolor: 'background.paper' }}>
        <EmptyDataWatermark />
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 10, px: 4, userSelect: 'none', bgcolor: 'background.paper' }}>
      <Box sx={{ width: 72, height: 72, borderRadius: '50%', bgcolor: theme.palette.action.hover, display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 2 }}>
        <SearchOffIcon sx={{ fontSize: 34, color: theme.palette.text.disabled }} />
      </Box>
      <Typography variant="subtitle1" sx={{ fontWeight: 600, color: 'text.secondary', mb: 0.5 }}>
        {t('grid.no_records')}
      </Typography>
      <Typography variant="body2" sx={{ color: 'text.disabled', textAlign: 'center', maxWidth: 280, lineHeight: 1.6 }}>
        {t('grid.no_results_msg')}
      </Typography>
    </Box>
  );
}
