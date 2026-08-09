import type { GridLookupColumn } from '../components/lookups/types';

export function filterLocalizedColumns<T extends object>(
  columns: GridLookupColumn<T>[],
  isRtl: boolean
): GridLookupColumn<T>[] {
  if (!columns || !Array.isArray(columns)) return [];

  return columns
    .filter((col) => {
      if (col.hidden) return false;
      if (isRtl && col.showInRtl === false) return false;
      if (!isRtl && col.showInLtr === false) return false;
      return true;
    })
    .map((col) => {
      const header = isRtl ? col.headerAr || col.header : col.headerEn || col.header;
      return {
        ...col,
        header,
      };
    });
}
