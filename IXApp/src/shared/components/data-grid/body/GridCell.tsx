import React, { memo } from 'react';
import { Box, alpha, useTheme } from '@mui/material';
import type { ColumnDef } from '../types';

interface GridCellProps<T> {
  row: T;
  col: ColumnDef<T>;
  rowIndex: number;
  rowHeight: number;
  showColumnBorders: boolean;
  showCellBorders: boolean;
  isPinned: boolean;
  offset: number;
  position: 'left' | 'right' | undefined;
  renderCell: (row: T, col: ColumnDef<T>, rowIndex: number) => React.ReactNode;
  colIndex: number;
}

const GridCellInternal = <T,>({
  row, col, rowIndex, rowHeight, showColumnBorders, showCellBorders,
  isPinned, offset, position, renderCell, colIndex
}: GridCellProps<T>) => {
  const theme = useTheme();

  return (
    <Box
      tabIndex={-1}
      role="gridcell"
      data-row-index={rowIndex}
      data-col-index={colIndex}
      sx={{
        display: 'flex',
        alignItems: 'center',
        boxSizing: 'border-box',
        p: col.field === '_selection' ? 0 : '0 12px',
        height: rowHeight,
        borderBottom: showCellBorders ? `1px solid ${theme.palette.divider}` : 'none',
        borderInlineEnd: showColumnBorders ? `1px solid ${theme.palette.divider}` : 'none',
        width: col.width || 150,
        minWidth: col.width || 150,
        maxWidth: col.width || 150,
        flex: 'none',
        flexShrink: 0,
        flexGrow: 0,
        overflow: 'hidden',
        bgcolor: 'inherit',
        justifyContent: col.field === '_selection' || col.align === 'center'
          ? 'center'
          : col.align === 'left'
            ? theme.direction === 'rtl' ? 'flex-end' : 'flex-start'
            : col.align === 'right'
              ? theme.direction === 'rtl' ? 'flex-start' : 'flex-end'
              : 'flex-start',
        ...(isPinned && {
          position: 'sticky',
          zIndex: 2,
          bgcolor: 'inherit',
          ...(position === 'left'
            ? { insetInlineStart: offset, borderInlineEnd: showColumnBorders ? `1px solid ${theme.palette.divider}` : undefined }
            : { insetInlineEnd: offset, borderInlineStart: showColumnBorders ? `1px solid ${theme.palette.divider}` : undefined }),
        }),
        '&:focus': {
          outline: `1px solid ${theme.palette.primary.main}`,
          outlineOffset: '-2px',
          zIndex: 3,
          bgcolor: alpha(theme.palette.primary.main, theme.palette.mode === 'light' ? 0.06 : 0.12),
        }
      }}
    >
      {renderCell(row, col, rowIndex)}
    </Box>
  );
};

export const GridCell = memo(GridCellInternal) as typeof GridCellInternal;
