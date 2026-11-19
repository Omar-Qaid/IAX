import type { ColumnDef } from './types';

type Align = 'left' | 'center' | 'right';

/**
 * Alignment of a column's values: the column's own `align`, otherwise numbers
 * on the right and text on the left. Every grid gets
 * this by default; a page overrides it with `align`.
 */
export const cellAlign = <T>(col: Pick<ColumnDef<T>, 'align' | 'type'>): Align | undefined =>
  col.align ?? (col.type === 'number' ? 'right' : 'left');

/** Header alignment follows the values unless `headerAlign` is set. */
export const headerAlign = <T>(
  col: Pick<ColumnDef<T>, 'align' | 'headerAlign' | 'type'>
): Align | undefined => col.headerAlign ?? cellAlign(col);

/**
 * Marks columns without `type`/`align` as numeric when their first non-empty
 * value is a number, so numeric columns are right-aligned without per-page
 * configuration. Returns the same array when nothing changes.
 */
export const inferNumericColumns = <T>(
  columns: ColumnDef<T>[],
  rows: readonly T[]
): ColumnDef<T>[] => {
  let changed = false;
  const next = columns.map((col) => {
    if (col.align || col.type) return col;
    for (const row of rows) {
      const value = col.valueGetter
        ? col.valueGetter({ row })
        : (row as Record<string, unknown>)[String(col.field)];
      if (value == null || value === '') continue;
      if (typeof value !== 'number') return col;
      changed = true;
      // Alignment only: the column's filter and editor types are unchanged.
      return { ...col, align: 'right' as const };
    }
    return col;
  });
  return changed ? next : columns;
};
