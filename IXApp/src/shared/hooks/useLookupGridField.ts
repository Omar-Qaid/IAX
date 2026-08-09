import { useMemo } from 'react';
import { useInfiniteQuery, type QueryKey } from '@tanstack/react-query';
import { useDebounce } from './useDebounce';
import type { FetchPageFn } from '../components/lookups/types';

export interface UseLookupGridFieldParams<T> {
  queryKey: QueryKey;
  fetchPage: FetchPageFn<T>;
  enabled?: boolean;
  pageSize?: number;
  search?: string;
  debounceMs?: number;
}

export function useLookupGridField<T extends object>({
  queryKey,
  fetchPage,
  enabled = true,
  pageSize = 50,
  search = '',
  debounceMs = 300,
}: UseLookupGridFieldParams<T>) {
  const debouncedSearch = useDebounce(search, debounceMs);

  const fullQueryKey = useMemo(
    () => [...queryKey, 'grid-lookup', debouncedSearch, pageSize],
    [queryKey, debouncedSearch, pageSize]
  );

  const query = useInfiniteQuery({
    queryKey: fullQueryKey,
    queryFn: ({ pageParam = 1, signal }) =>
      fetchPage({
        pageNumber: pageParam as number,
        pageSize,
        search: debouncedSearch,
        signal,
      }),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => {
      if (!lastPage || lastPage.pageNumber >= lastPage.totalPages) {
        return undefined;
      }
      return lastPage.pageNumber + 1;
    },
    enabled,
    staleTime: 1000 * 60 * 2,
  });

  const rows = useMemo(() => {
    if (!query.data?.pages) return [];
    return query.data.pages.flatMap((page) => page.data || []);
  }, [query.data]);

  const totalRecords = useMemo(() => {
    if (!query.data?.pages?.[0]) return 0;
    return query.data.pages[0].totalRecords ?? rows.length;
  }, [query.data, rows.length]);

  return {
    rows,
    totalRecords,
    debouncedSearch,
    isLoading: query.isLoading,
    isFetching: query.isFetching,
    isFetchingNextPage: query.isFetchingNextPage,
    hasNextPage: !!query.hasNextPage,
    fetchNextPage: query.fetchNextPage,
    refetch: query.refetch,
  };
}

export const useGridLookupData = useLookupGridField; // Backward compatibility alias
