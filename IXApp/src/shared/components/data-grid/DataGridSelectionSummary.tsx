import React from 'react';
import { Box, Typography, alpha } from '@mui/material';
import { useTranslation } from 'react-i18next';

interface DataGridSelectionSummaryProps {
  serverSide?: boolean;
  loadedRows: number;
  totalRowCount: number;
  filteredRows: number;
}

export function DataGridSelectionSummary({ serverSide, loadedRows, totalRowCount, filteredRows }: DataGridSelectionSummaryProps) {
  const { t } = useTranslation();

  const rowLabel = (() => {
    if (serverSide) {
      return loadedRows < totalRowCount
        ? t('grid.rows_loaded', { loaded: loadedRows.toLocaleString(), total: totalRowCount.toLocaleString() })
        : t('grid.rows_count', { count: loadedRows.toLocaleString() });
    }
    return filteredRows < totalRowCount
      ? t('grid.rows_filtered', { filtered: filteredRows.toLocaleString(), total: totalRowCount.toLocaleString() })
      : t('grid.rows_count', { count: totalRowCount.toLocaleString() });
  })();

  const isFiltered = serverSide ? loadedRows < totalRowCount : filteredRows < totalRowCount;

  return (
    <Box sx={{ display: { xs: 'none', sm: 'flex' }, alignItems: 'center' }}>
      <Box
        sx={{
          display: 'inline-flex',
          alignItems: 'center',
          px: 1.25,
          py: 0.5,
          borderRadius: '20px',
          bgcolor: (theme) => isFiltered ? alpha(theme.palette.primary.main, 0.1) : theme.palette.mode === 'light' ? '#f1f5f9' : 'rgba(255,255,255,0.05)',
          border: '1px solid',
          borderColor: (theme) => isFiltered ? alpha(theme.palette.primary.main, 0.2) : 'divider',
          transition: 'all 0.2s',
        }}
      >
        <Typography
          variant="caption"
          sx={{
            fontWeight: 700,
            fontSize: '0.6875rem',
            color: isFiltered ? 'primary.main' : 'text.secondary',
            letterSpacing: '0.02em',
            whiteSpace: 'nowrap',
          }}
        >
          {rowLabel.toUpperCase()}
        </Typography>
      </Box>
    </Box>
  );
}
