import React from 'react';
import { Box } from '@mui/material';
import { DataGrid } from './DataGrid';
import type { DataGridProps, DataGridHandle } from './types';
import { PATTERN_GRID_DEFAULTS } from './patternGridDefaults';

/** Common invoice-style grid and custom editor layout for page patterns. */
export function PatternDataGrid<T>(
  props: DataGridProps<T> & { ref?: React.Ref<DataGridHandle> }
): React.ReactElement {
  const rowHeight = props.rowHeight ?? PATTERN_GRID_DEFAULTS.rowHeight;
  const columns = React.useMemo(
    () =>
      props.columns.map((column) => {
        const renderEditCell = column.renderEditCell;
        return renderEditCell
          ? {
              ...column,
              renderEditCell: (params: Parameters<NonNullable<typeof renderEditCell>>[0]) => (
                <Box
                  sx={{
                    width: '100%',
                    minWidth: 0,
                    height: rowHeight,
                    display: 'flex',
                    alignItems: 'center',
                  }}
                >
                  {renderEditCell(params)}
                </Box>
              ),
            }
          : column;
      }),
    [props.columns, rowHeight]
  );
  return <DataGrid {...PATTERN_GRID_DEFAULTS} {...props} columns={columns} />;
}
