import { uiDensity } from '@shared/constants/uiDensity';
import React, { memo } from 'react';
import { Box, alpha, useTheme } from '@mui/material';
import type { ColumnDef } from '../types';
import { enterpriseSurfaces } from '@shared/constants/enterpriseSurfaces';
import { cellAlign } from '../columnAlign';

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
  /**
   * Not read here. `renderCell` is a stable callback, so these props are what
   * re-render the cells of the line being edited (values and saving state) while
   * every other cell stays memoized.
   */
  editSnapshot?: unknown;
  saving?: boolean;
}

const GridCellInternal = <T,>({
  row,
  col,
  rowIndex,
  rowHeight,
  showColumnBorders,
  showCellBorders,
  isPinned,
  offset,
  position,
  renderCell,
  colIndex,
}: GridCellProps<T>) => {
  const theme = useTheme();

  return (
    <Box
      tabIndex={-1}
      role="gridcell"
      aria-colindex={colIndex + 1}
      data-field={String(col.field)}
      data-row-index={rowIndex}
      data-col-index={colIndex}
      sx={{
        display: 'flex',
        alignItems: 'center',
        boxSizing: 'border-box',
        p: col.field === '_selection' ? 0 : '0 6px',
        height: rowHeight,
        borderBottom: showCellBorders ? `1px solid ${enterpriseSurfaces(theme).gridLine}` : 'none',
        borderInlineEnd: showColumnBorders
          ? `1px solid ${enterpriseSurfaces(theme).gridLine}`
          : 'none',
        width: col.width || 150,
        minWidth: col.width || 150,
        maxWidth: col.width || 150,
        flex: 'none',
        flexShrink: 0,
        flexGrow: 0,
        fontSize: uiDensity.gridFontSize,
        overflow: 'hidden',
        bgcolor: 'inherit',
        justifyContent:
          col.field === '_selection' || cellAlign(col) === 'center'
            ? 'center'
            : cellAlign(col) === 'left'
              ? theme.direction === 'rtl'
                ? 'flex-end'
                : 'flex-start'
              : cellAlign(col) === 'right'
                ? theme.direction === 'rtl'
                  ? 'flex-start'
                  : 'flex-end'
                : 'flex-start',
        ...(isPinned && {
          position: 'sticky',
          zIndex: 2,
          bgcolor: 'inherit',
          ...(position === 'left'
            ? {
                insetInlineStart: offset,
                borderInlineEnd: showColumnBorders
                  ? `1px solid ${theme.palette.divider}`
                  : undefined,
              }
            : {
                insetInlineEnd: offset,
                borderInlineStart: showColumnBorders
                  ? `1px solid ${theme.palette.divider}`
                  : undefined,
              }),
        }),
        '&:focus': {
          outline: `1px solid ${theme.palette.primary.main}`,
          outlineOffset: '-2px',
          zIndex: 3,
          bgcolor: alpha(theme.palette.primary.main, theme.palette.mode === 'light' ? 0.06 : 0.12),
        },
      }}
    >
      {renderCell(row, col, rowIndex)}
    </Box>
  );
};

export const GridCell = memo(GridCellInternal) as typeof GridCellInternal;
