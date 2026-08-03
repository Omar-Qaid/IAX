import { useCallback, useMemo, useState } from 'react';

export interface UseSimpleListPageOptions<T> {
  rows: T[];
  getRowId: (row: T) => string | number;
  matchesSearch?: (row: T, query: string) => boolean;
}

export function useSimpleListPage<T>({ rows, getRowId, matchesSearch }: UseSimpleListPageOptions<T>) {
  const [query, setQuery] = useState('');
  const [selectedIds, setSelectedIds] = useState<Array<string | number>>([]);
  const filteredRows = useMemo(() => {
    const normalized = query.trim().toLocaleLowerCase();
    if (!normalized) return rows;
    return rows.filter(row => matchesSearch?.(row, normalized) ?? JSON.stringify(row).toLocaleLowerCase().includes(normalized));
  }, [matchesSearch, query, rows]);
  const selectedRows = useMemo(() => {
    const ids = new Set(selectedIds);
    return rows.filter(row => ids.has(getRowId(row)));
  }, [getRowId, rows, selectedIds]);
  const clearSelection = useCallback(() => setSelectedIds([]), []);
  return { query, setQuery, filteredRows, selectedIds, setSelectedIds, selectedRows, clearSelection };
}
