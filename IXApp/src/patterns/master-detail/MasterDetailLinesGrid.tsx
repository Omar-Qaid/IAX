import React from 'react';
import { Box } from '@mui/material';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { DataGridHandle, DataGridProps } from '@shared/components/data-grid/types';
import { MASTER_DETAIL_LINES_GRID_PROPS } from './MasterDetailLines';

export type MasterDetailLinesGridProps<T> = DataGridProps<T> & {
  ref?: React.Ref<DataGridHandle>;
  /** The page is in edit mode: the selected row is edited in place. */
  editing: boolean;
  /** Saving/posting: no editing. */
  busy?: boolean;
  /** The user may edit this document (permission, not posted...). */
  canEdit: boolean;
  /** Switches the page to edit mode (same as the Edit button). */
  onEdit: () => void;
};

/**
 * Lines grid of a master-detail page: the free-text invoice appearance and the D365 edit
 * flow for every page. In edit mode rows are edited in place; in view mode
 * editable cells show a pencil that switches the page to edit mode and edits
 * that cell. Page grid props (columns, rows, storageKey, handlers) pass through
 * and may override the look.
 */
export function MasterDetailLinesGrid<T>({
  editing,
  busy = false,
  canEdit,
  onEdit,
  ref,
  ...grid
}: MasterDetailLinesGridProps<T>): React.ReactElement {
  const rowHeight = grid.rowHeight ?? MASTER_DETAIL_LINES_GRID_PROPS.rowHeight;
  const columns = React.useMemo(
    () =>
      grid.columns.map((column) => {
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
    [grid.columns, rowHeight]
  );
  return (
    <DataGrid<T>
      {...MASTER_DETAIL_LINES_GRID_PROPS}
      {...grid}
      columns={columns}
      ref={ref}
      masterForm={editing && !busy}
      onCellEditRequest={canEdit && !editing && !busy ? () => onEdit() : undefined}
    />
  );
}
