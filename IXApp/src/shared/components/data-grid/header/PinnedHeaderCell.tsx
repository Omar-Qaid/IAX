import { uiDensity } from '@shared/constants/uiDensity';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton } from '@mui/material';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import FilterIcon from '@mui/icons-material/FilterList';
import type { ColumnDef, FilterModel, SortModel } from '../types';
import { FilterInput } from './FilterInput';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';
import { headerAlign } from '../columnAlign';

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
  onResizeStart: (
    event: React.MouseEvent,
    field: string,
    edge?: 'inline-start' | 'inline-end'
  ) => void;
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
        bgcolor: 'background.paper',
        width: column.width || 150,
        minWidth: column.width || 150,
        maxWidth: column.width || 150,
        overflow: 'hidden',
        position: 'sticky',
        zIndex: 6,
        flexShrink: 0,
        flexGrow: 0,
        ...(side === 'left'
          ? {
              insetInlineStart: offset,
              borderInlineEnd: (theme) =>
                showColumnBorders ? `1px solid ${enterpriseSurfaces(theme).gridLine}` : 'none',
            }
          : {
              insetInlineEnd: offset,
              borderInlineStart: (theme) =>
                showColumnBorders ? `1px solid ${enterpriseSurfaces(theme).gridLine}` : 'none',
            }),
      }}
    >
      {/* Name row */}
      <Box
        role="columnheader"
        aria-sort={sort === 'asc' ? 'ascending' : sort === 'desc' ? 'descending' : 'none'}
        tabIndex={column.sortable !== false && onSort ? 0 : -1}
        onClick={() => {
          if (column.sortable !== false) onSort?.(String(column.field));
        }}
        onKeyDown={(event) => {
          if (
            event.target === event.currentTarget &&
            ['Enter', ' '].includes(event.key) &&
            column.sortable !== false
          ) {
            event.preventDefault();
            onSort?.(String(column.field));
          }
        }}
        sx={{
          display: 'flex',
          alignItems: 'center',
          p: '3px var(--data-grid-cell-padding, 6px)',
          borderBottom: (theme) => `1px solid ${enterpriseSurfaces(theme).gridLine}`,
          position: 'relative',
          height: headerHeight,
          minHeight: headerHeight,
          boxSizing: 'border-box',
          '&:hover .column-menu-trigger, &:focus-within .column-menu-trigger': { opacity: 1 },
          '&:hover .MuiTypography-root, &:focus-visible .MuiTypography-root': {
            paddingRight: hideColumnMenu ? (sort ? '16px' : 0) : sort ? '38px' : '22px',
          },
          '@media (hover: none)': { '& .column-menu-trigger': { opacity: 1 } },
          '&:hover .column-sort-indicator, &:focus-within .column-sort-indicator': {
            right: hideColumnMenu ? 6 : 26,
          },
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
            minWidth: 0,
            width: '100%',
            fontWeight: 600,
            fontFamily: APP_FONT_FAMILY,
            fontSize: uiDensity.gridFontSize,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            letterSpacing: 0,
            textAlign: headerAlign(column) ?? 'start',
            boxSizing: 'border-box',
            paddingLeft: 0,
            paddingRight: sort ? '16px' : 0,
          }}
        >
          {t(column.headerName || '')}
        </Typography>
        {sort === 'asc' && (
          <ArrowUpwardIcon
            className="column-sort-indicator"
            sx={{ fontSize: 14, position: 'absolute', right: 6 }}
          />
        )}
        {sort === 'desc' && (
          <ArrowDownwardIcon
            className="column-sort-indicator"
            sx={{ fontSize: 14, position: 'absolute', right: 6 }}
          />
        )}
        {!hideColumnMenu && (
          <IconButton
            size="small"
            aria-label={t('grid.column_menu', { column: t(column.headerName || '') })}
            className="column-menu-trigger"
            sx={{
              p: 0.25,
              position: 'absolute',
              right: 4,
              top: '50%',
              transform: 'translateY(-50%)',
              opacity: 0,
              flexShrink: 0,
              color: 'text.primary',
            }}
            onClick={(e) => {
              e.stopPropagation();
              onMenuOpen(e, column);
            }}
          >
            <KeyboardArrowDown sx={{ fontSize: 16 }} />
          </IconButton>
        )}
      </Box>
      {/* Filter row */}
      {!hideFilterRow && (
        <Box
          sx={{
            p: '2px 8px',
            borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
            bgcolor: (theme) => theme.palette.background.paper,
            display: 'flex',
            alignItems: 'center',
            height: Math.max(26, headerHeight - 2),
            minHeight: Math.max(26, headerHeight - 2),
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
        onMouseDown={(e) =>
          onResizeStart(e, column.field as string, side === 'left' ? 'inline-end' : 'inline-start')
        }
        sx={{
          position: 'absolute',
          top: 0,
          bottom: 0,
          width: 6,
          cursor: 'col-resize',
          zIndex: 10,
          ...(side === 'left' ? { insetInlineEnd: 0 } : { insetInlineStart: 0 }),
          '&:hover .column-sort-indicator, &:focus-within .column-sort-indicator': {
            right: hideColumnMenu ? 6 : 26,
          },
          '&:hover': { bgcolor: 'primary.main', opacity: 0.5 },
        }}
      />
    </Box>
  );
}
