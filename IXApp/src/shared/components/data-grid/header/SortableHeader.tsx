import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton, useTheme } from '@mui/material';
import { ArrowUpward, ArrowDownward, MoreVert, FilterList as FilterIcon } from '@mui/icons-material';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import type { ColumnDef, SortModel, FilterModel } from '../Types';
import { FilterInput } from './FilterInput';

interface SortableHeaderProps<T> {
  column: ColumnDef<T>;
  sortModel: SortModel[];
  onSort: (field: string, direction?: 'asc' | 'desc') => void;
  filters: FilterModel[];
  onFilterChange: (field: string, value: string) => void;
  onFilterIconClick: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onMenuOpen: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onResizeStart: (event: React.MouseEvent, field: string) => void;
  showColumnBorders?: boolean;
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
}

export function SortableHeader<T>({
  column, sortModel, onSort, filters, onFilterChange,
  onFilterIconClick, onMenuOpen, onResizeStart, showColumnBorders,
  hideFilterRow = false, hideColumnMenu = false,
}: SortableHeaderProps<T>) {
  const { t } = useTranslation();
  const theme = useTheme();
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: column.field as string,
  });

  const sort = sortModel.find(s => s.field === column.field);

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
        backgroundColor: theme.palette.mode === 'light' ? '#f8f9fa' : '#1a202c',
        borderRight: showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none',
        width: column.width || 150,
        minWidth: column.width || 150,
        maxWidth: column.width || 150,
        flex: 'none',
        flexShrink: 0,
        flexGrow: 0,
        overflow: 'hidden',
      }}
    >
      {/* Name row — drag handle */}
      <Box
        {...attributes} {...listeners}
        onClick={() => { if (!isDragging && column.sortable !== false) onSort(column.field as string); }}
        sx={{
          display: 'flex', alignItems: 'center', p: '4px 12px',
          borderBottom: `1px solid ${theme.palette.divider}`,
          justifyContent: column.headerAlign === 'center' ? 'center' : column.headerAlign === 'right' ? 'flex-end' : 'flex-start',
          cursor: 'grab', userSelect: 'none', position: 'relative', height: 36,
          '&:hover': { bgcolor: theme.palette.action.hover }
        }}
      >
        <Typography variant="subtitle2" sx={{ 
          flexGrow: 1, 
          fontWeight: 700, 
          color: 'text.primary', 
          overflow: 'hidden', 
          textOverflow: 'ellipsis', 
          whiteSpace: 'nowrap', 
          fontSize: '0.75rem',
          letterSpacing: '0.02em',
        }}>
          {t(column.headerName || '')}
        </Typography>
        {sort?.sort === 'asc'  && <ArrowUpward   sx={{ fontSize: 12, ml: 0.25, color: 'primary.main' }} />}
        {sort?.sort === 'desc' && <ArrowDownward  sx={{ fontSize: 12, ml: 0.25, color: 'primary.main' }} />}
        {!hideColumnMenu && (
          <IconButton size="small" sx={{ p: 0.25, ml: 0.25 }} onClick={(e) => { e.stopPropagation(); onMenuOpen(e, column); }}>
            <MoreVert sx={{ fontSize: 14 }} />
          </IconButton>
        )}
      </Box>

      {/* Filter row */}
      {!hideFilterRow && (
        <Box sx={{
          p: '4px 12px',
          borderBottom: `1px solid ${theme.palette.divider}`,
          bgcolor: theme.palette.mode === 'light' ? '#ffffff' : '#2d3748',
          display: 'flex',
          alignItems: 'center',
          height: 36
        }}>
          {column.filterable === false
            ? <FilterIcon sx={{ fontSize: 12, color: 'action.disabled' }} />
            : <FilterInput column={column} filters={filters} onFilterChange={onFilterChange} onFilterIconClick={onFilterIconClick} />
          }
        </Box>
      )}

      {/* Resize handle */}
      <Box
        onMouseDown={(e) => onResizeStart(e, column.field as string)}
        sx={{
          position: 'absolute', top: 0, bottom: 0, width: 6,
          cursor: 'col-resize', zIndex: 10,
          ...(theme.direction === 'rtl' ? { left: 0 } : { right: 0 }),
          '&:hover': { bgcolor: 'primary.main', opacity: 0.5 },
        }}
      />
    </Box>
  );
}
