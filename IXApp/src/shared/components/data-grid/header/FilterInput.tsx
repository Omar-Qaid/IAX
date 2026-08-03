import React from 'react';
import { Box, Chip, Typography, IconButton } from '@mui/material';
import { FilterList as FilterIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import type { ColumnDef, FilterModel } from '../types';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { AppBooleanField } from '@shared/components/fields/AppBooleanField';

interface FilterInputProps<T> {
  column: ColumnDef<T>;
  filters: FilterModel[];
  onFilterChange: (field: string, value: string) => void;
  onFilterIconClick?: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
}

export function FilterInput<T>({ column, filters, onFilterChange, onFilterIconClick }: FilterInputProps<T>) {
  const { t } = useTranslation();
  const field = column.field as string;
  const currentFilter = filters.find(f => f.field === field);

  if (column.type === 'boolean') {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <AppBooleanField
          value={!!currentFilter?.value}
          onChange={(v) => onFilterChange(field, v ? 'true' : '')}
          sx={{ '& .MuiSwitch-root': { transform: 'scale(0.8)' } }}
        />
      </Box>
    );
  }

  if (currentFilter?.operator === 'in') {
    const items: string[] = Array.isArray(currentFilter.value) ? currentFilter.value : [];
    return (
      <Box
        sx={{
          display: 'flex', alignItems: 'center', width: '100%',
          height: 24, px: 0.5, gap: 0.5,
          border: '1px solid', borderColor: 'primary.main',
          borderRadius: 1, overflow: 'hidden', cursor: 'pointer',
        }}
        onClick={(e) => onFilterIconClick?.(e, column)}
      >
        <Box sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', gap: 0.5, overflow: 'hidden' }}>
          {items.slice(0, 2).map(v => (
            <Chip
              key={v}
              label={v}
              size="small"
              sx={{ height: 16, fontSize: '0.6rem', borderRadius: 0.5, maxWidth: 60 }}
            />
          ))}
          {items.length > 2 && (
            <Typography sx={{ fontSize: '0.65rem', color: 'text.secondary', whiteSpace: 'nowrap' }}>
              +{items.length - 2}
            </Typography>
          )}
        </Box>
        <FilterIcon sx={{ fontSize: 12, color: 'primary.main', flexShrink: 0 }} />
      </Box>
    );
  }

  const inputType = column.type === 'number' ? 'number' : column.type === 'date' ? 'date' : 'text';

  return (
    <AppTextField
      placeholder={t('grid.filter_placeholder')}
      fullWidth
      type={inputType}
      value={typeof currentFilter?.value === 'string' ? currentFilter.value : (currentFilter?.value ?? '')}
      onChange={(val: any) => onFilterChange(field, val)}
      slotProps={{
        input: {
          endAdornment: onFilterIconClick ? (
            <IconButton aria-label={t('grid.open_column_filter', { column: column.headerName, defaultValue: `Filter ${column.headerName}` })} size="small" sx={{ p: 0 }} onClick={(e) => onFilterIconClick(e, column)}>
              <FilterIcon sx={{ fontSize: 13, color: currentFilter ? 'primary.main' : 'text.disabled' }} />
            </IconButton>
          ) : (
            <FilterIcon sx={{ fontSize: 13, color: 'text.disabled' }} />
          ),
          sx: { 
            fontSize: '0.75rem', 
            height: 26, 
            bgcolor: 'transparent',
            '& input': { 
              padding: '2px 8px',
            },
            '& input::placeholder': { 
              fontSize: '0.7rem', 
              opacity: 0.5 
            },
            '& fieldset': { border: 'none' },
            '&:hover fieldset': { border: 'none' },
            '&.Mui-focused fieldset': { 
                border: '1px solid',
                borderColor: 'primary.main',
                borderRadius: 1
            },
          },
        }
      }}
    />
  );
}
