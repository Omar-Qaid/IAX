import { useState, useEffect, useRef, useCallback } from 'react';
import type { SortModel, FilterModel, FetchRowsParams } from '../types';

interface UseGridDataSourceOptions {
  enabled: boolean;
  initialSort: SortModel[];
  initialFilters: FilterModel[];
  pageSize: number;
  /** Rows loaded so far; the next page index is derived from it. */
  loadedRowCount?: number;
  onFetchRows?: (params: FetchRowsParams) => void;
  onScrollReset: () => void;
}

export interface GridDataSourceHandle {
  sortModel: SortModel[];
  setSortModel: React.Dispatch<React.SetStateAction<SortModel[]>>;
  filters: FilterModel[];
  setFilters: React.Dispatch<React.SetStateAction<FilterModel[]>>;
  globalSearch: string;
  setGlobalSearch: (v: string) => void;
  handleSort: (field: string, direction?: 'asc' | 'desc') => void;
  loadNextPage: () => void;
}

export function useGridDataSource({
  enabled,
  initialSort,
  initialFilters,
  pageSize,
  loadedRowCount,
  onFetchRows,
  onScrollReset,
}: UseGridDataSourceOptions): GridDataSourceHandle {
  const [sortModel, setSortModel] = useState<SortModel[]>(initialSort);
  const [filters, setFilters] = useState<FilterModel[]>(initialFilters);
  const [globalSearch, setGlobalSearch] = useState('');

  const paramsRef = useRef({ sortModel, filters, globalSearch, pageSize });
  const pageRef = useRef(0);
  const abortRef = useRef<AbortController | null>(null);
  const onFetchRowsRef = useRef(onFetchRows);
  const scrollResetRef = useRef(onScrollReset);
  const loadedRowCountRef = useRef(loadedRowCount);

  useEffect(() => {
    loadedRowCountRef.current = loadedRowCount;
    paramsRef.current = { sortModel, filters, globalSearch, pageSize };
    onFetchRowsRef.current = onFetchRows;
    scrollResetRef.current = onScrollReset;
  });

  const emit = useCallback((isFirstPage: boolean) => {
    const { sortModel, filters, globalSearch, pageSize } = paramsRef.current;

    abortRef.current?.abort('superseded');
    const controller = new AbortController();
    abortRef.current = controller;

    if (isFirstPage) {
      pageRef.current = 0;
      scrollResetRef.current();
    }

    if (onFetchRowsRef.current) {
      onFetchRowsRef.current({
        sort: sortModel,
        filters,
        globalSearch,
        page: pageRef.current,
        pageSize,
        isFirstPage,
        signal: controller.signal,
      });
    }
  }, []);

  const isMountRef = useRef(true);
  const prevSortRef = useRef(sortModel);

  useEffect(() => {
    if (!enabled) return;

    const isMount = isMountRef.current;
    const sortChanged = !isMount && sortModel !== prevSortRef.current;
    isMountRef.current = false;
    prevSortRef.current = sortModel;

    if (isMount) return;

    const delay = sortChanged ? 0 : 300;
    const timerId = setTimeout(() => emit(true), delay);
    return () => clearTimeout(timerId);
  }, [enabled, sortModel, filters, globalSearch, emit]);

  useEffect(
    () => () => {
      abortRef.current?.abort('unmount');
    },
    []
  );

  const loadNextPage = useCallback(() => {
    if (!enabled) return;
    // Derive the page from what actually arrived: a failed request is asked
    // for again instead of being skipped by a counter that already moved on.
    const loaded = loadedRowCountRef.current;
    const { pageSize } = paramsRef.current;
    pageRef.current =
      loaded !== undefined && pageSize > 0 ? Math.floor(loaded / pageSize) : pageRef.current + 1;
    emit(false);
  }, [enabled, emit]);

  const handleSort = useCallback((field: string, direction?: 'asc' | 'desc') => {
    setSortModel((prev) => {
      if (direction) return [{ field, sort: direction }];
      const existing = prev.find((s) => s.field === field);
      if (!existing) return [{ field, sort: 'asc' }];
      if (existing.sort === 'asc') return [{ field, sort: 'desc' }];
      return [];
    });
  }, []);

  return {
    sortModel,
    setSortModel,
    filters,
    setFilters,
    globalSearch,
    setGlobalSearch,
    handleSort,
    loadNextPage,
  };
}
