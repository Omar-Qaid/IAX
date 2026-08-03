import React, { memo } from 'react';
import { Box, Checkbox, useTheme, alpha, IconButton, CircularProgress, Tooltip } from '@mui/material';
import { Check as SaveIcon, Close as CancelIcon } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { GridCell } from './GridCell';
import type { ColumnDef } from '../types';
import { GRID_SELECTION_COLUMN_WIDTH } from '../constants';

interface GridRowProps<T> {
  row: T;
  index: number;
  virtualRow: { size: number; start: number };
  getRowId: (row: T) => string | number;
  selectionMode?: 'none' | 'single' | 'multiple';
  isSelected: boolean;
  onToggleRow: (id: string | number) => void;
  onRowClick?: (row: T) => void;
  onRowDoubleClick?: (row: T) => void;
  headerHeight: number;
  rowHeight: number;
  showColumnBorders: boolean;
  showCellBorders: boolean;
  pinnedLeftCols: ColumnDef<T>[];
  unpinnedCols: ColumnDef<T>[];
  pinnedRightCols: ColumnDef<T>[];
  pinnedLeftOffsets: number[];
  pinnedRightOffsets: number[];
  onContextMenu: (e: React.MouseEvent, row: T) => void;
  renderCell: (row: T, col: ColumnDef<T>, rowIndex: number) => React.ReactNode;
  isEditing?: boolean;
  saving?: boolean;
  onSave?: () => void;
  onCancel?: () => void;
  hideInlineEditActions?: boolean;
}

export const GridRow = memo(function GridRowInner<T>({
  row, index, virtualRow, getRowId, selectionMode, isSelected,
  onToggleRow, onRowClick, onRowDoubleClick, headerHeight, rowHeight,
  showColumnBorders, showCellBorders,
  pinnedLeftCols, unpinnedCols, pinnedRightCols, pinnedLeftOffsets, pinnedRightOffsets,
  onContextMenu, renderCell,
  isEditing = false, saving = false, onSave, onCancel, hideInlineEditActions = false,
}: GridRowProps<T>) {
  const theme = useTheme();
  const { t } = useTranslation();
  const rowId = getRowId(row);
  const rowBg = isSelected
    ? alpha(theme.palette.primary.main, theme.palette.mode === 'light' ? 0.14 : 0.22)
    : index % 2 === 1 ? (theme.palette.mode === 'light' ? '#fcfcfc' : alpha(theme.palette.action.hover, 0.04)) : 'background.paper';
  
  const hoverBg = isSelected 
    ? alpha(theme.palette.primary.main, theme.palette.mode === 'light' ? 0.18 : 0.28)
    : theme.palette.mode === 'light' ? '#f3f6fa' : alpha(theme.palette.action.hover, 0.1);

  const editingBg = isEditing
    ? alpha(theme.palette.warning.main, theme.palette.mode === 'light' ? 0.06 : 0.12)
    : undefined;

  const selectionOffset = selectionMode === 'multiple' ? 1 : 0;
  const unpinnedOffset = pinnedLeftCols.length + selectionOffset;
  const rightOffset = pinnedLeftCols.length + unpinnedCols.length + selectionOffset;

  return (
    <Box
      onContextMenu={isEditing ? undefined : e => onContextMenu(e, row)}
      onClick={() => { if (!isEditing) { onToggleRow(rowId); onRowClick?.(row); } }}
      onDoubleClick={() => { if (!isEditing) { onRowDoubleClick?.(row); } }}
      role="row"
      aria-selected={isSelected}
      sx={{
        position: 'absolute', top: 0, left: 0,
        width: 'max-content', minWidth: '100%', height: `${virtualRow.size}px`,
        transform: `translateY(${virtualRow.start - headerHeight}px)`,
        display: 'flex',
        cursor: isEditing ? 'default' : (onRowClick ? 'pointer' : 'default'),
        bgcolor: editingBg ?? rowBg,
        boxShadow: isSelected ? `inset 3px 0 0 ${theme.palette.primary.main}` : 'none',
        transition: theme.transitions.create(['background-color', 'box-shadow'], { duration: theme.transitions.duration.shortest }),
        outline: isEditing ? `2px solid ${theme.palette.warning.main}` : undefined,
        outlineOffset: isEditing ? '-2px' : undefined,
        '&:hover': { bgcolor: isEditing ? editingBg : hoverBg },
      }}
    >
      {selectionMode === 'multiple' && (
        <GridCell
          row={row}
          col={{ field: '_selection', headerName: '', width: GRID_SELECTION_COLUMN_WIDTH } as ColumnDef<T>}
          rowIndex={index}
          rowHeight={rowHeight}
          showColumnBorders={showColumnBorders}
          showCellBorders={showCellBorders}
          isPinned={true}
          offset={0}
          position="left"
          renderCell={() => (
            <Checkbox aria-label={t('grid.select_row', { id: rowId, defaultValue: `Select row ${rowId}` })} size="small" checked={isSelected} onChange={() => onToggleRow(rowId)} onClick={(event) => event.stopPropagation()} sx={{ p: 0, color: 'text.secondary', '&.Mui-checked': { color: 'primary.main' } }} />
          )}
          colIndex={0}
        />
      )}

      {pinnedLeftCols.map((col, i) => (
        <GridCell
          key={col.field as string}
          row={row}
          col={col}
          rowIndex={index}
          rowHeight={rowHeight}
          showColumnBorders={showColumnBorders}
          showCellBorders={showCellBorders}
          isPinned={true}
          offset={pinnedLeftOffsets[i] + (selectionMode === 'multiple' ? GRID_SELECTION_COLUMN_WIDTH : 0)}
          position="left"
          renderCell={renderCell}
          colIndex={i + selectionOffset}
        />
      ))}

      {unpinnedCols.map((col, i) => (
        <GridCell
          key={col.field as string}
          row={row}
          col={col}
          rowIndex={index}
          rowHeight={rowHeight}
          showColumnBorders={showColumnBorders}
          showCellBorders={showCellBorders}
          isPinned={false}
          offset={0}
          position={undefined}
          renderCell={renderCell}
          colIndex={i + unpinnedOffset}
        />
      ))}

      {/* Filler to absorb remaining space and stay aligned with the header filler */}
      <Box sx={{
        flexGrow: 1,
        flexShrink: 0,
        minWidth: 0,
        height: rowHeight,
        borderBottom: `1px solid ${theme.palette.divider}`,
        bgcolor: 'inherit',
      }} />

      {pinnedRightCols.map((col, i) => (
        <GridCell
          key={col.field as string}
          row={row}
          col={col}
          rowIndex={index}
          rowHeight={rowHeight}
          showColumnBorders={showColumnBorders}
          showCellBorders={showCellBorders}
          isPinned={true}
          offset={pinnedRightOffsets[i]}
          position="right"
          renderCell={renderCell}
          colIndex={i + rightOffset}
        />
      ))}

      {isEditing && !hideInlineEditActions && (
        <Box
          sx={{
            position: 'sticky',
            right: 0,
            display: 'flex',
            alignItems: 'center',
            gap: 0.25,
            px: 0.5,
            height: rowHeight,
            bgcolor: editingBg,
            borderBottom: `1px solid ${theme.palette.divider}`,
            borderLeft: `1px solid ${theme.palette.warning.main}`,
            zIndex: 2,
            flexShrink: 0,
          }}
          onClick={e => e.stopPropagation()}
        >
          {saving ? (
            <CircularProgress size={16} thickness={4} />
          ) : (
            <>
              <Tooltip title={t('common.save')}>
                <IconButton aria-label={t('common.save')} size="small" color="success" onClick={onSave} sx={{ p: 0.25 }}>
                  <SaveIcon sx={{ fontSize: 16 }} />
                </IconButton>
              </Tooltip>
              <Tooltip title={t('common.cancel')}>
                <IconButton aria-label={t('common.cancel')} size="small" color="error" onClick={onCancel} sx={{ p: 0.25 }}>
                  <CancelIcon sx={{ fontSize: 16 }} />
                </IconButton>
              </Tooltip>
            </>
          )}
        </Box>
      )}
    </Box>
  );
}) as <T>(props: GridRowProps<T>) => React.ReactElement | null;
