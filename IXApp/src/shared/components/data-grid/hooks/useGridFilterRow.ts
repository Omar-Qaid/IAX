import { useLocalStorage } from '@shared/hooks/useLocalStorage';

/** Stores the filter row preference independently from the grid's filter values. */
export function useGridFilterRow(storageKey: string, initiallyVisible = false) {
  return useLocalStorage(`ixapp.grid.filter-row.${storageKey}`, initiallyVisible);
}
