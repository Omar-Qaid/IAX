import React, { useState } from 'react';
import {
  Box, Typography, Button, Popover, Divider, Chip, MenuItem, ListItemIcon, ListItemText, Menu
} from '@mui/material';
import {
  ArrowUpward, ArrowDownward, KeyboardArrowDown
} from '@mui/icons-material';
import type { ColumnDef, FilterModel } from '../types';
import { GRID_FILTER_OPERATORS } from '../constants';
import { AppTextField } from '@shared/components/fields/AppTextField';

interface FilterPopoverProps<T> {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  column: ColumnDef<T> | null;
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
}

export function FilterPopover<T>({
  anchorEl, onClose, column, filters, setFilters, onSort
}: FilterPopoverProps<T>) {
  const [operatorAnchor, setOperatorAnchor] = useState<HTMLElement | null>(null);
  const [tempValue, setTempValue] = useState('');
  
  // Local state for the filter being edited in the popover
  const [localFilter, setLocalFilter] = useState<FilterModel>(() => {
    const existing = column ? filters.find(f => f.field === column.field) : null;
    return existing || { field: column?.field as string || '', operator: 'contains', value: '' };
  });

  // Sync local filter when popover opens for a column
  React.useEffect(() => {
    if (anchorEl && column) {
      const existing = filters.find(f => f.field === column.field);
      setLocalFilter(existing || { field: column.field as string, operator: 'contains', value: '' });
      setTempValue('');
    }
  }, [anchorEl, column, filters]);

  if (!column) return null;

  const handleApply = () => {
    setFilters(prev => {
      const others = prev.filter(f => f.field !== localFilter.field);
      const val = localFilter.value;
      if (!val || (Array.isArray(val) && val.length === 0)) return others;
      return [...others, localFilter];
    });
    onClose();
  };

  const handleClear = () => {
    setFilters(prev => prev.filter(f => f.field !== localFilter.field));
    setLocalFilter(f => ({ ...f, value: f.operator === 'in' ? [] : '' }));
    onClose();
  };

  return (
    <>
      <Popover
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        onClose={onClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
        slotProps={{ paper: { sx: { width: 300, p: 2, borderRadius: 1, boxShadow: '0 4px 20px rgba(0,0,0,0.15)' } } }}
      >
        {/* Quick sort shortcuts */}
        <Box sx={{ mb: 1.5 }}>
          <MenuItem onClick={() => { onSort(column.field as string, 'asc'); onClose(); }} sx={{ p: '4px 8px', borderRadius: 1 }}>
            <ListItemIcon><ArrowUpward fontSize="small" /></ListItemIcon>
            <ListItemText primary="Sort A to Z" slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }} />
          </MenuItem>
          <MenuItem onClick={() => { onSort(column.field as string, 'desc'); onClose(); }} sx={{ p: '4px 8px', borderRadius: 1 }}>
            <ListItemIcon><ArrowDownward fontSize="small" /></ListItemIcon>
            <ListItemText primary="Sort Z to A" slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }} />
          </MenuItem>
        </Box>

        <Divider sx={{ mb: 1.5 }} />

        <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: '#333' }}>
          {column.headerName}
        </Typography>

        {/* Operator selector */}
        <Box sx={{ mb: 1 }}>
          <Button
            size="small"
            onClick={(e) => setOperatorAnchor(e.currentTarget)}
            endIcon={<KeyboardArrowDown />}
            sx={{ textTransform: 'none', fontSize: '0.85rem', p: 0, color: 'primary.main', fontWeight: 500 }}
          >
            {GRID_FILTER_OPERATORS.find((o: { value: string; label: string }) => o.value === localFilter.operator)?.label}
          </Button>
        </Box>

        {/* Value input */}
        <AppTextField
          fullWidth
          placeholder={localFilter.operator === 'in' ? 'Press Enter to add?' : 'Filter value?'}
          value={localFilter.operator === 'in' ? tempValue : (localFilter.value || '')}
          onChange={(val: any) => {
            if (localFilter.operator === 'in') setTempValue(val);
            else setLocalFilter(f => ({ ...f, value: val }));
          }}
          slotProps={{
            htmlInput: {
              onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => {
                if (e.key === 'Enter' && localFilter.operator === 'in' && tempValue) {
                  const current = Array.isArray(localFilter.value) ? localFilter.value : [];
                  if (!current.includes(tempValue)) {
                    setLocalFilter(f => ({ ...f, value: [...current, tempValue] }));
                  }
                  setTempValue('');
                }
              }
            }
          }}
          sx={{ mb: 1.5, '& input': { fontSize: '0.85rem' } }}
        />

        {/* "is one of" tag list */}
        {localFilter.operator === 'in' && Array.isArray(localFilter.value) && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 2, maxHeight: 120, overflowY: 'auto' }}>
            {localFilter.value.map((val: string) => (
              <Chip
                key={val}
                label={val}
                size="small"
                onDelete={() => setLocalFilter(f => ({ ...f, value: (f.value as string[]).filter(v => v !== val) }))}
                sx={{ borderRadius: 1, bgcolor: '#f5f5f5', border: '1px solid #e0e0e0' }}
              />
            ))}
          </Box>
        )}

        <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
          <Button
            fullWidth variant="contained" size="small"
            onClick={handleApply}
            sx={{ bgcolor: '#3b5bdb', '&:hover': { bgcolor: '#2f49b5' }, textTransform: 'none', fontWeight: 600 }}
          >
            Apply
          </Button>
          <Button
            fullWidth variant="outlined" size="small"
            onClick={handleClear}
            sx={{ textTransform: 'none', color: '#333', borderColor: '#ccc', fontWeight: 600 }}
          >
            Clear
          </Button>
        </Box>
      </Popover>

      {/* Operator sub-menu */}
      <Menu
        anchorEl={operatorAnchor}
        open={Boolean(operatorAnchor)}
        onClose={() => setOperatorAnchor(null)}
      >
        {GRID_FILTER_OPERATORS.map((op: { value: string; label: string }) => (
          <MenuItem key={op.value} onClick={() => { setLocalFilter(f => ({ ...f, operator: op.value as any })); setOperatorAnchor(null); }}>
            <ListItemText primary={op.label} slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }} />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
