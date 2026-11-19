import { uiDensity } from '@shared/constants/uiDensity';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton, useTheme } from '@mui/material';
import ArrowUpward from '@mui/icons-material/ArrowUpward';
import ArrowDownward from '@mui/icons-material/ArrowDownward';
import KeyboardArrowDown from '@mui/icons-material/KeyboardArrowDown';
import DragIndicator from '@mui/icons-material/DragIndicator';
import FilterIcon from '@mui/icons-material/FilterList';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { ColumnDef, SortModel, FilterModel } from '../types';
import { FilterInput } from './FilterInput';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';
import { headerAlign } from '../columnAlign';

interface SortableHeaderProps<T> {
  column: ColumnDef<T>;
  sortModel: SortModel[];
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
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

export function SortableHeader<T>({
  column,
  sortModel,
  onSort,
  filters,
  onFilterChange,
  onFilterIconClick,
  onMenuOpen,
  onResizeStart,
  headerHeight,
  showColumnBorders,
  hideFilterRow = false,
  hideColumnMenu = false,
}: SortableHeaderProps<T>) {
  const { t } = useTranslation();
  const theme = useTheme();
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: column.field as string,
  });

  const sort = sortModel.find((s) => s.field === column.field);

  return (
    <Box
      ref={setNodeRef}
      style={{
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 10 : 1,
        position: 'relative',
        display: 'flex',
        flexDirection: 'column',
        boxSizing: 'border-box',
        backgroundColor: 'background.paper',
        borderInlineEnd: showColumnBorders
          ? `1px solid ${enterpriseSurfaces(theme).gridLine}`
          : 'none',
        width: column.width || 150,
        minWidth: column.width || 150,
        maxWidth: column.width || 150,
        flex: 'none',
        flexShrink: 0,
        flexGrow: 0,
        overflow: 'hidden',
      }}
    >
      {/* Name row - drag handle */}
      <Box
        {...attributes}
        {...listeners}
        role="columnheader"
        aria-sort={
          sort?.sort === 'asc' ? 'ascending' : sort?.sort === 'desc' ? 'descending' : 'none'
        }
        onKeyDown={(event) => {
          if (
            event.target === event.currentTarget &&
            event.key === 'Enter' &&
            !isDragging &&
            column.sortable !== false
          ) {
            event.preventDefault();
            event.stopPropagation();
            onSort(String(column.field));
          } else listeners?.onKeyDown?.(event);
        }}
        onClick={() => {
          if (!isDragging && column.sortable !== false) onSort(column.field as string);
        }}
        sx={{
          display: 'flex',
          alignItems: 'center',
          // Same inline padding as the body cells so headers line up with values.
          p: '3px var(--data-grid-cell-padding, 6px)',
          borderBottom: `1px solid ${enterpriseSurfaces(theme).gridLine}`,
          justifyContent:
            headerAlign(column) === 'center'
              ? 'center'
              : headerAlign(column) === 'right'
                ? 'flex-end'
                : 'flex-start',
          cursor: column.sortable === false ? 'default' : 'pointer',
          userSelect: 'none',
          position: 'relative',
          height: headerHeight,
          minHeight: headerHeight,
          boxSizing: 'border-box',
          '&:hover .column-header-action, &:focus-within .column-header-action': { opacity: 1 },
          // Only the hovered header label moves; body cells keep their alignment.
          '&:hover .MuiTypography-root, &:focus-visible .MuiTypography-root': {
            paddingLeft: '8px',
            paddingRight: hideColumnMenu ? (sort ? '16px' : 0) : sort ? '38px' : '22px',
          },
          '@media (hover: none)': { '& .column-header-action': { opacity: 1 } },
          '&:hover .column-sort-indicator, &:focus-within .column-sort-indicator': {
            right: hideColumnMenu ? 6 : 26,
          },
          '&:hover': {
            bgcolor: theme.palette.mode === 'light' ? '#e9e8e7' : theme.palette.action.hover,
          },
        }}
      >
        <DragIndicator
          className="column-header-action"
          aria-hidden="true"
          sx={{
            fontSize: 15,
            color: 'text.secondary',
            opacity: isDragging ? 1 : 0,
            // Overlays the start padding so the header text lines up with the
            // cell values below it.
            position: 'absolute',
            insetInlineStart: -2,
            top: '50%',
            transform: 'translateY(-50%)',
            pointerEvents: 'none',
          }}
        />
        <Typography
          variant="subtitle2"
          sx={{
            flexGrow: 1,
            minWidth: 0,
            width: '100%',
            fontWeight: 600,
            color: 'text.primary',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            fontFamily: APP_FONT_FAMILY,
            fontSize: uiDensity.gridFontSize,
            letterSpacing: 0,
            textAlign: headerAlign(column) ?? 'start',
            boxSizing: 'border-box',
            paddingLeft: 0,
            paddingRight: sort ? '16px' : 0,
          }}
        >
          {t(column.headerName || '')}
        </Typography>
        {sort?.sort === 'asc' && (
          <ArrowUpward
            className="column-sort-indicator"
            sx={{
              fontSize: 12,
              position: 'absolute',
              right: 6,
              color: 'primary.main',
            }}
          />
        )}
        {sort?.sort === 'desc' && (
          <ArrowDownward
            className="column-sort-indicator"
            sx={{
              fontSize: 12,
              position: 'absolute',
              right: 6,
              color: 'primary.main',
            }}
          />
        )}
        {!hideColumnMenu && (
          <IconButton
            size="small"
            aria-label={t('grid.column_menu', { column: t(column.headerName || '') })}
            className="column-header-action"
            sx={{
              p: 0.25,
              position: 'absolute',
              right: 4,
              top: '50%',
              transform: 'translateY(-50%)',
              opacity: 0,
              flexShrink: 0,
              color: 'text.primary',
              // Covers the text end it overlays, on the header hover color.
              bgcolor: theme.palette.mode === 'light' ? '#e9e8e7' : theme.palette.action.hover,
              '&:hover .column-sort-indicator, &:focus-within .column-sort-indicator': {
                right: hideColumnMenu ? 6 : 26,
              },
              '&:hover': {
                bgcolor: theme.palette.mode === 'light' ? '#dcdad8' : theme.palette.action.selected,
              },
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
            borderBottom: `1px solid ${theme.palette.divider}`,
            bgcolor: theme.palette.background.paper,
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
        onMouseDown={(e) => onResizeStart(e, column.field as string, 'inline-end')}
        sx={{
          position: 'absolute',
          top: 0,
          bottom: 0,
          width: 6,
          cursor: 'col-resize',
          zIndex: 10,
          insetInlineEnd: 0,
          '&:hover .column-sort-indicator, &:focus-within .column-sort-indicator': {
            right: hideColumnMenu ? 6 : 26,
          },
          '&:hover': { bgcolor: 'primary.main', opacity: 0.65 },
        }}
      />
    </Box>
  );
}
