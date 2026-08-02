import { useCallback, useEffect, useRef, useState } from 'react';
import type { SimpleListDataSource } from './types';

interface SimpleListDataState<T> {
  rows: T[];
  loading: boolean;
  error: string | null;
  refresh: () => void;
}

export function useSimpleListDataSource<T>(source: SimpleListDataSource<T>): SimpleListDataState<T> {
  const remoteLoader = source.type === 'remote' ? source.load : null;
  const loaderRef = useRef(remoteLoader);
  const [remoteRows, setRemoteRows] = useState<T[]>(source.type === 'remote' ? (source.initialRows ?? []) : []);
  const [remoteLoading, setRemoteLoading] = useState(source.type === 'remote');
  const [remoteError, setRemoteError] = useState<string | null>(null);
  const [reloadVersion, setReloadVersion] = useState(0);
  const refreshRemote = useCallback(() => setReloadVersion((version) => version + 1), []);

  useEffect(() => {
    loaderRef.current = remoteLoader;
  }, [remoteLoader]);

  const sourceType = source.type;
  const sourceKey = source.type === 'remote' ? source.key : source.type;
  useEffect(() => {
    if (sourceType !== 'remote' || !loaderRef.current) return;
    const controller = new AbortController();
    setRemoteLoading(true);
    setRemoteError(null);
    void loaderRef.current(controller.signal)
      .then((rows) => { if (!controller.signal.aborted) setRemoteRows(rows); })
      .catch((error: unknown) => { if (!controller.signal.aborted) setRemoteError(error instanceof Error ? error.message : String(error)); })
      .finally(() => { if (!controller.signal.aborted) setRemoteLoading(false); });
    return () => controller.abort();
  }, [reloadVersion, sourceKey, sourceType]);

  if (source.type === 'static') return { rows: source.rows, loading: false, error: null, refresh: () => undefined };
  if (source.type === 'controlled') return { rows: source.rows, loading: source.loading ?? false, error: source.error ?? null, refresh: source.refresh ?? (() => undefined) };
  return { rows: remoteRows, loading: remoteLoading, error: remoteError, refresh: refreshRemote };
}
