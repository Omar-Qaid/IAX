import React from 'react';
import { useTranslation } from 'react-i18next';
import {
  Box, Typography, IconButton, Button, Chip,
} from '@mui/material';
import {
  Close as CloseIcon,
} from '@mui/icons-material';
import type { ColumnDef, FilterModel } from '../types';

interface FiltersPanelProps<T> {
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  columns: ColumnDef<T>[];
  operatorLabels: Record<FilterModel['operator'], string>;
}

export function FiltersPanel<T>({
  filters, setFilters, columns, operatorLabels
}: FiltersPanelProps<T>) {
  const { t } = useTranslation();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, minHeight: 0, overflow: 'hidden' }}>
      <Box sx={{ flexGrow: 1, minHeight: 0, overflow: 'auto', p: 1.5 }}>
        {filters.length === 0 ? (
          <Typography variant="body2" color="text.disabled" sx={{ fontSize: '0.8rem', textAlign: 'center', mt: 2 }}>
            {t('grid.no_active_filters')}
          </Typography>
        ) : (
          filters.map(f => {
            const col = columns.find(c => c.field === f.field);
            const valueLabel = Array.isArray(f.value) ? f.value.join(', ') : f.value;
            return (
              <Box
                key={f.field}
                sx={{ mb: 1, p: 1, bgcolor: '#f8f9fa', borderRadius: 1, border: '1px solid #e0e0e0' }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 0.5 }}>
                  <Typography variant="caption" sx={{ fontWeight: 700, flexGrow: 1, color: 'text.primary' }}>
                    {col?.headerName || f.field}
                  </Typography>
                  <IconButton
                    size="small"
                    sx={{ p: 0.25 }}
                    onClick={() => setFilters(prev => prev.filter(x => x.field !== f.field))}
                  >
                    <CloseIcon sx={{ fontSize: 13 }} />
                  </IconButton>
                </Box>
                <Typography variant="caption" sx={{ color: 'text.secondary', display: 'block' }}>
                  {operatorLabels[f.operator]}
                </Typography>
                {Array.isArray(f.value) ? (
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mt: 0.5 }}>
                    {f.value.map((v: string) => (
                      <Chip key={v} label={v} size="small" sx={{ height: 18, fontSize: '0.7rem', borderRadius: 1 }} />
                    ))}
                  </Box>
                ) : (
                  <Typography variant="caption" sx={{ color: 'primary.main', fontWeight: 600 }}>
                    {String(valueLabel)}
                  </Typography>
                )}
              </Box>
            );
          })
        )}
      </Box>

      {filters.length > 0 && (
        <Box sx={{ p: 1, borderTop: '1px solid #e0e0e0' }}>
          <Button
            size="small"
            variant="outlined"
            fullWidth
            sx={{ textTransform: 'none', fontSize: '0.8rem' }}
            onClick={() => setFilters([])}
          >
            {t('grid.clear_all_filters')}
          </Button>
        </Box>
      )}
    </Box>
  );
}
