import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, IconButton } from '@mui/material';
import MoreVert from '@mui/icons-material/MoreVert';
import FilterIcon from '@mui/icons-material/FilterList';
import type { ColumnDef, FilterModel } from '../types';
import { FilterInput } from './FilterInput';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

interface PinnedHeaderCellProps<T> {
  column: ColumnDef<T>;
  offset: number;
  side: 'left' | 'right';
  filters: FilterModel[];
  onFilterChange: (field: string, value: string) => void;
  onFilterIconClick: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onMenuOpen: (event: React.MouseEvent<HTMLElement>, column: ColumnDef<T>) => void;
  onResizeStart: (event: React.MouseEvent, field: string) => void;
  showColumnBorders?: boolean;
  hideFilterRow?: boolean;
  hideColumnMenu?: boolean;
}

export function PinnedHeaderCell<T>({
  column,
  offset,
  side,
  filters,
  onFilterChange,
  onFilterIconClick,
  onMenuOpen,
  onResizeStart,
  showColumnBorders = false,
  hideFilterRow = false,
  hideColumnMenu = false,
}: PinnedHeaderCellProps<T>) {
  const { t } = useTranslation();
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
          ? { left: offset, borderRight: (theme) => showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none' }
          : { right: offset, borderLeft: (theme) => showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none' }),
      }}
    >
      {/* Name row */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          p: '3px 8px',
          borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
          position: 'relative',
          height: 32,
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
          }}
        >
          {t(column.headerName || '')}
        </Typography>
        {!hideColumnMenu && (
          <IconButton
            size="small"
            sx={{ p: 0.25, marginInlineStart: 0.25 }}
            onClick={(e) => onMenuOpen(e, column)}
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
            height: 32,
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
        onMouseDown={(e) => onResizeStart(e, column.field as string)}
        sx={{
          position: 'absolute',
          top: 0,
          bottom: 0,
          width: 6,
          cursor: 'col-resize',
          zIndex: 10,
          ...(side === 'left' ? { right: 0 } : { left: 0 }),
          '&:hover': { bgcolor: 'primary.main', opacity: 0.5 },
        }}
      />
    </Box>
  );
}
