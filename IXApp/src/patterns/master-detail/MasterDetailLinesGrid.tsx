import React from 'react';

import { PatternDataGrid } from '@shared/components/data-grid/PatternDataGrid';
import type { DataGridHandle, DataGridProps } from '@shared/components/data-grid/types';

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
  const allowEditRequest = canEdit && !editing && !busy;
  const latest = React.useRef({ grid, onEdit });
  React.useLayoutEffect(() => {
    latest.current = { grid, onEdit };
  });
  // Pages usually pass inline handlers; hand the grid stable wrappers that call the
  // latest handler so it does not re-render on every page render. A wrapper exists
  // only while the page passes that handler (undefined still means "not handled").
  const handlerKeys = Object.keys(grid)
    .filter((key) => key.startsWith('on') && typeof grid[key as keyof typeof grid] === 'function')
    .sort()
    .join(',');
  const handlers = React.useMemo(() => {
    const wrapped: Record<string, (...args: unknown[]) => unknown> = {};
    for (const key of handlerKeys ? handlerKeys.split(',') : []) {
      wrapped[key] = (...args) =>
        (latest.current.grid[key as keyof typeof grid] as (...a: unknown[]) => unknown)(...args);
    }
    return wrapped as Partial<DataGridProps<T>>;
  }, [handlerKeys]);
  const requestEdit = React.useCallback(() => latest.current.onEdit(), []);
  return (
    <PatternDataGrid<T>
      {...grid}
      {...handlers}
      ref={ref}
      masterForm={editing && canEdit}
      editDisabled={busy}
      onCellEditRequest={allowEditRequest ? requestEdit : undefined}
    />
  );
}
