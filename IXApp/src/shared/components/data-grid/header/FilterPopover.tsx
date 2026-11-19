import React, { useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Popover,
  Divider,
  Chip,
  MenuItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Menu,
} from '@mui/material';
import ArrowUpward from '@mui/icons-material/ArrowUpward';
import ArrowDownward from '@mui/icons-material/ArrowDownward';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import type { ColumnDef, FilterModel } from '../types';
import {
  GRID_FILTER_OPERATORS,
  VALUELESS_FILTER_OPERATORS,
  defaultFilterOperator,
  filterOperatorsFor,
} from '../constants';
import { AppTextField } from '@shared/components/fields/AppTextField';
import { useTranslation } from 'react-i18next';

interface FilterPopoverProps<T> {
  anchorEl: HTMLElement | null;
  onClose: () => void;
  column: ColumnDef<T> | null;
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
}

export function FilterPopover<T>({
  anchorEl,
  onClose,
  column,
  filters,
  setFilters,
  onSort,
}: FilterPopoverProps<T>) {
  const { t } = useTranslation();
  const [operatorAnchor, setOperatorAnchor] = useState<HTMLElement | null>(null);
  const [tempValue, setTempValue] = useState('');

  // Local state for the filter being edited in the popover
  const [localFilter, setLocalFilter] = useState<FilterModel>(() => {
    const existing = column ? filters.find((f) => f.field === column.field) : null;
    return (
      existing || {
        field: (column?.field as string) || '',
        operator: defaultFilterOperator(column?.type),
        value: '',
      }
    );
  });

  // Sync local filter when popover opens for a column
  React.useEffect(() => {
    if (anchorEl && column) {
      const existing = filters.find((f) => f.field === column.field);
      setLocalFilter(
        existing || {
          field: column.field as string,
          operator: defaultFilterOperator(column.type),
          value: '',
        }
      );
      setTempValue('');
    }
  }, [anchorEl, column, filters]);

  if (!column) return null;

  const valueless = VALUELESS_FILTER_OPERATORS.has(localFilter.operator);

  const handleApply = () => {
    // A value typed into "is one of" but not yet confirmed with Enter still counts.
    let next = localFilter;
    if (localFilter.operator === 'in' && tempValue.trim()) {
      const current = Array.isArray(localFilter.value) ? (localFilter.value as string[]) : [];
      const pending = tempValue.trim();
      next = { ...localFilter, value: current.includes(pending) ? current : [...current, pending] };
    }
    if (valueless) next = { ...next, value: '' };
    setFilters((prev) => {
      const others = prev.filter((f) => f.field !== next.field);
      const val = next.value;
      if (!valueless && (val == null || val === '' || (Array.isArray(val) && val.length === 0)))
        return others;
      return [...others, next];
    });
    onClose();
  };

  const handleClear = () => {
    setFilters((prev) => prev.filter((f) => f.field !== localFilter.field));
    setLocalFilter((f) => ({ ...f, value: f.operator === 'in' ? [] : '' }));
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
        slotProps={{
          paper: {
            sx: { width: 300, p: 2, borderRadius: 1, boxShadow: '0 4px 20px rgba(0,0,0,0.15)' },
          },
        }}
      >
        {/* Quick sort shortcuts */}
        <Box sx={{ mb: 1.5 }}>
          <ListItemButton
            onClick={() => {
              onSort(column.field as string, 'asc');
              onClose();
            }}
            sx={{ p: '4px 8px', borderRadius: 1 }}
          >
            <ListItemIcon>
              <ArrowUpward fontSize="small" />
            </ListItemIcon>
            <ListItemText
              primary={t('grid.sort_ascending')}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </ListItemButton>
          <ListItemButton
            onClick={() => {
              onSort(column.field as string, 'desc');
              onClose();
            }}
            sx={{ p: '4px 8px', borderRadius: 1 }}
          >
            <ListItemIcon>
              <ArrowDownward fontSize="small" />
            </ListItemIcon>
            <ListItemText
              primary={t('grid.sort_descending')}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </ListItemButton>
        </Box>

        <Divider sx={{ mb: 1.5 }} />

        <Typography variant="subtitle2" sx={{ mb: 1, fontWeight: 600, color: 'text.primary' }}>
          {column.headerName}
        </Typography>

        {/* Operator selector */}
        <Box sx={{ mb: 1 }}>
          <Button
            size="small"
            onClick={(e) => setOperatorAnchor(e.currentTarget)}
            endIcon={<KeyboardArrowDown />}
            sx={{
              textTransform: 'none',
              fontSize: '0.85rem',
              p: 0,
              color: 'primary.main',
              fontWeight: 500,
            }}
          >
            {getOperatorLabel(localFilter.operator, t)}
          </Button>
        </Box>

        {/* Value input (blank checks take none) */}
        {!valueless && (
          <AppTextField
            fullWidth
            placeholder={
              localFilter.operator === 'in'
                ? t('grid.press_enter_to_add')
                : t('grid.filter_placeholder')
            }
            type={
              localFilter.operator === 'in'
                ? 'text'
                : column.type === 'number'
                  ? 'number'
                  : column.type === 'date'
                    ? 'date'
                    : 'text'
            }
            value={
              localFilter.operator === 'in'
                ? tempValue
                : Array.isArray(localFilter.value)
                  ? ''
                  : String(localFilter.value ?? '')
            }
            onChange={(val) => {
              const nextValue = String(val ?? '');
              if (localFilter.operator === 'in') setTempValue(nextValue);
              else setLocalFilter((f) => ({ ...f, value: nextValue }));
            }}
            slotProps={{
              htmlInput: {
                onKeyDown: (e: React.KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === 'Enter' && localFilter.operator === 'in' && tempValue) {
                    const current = Array.isArray(localFilter.value) ? localFilter.value : [];
                    if (!current.includes(tempValue)) {
                      setLocalFilter((f) => ({ ...f, value: [...current, tempValue] }));
                    }
                    setTempValue('');
                  }
                },
              },
            }}
            sx={{ mb: 1.5, '& input': { fontSize: '0.85rem' } }}
          />
        )}

        {/* "is one of" tag list */}
        {localFilter.operator === 'in' && Array.isArray(localFilter.value) && (
          <Box
            sx={{
              display: 'flex',
              flexWrap: 'wrap',
              gap: 0.5,
              mb: 2,
              maxHeight: 120,
              overflowY: 'auto',
            }}
          >
            {localFilter.value.map((val: string) => (
              <Chip
                key={val}
                label={val}
                size="small"
                onDelete={() =>
                  setLocalFilter((f) => ({
                    ...f,
                    value: (f.value as string[]).filter((v) => v !== val),
                  }))
                }
                sx={{ borderRadius: 1, bgcolor: 'action.hover', border: 1, borderColor: 'divider' }}
              />
            ))}
          </Box>
        )}

        <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
          <Button
            fullWidth
            variant="contained"
            size="small"
            onClick={handleApply}
            sx={{
              bgcolor: 'primary.main',
              '&:hover': { bgcolor: 'primary.dark' },
              textTransform: 'none',
              fontWeight: 600,
            }}
          >
            {t('actions.apply')}
          </Button>
          <Button
            fullWidth
            variant="outlined"
            size="small"
            onClick={handleClear}
            sx={{
              textTransform: 'none',
              color: 'text.primary',
              borderColor: 'divider',
              fontWeight: 600,
            }}
          >
            {t('actions.clear')}
          </Button>
        </Box>
      </Popover>

      {/* Operator sub-menu */}
      <Menu
        anchorEl={operatorAnchor}
        open={Boolean(operatorAnchor)}
        onClose={() => setOperatorAnchor(null)}
      >
        {filterOperatorsFor(column.type).map((op) => (
          <MenuItem
            key={op.value}
            selected={op.value === localFilter.operator}
            onClick={() => {
              // "is one of" holds a list; every other operator holds a single value.
              // Carry the value across only when the shape stays the same.
              setLocalFilter((f) => {
                const operator = op.value as FilterModel['operator'];
                const wasList = f.operator === 'in';
                const isList = operator === 'in';
                if (wasList === isList) return { ...f, operator };
                if (isList) {
                  const single = String(f.value ?? '').trim();
                  return { ...f, operator, value: single ? [single] : [] };
                }
                return { ...f, operator, value: '' };
              });
              setTempValue('');
              setOperatorAnchor(null);
            }}
          >
            <ListItemText
              primary={getOperatorLabel(op.value, t)}
              slotProps={{ primary: { sx: { fontSize: '0.85rem' } } }}
            />
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}

const getOperatorLabel = (
  operator: string,
  t: (key: string, options: { defaultValue: string }) => string
) => {
  const translationKey = GRID_FILTER_OPERATORS.find(
    (candidate) => candidate.value === operator
  )?.labelKey;
  return translationKey ? t(translationKey, { defaultValue: operator }) : operator;
};
