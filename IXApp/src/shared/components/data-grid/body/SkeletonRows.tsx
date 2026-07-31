import { Box, Skeleton } from '@mui/material';
import type { ColumnDef } from '../types';

interface SkeletonRowsProps<T> {
  rowHeight: number;
  pinnedLeftCols: ColumnDef<T>[];
  unpinnedCols: ColumnDef<T>[];
  pinnedRightCols: ColumnDef<T>[];
  visibleColumnsCount: number;
}

const SKELETON_ROW_COUNT = 10;
const SKELETON_WIDTHS = ['65%', '80%', '50%', '72%', '45%', '78%', '60%', '85%', '55%', '70%'];
const skW = (row: number, col: number) => SKELETON_WIDTHS[(row * 3 + col * 7) % SKELETON_WIDTHS.length];

export function SkeletonRows<T>({
  rowHeight, pinnedLeftCols, unpinnedCols, pinnedRightCols, visibleColumnsCount
}: SkeletonRowsProps<T>) {
  return (
    <Box>
      {Array.from({ length: SKELETON_ROW_COUNT }).map((_, rowIdx) => (
        <Box key={rowIdx} sx={{ display: 'flex', height: rowHeight, bgcolor: rowIdx % 2 === 1 ? '#fafafa' : '#fff', borderBottom: '1px solid #f0f0f0' }}>
          {pinnedLeftCols.map((col, colIdx) => (
            <Box key={col.field as string} sx={{ width: col.width || 150, minWidth: col.minWidth || 50, px: 1.5, display: 'flex', alignItems: 'center', flexShrink: 0 }}>
              <Skeleton variant="text" width={skW(rowIdx, colIdx)} sx={{ fontSize: '0.8rem', borderRadius: 1 }} />
            </Box>
          ))}
          <Box sx={{ display: 'flex', flexGrow: 1 }}>
            {unpinnedCols.map((col, colIdx) => (
              <Box key={col.field as string} sx={{ width: col.width || 150, minWidth: col.minWidth || 50, px: 1.5, display: 'flex', alignItems: 'center', flexShrink: 0 }}>
                <Skeleton variant="text" width={skW(rowIdx, colIdx + pinnedLeftCols.length)} sx={{ fontSize: '0.8rem', borderRadius: 1 }} />
              </Box>
            ))}
          </Box>
          {pinnedRightCols.map((col, colIdx) => (
            <Box key={col.field as string} sx={{ width: col.width || 150, minWidth: col.minWidth || 50, px: 1.5, display: 'flex', alignItems: 'center', flexShrink: 0 }}>
              <Skeleton variant="text" width={skW(rowIdx, colIdx + visibleColumnsCount - pinnedRightCols.length)} sx={{ fontSize: '0.8rem', borderRadius: 1 }} />
            </Box>
          ))}
        </Box>
      ))}
    </Box>
  );
}
