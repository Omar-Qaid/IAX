import { useQuery } from '@tanstack/react-query';
import type { SimpleListDataSource } from './types';

interface SimpleListDataState<T> {
  rows: T[];
  loading: boolean;
  error: string | null;
  refresh: () => void;
}

export function useSimpleListDataSource<T>(source: SimpleListDataSource<T>): SimpleListDataState<T> {
  const sourceType = source.type;
  const sourceKey = source.type === 'remote' ? source.key : source.type;
  const remote = useQuery({
    queryKey: ['simple-list', sourceKey],
    queryFn: ({ signal }) => source.type === 'remote' ? source.load(signal) : Promise.resolve([] as T[]),
    enabled: sourceType === 'remote',
    initialData: source.type === 'remote' ? source.initialRows : undefined,
  });

  if (source.type === 'static') return { rows: source.rows, loading: false, error: null, refresh: () => undefined };
  if (source.type === 'controlled') return { rows: source.rows, loading: source.loading ?? false, error: source.error ?? null, refresh: source.refresh ?? (() => undefined) };
  return {
    rows: remote.data ?? [],
    loading: remote.isLoading || remote.isFetching,
    error: remote.error instanceof Error ? remote.error.message : remote.error ? String(remote.error) : null,
    refresh: () => { void remote.refetch(); },
  };
}
