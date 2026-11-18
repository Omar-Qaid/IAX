import React from 'react';
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
 * Lines grid of a master-detail page: the sales order look and the D365 edit
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
  return (
    <DataGrid<T>
      {...MASTER_DETAIL_LINES_GRID_PROPS}
      {...grid}
      ref={ref}
      masterForm={editing && !busy}
      onCellEditRequest={canEdit && !editing && !busy ? () => onEdit() : undefined}
    />
  );
}
