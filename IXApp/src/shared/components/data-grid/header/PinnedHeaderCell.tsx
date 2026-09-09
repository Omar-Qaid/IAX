import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton } from '@mui/material';
import MoreVert from '@mui/icons-material/MoreVert';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import FilterIcon from '@mui/icons-material/FilterList';
import type { ColumnDef, FilterModel, SortModel } from '../types';
import { FilterInput } from './FilterInput';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

interface PinnedHeaderCellProps<T> {
  column: ColumnDef<T>;
  sortModel?: SortModel[];
  onSort?: (field: string) => void;
  offset: number;
  side: 'left' | 'right';
  filters: FilterModel[];
  onFilterChange: (field: string, value: string) => void;
  onFilterIconClick: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onMenuOpen: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onResizeStart: (event: React.MouseEvent, field: string, edge?: 'inline-start' | 'inline-end') => void;
  headerHeight: number;
  showColumnBorders?: boolean;
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
}

export function PinnedHeaderCell<T>({
  column,
  sortModel = [],
  onSort,
  offset,
  side,
  filters,
  onFilterChange,
  onFilterIconClick,
  onMenuOpen,
  onResizeStart,
  headerHeight,
  showColumnBorders = false,
  hideFilterRow = false,
  hideColumnMenu = false,
}: PinnedHeaderCellProps<T>) {
  const { t } = useTranslation();
  const sort = sortModel.find((entry) => entry.field === column.field)?.sort;
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        boxSizing: 'border-box',
        bgcolor: '#ffffff',
        width: column.width || 150,
        minWidth: column.width || 150,
        maxWidth: column.width || 150,
        overflow: 'hidden',
        position: 'sticky',
        zIndex: 6,
        flexShrink: 0,
        flexGrow: 0,
        ...(side === 'left'
          ? { insetInlineStart: offset, borderInlineEnd: (theme) => showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none' }
          : { insetInlineEnd: offset, borderInlineStart: (theme) => showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none' }),
      }}
    >
      {/* Name row */}
      <Box
        role="columnheader"
        aria-sort={sort === 'asc' ? 'ascending' : sort === 'desc' ? 'descending' : 'none'}
        tabIndex={column.sortable !== false && onSort ? 0 : -1}
        onClick={() => { if (column.sortable !== false) onSort?.(String(column.field)); }}
        onKeyDown={(event) => {
          if (event.target === event.currentTarget && ['Enter', ' '].includes(event.key) && column.sortable !== false) {
            event.preventDefault(); onSort?.(String(column.field));
          }
        }}
        sx={{
          display: 'flex',
          alignItems: 'center',
          p: '3px 8px',
          borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
          position: 'relative',
          height: headerHeight,
          minHeight: headerHeight,
          boxSizing: 'border-box',
          '&:hover': {
            bgcolor: (theme) =>
              theme.palette.mode === 'light' ? '#e9e8e7' : theme.palette.action.hover,
          },
        }}
      >
        <Typography
          variant="subtitle2"
          sx={{
            flexGrow: 1,
            fontWeight: 600,
            fontFamily: APP_FONT_FAMILY,
            fontSize: 12,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            letterSpacing: 0,
            textAlign: column.headerAlign ?? 'start',
          }}
        >
          {t(column.headerName || '')}
        </Typography>
        {sort === 'asc' && <ArrowUpwardIcon sx={{ fontSize: 14 }} />}
        {sort === 'desc' && <ArrowDownwardIcon sx={{ fontSize: 14 }} />}
        {!hideColumnMenu && (
          <IconButton
            size="small"
            aria-label={t('grid.column_menu', { column: t(column.headerName || '') })}
            sx={{ p: 0.25, marginInlineStart: 0.25 }}
            onClick={(e) => { e.stopPropagation(); onMenuOpen(e, column); }}
          >
            <MoreVert sx={{ fontSize: 14 }} />
          </IconButton>
        )}
      </Box>
      {/* Filter row */}
      {!hideFilterRow && (
        <Box
          sx={{
            p: '3px 8px',
            borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
            bgcolor: (theme) => (theme.palette.mode === 'light' ? '#ffffff' : '#2d3748'),
            display: 'flex',
            alignItems: 'center',
            height: headerHeight,
            minHeight: headerHeight,
            boxSizing: 'border-box',
          }}
        >
          {column.filterable === false ? (
            <FilterIcon sx={{ fontSize: 12, color: 'action.disabled' }} />
          ) : (
            <FilterInput
              column={column}
              filters={filters}
              onFilterChange={onFilterChange}
              onFilterIconClick={onFilterIconClick}
            />
          )}
        </Box>
      )}
      {/* Resize handle */}
      <Box
        data-grid-resize-handle={String(column.field)}
        onMouseDown={(e) => onResizeStart(e, column.field as string, side === 'left' ? 'inline-end' : 'inline-start')}
        sx={{
          position: 'absolute',
          top: 0,
          bottom: 0,
          width: 6,
          cursor: 'col-resize',
          zIndex: 10,
          ...(side === 'left' ? { insetInlineEnd: 0 } : { insetInlineStart: 0 }),
          '&:hover': { bgcolor: 'primary.main', opacity: 0.5 },
        }}
      />
    </Box>
  );
}
