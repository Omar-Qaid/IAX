import { useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import type { DataGridHandle } from '@shared/components/data-grid/types';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import type { MasterDetailRecord } from './types';

export interface MasterDetailDocumentRecord<L extends { id: string }> extends MasterDetailRecord {
  lines: L[];
}

/** Feature adapters own endpoints, mapping, permissions and business validation. */
export interface MasterDetailDocumentSource<T> {
  queryKey: readonly string[];
  get: (company: string, id: string, signal: AbortSignal) => Promise<T>;
  list: (company: string, signal: AbortSignal) => Promise<T[]>;
  save: (company: string, document: T) => Promise<T>;
  validate: (document: T) => void | Promise<void>;
}

/** Default controller for new master-details features. Mount once per company/document. */
export function useMasterDetailDocument<
  L extends { id: string },
  T extends MasterDetailDocumentRecord<L>,
>({
  company,
  documentId,
  source,
  canEdit,
}: {
  company: string;
  documentId: string;
  source: MasterDetailDocumentSource<T>;
  canEdit: boolean;
}) {
  const cache = useQueryClient();
  const documentKey = [...source.queryKey, company, 'document', documentId];
  const listKey = [...source.queryKey, company, 'list'];
  const query = useQuery({
    queryKey: documentKey,
    queryFn: ({ signal }) => source.get(company, documentId, signal),
  });
  const list = useQuery({
    queryKey: listKey,
    queryFn: ({ signal }) => source.list(company, signal),
  });
  const [draft, setDraft] = useState<T | null>(null);
  const draftRef = useRef<T | null>(null);
  const grid = useRef<DataGridHandle>(null);
  const lock = useRef(false);
  const committingGrid = useRef(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [tab, setTab] = useState('lines');
  const [filterVisible, setFilterVisible] = useState(false);
  const editing = draft !== null;
  useUnsavedChanges(editing);

  const change = (document: T | null) => {
    draftRef.current = document;
    setDraft(document);
  };
  const beginEdit = () => {
    if (!canEdit || lock.current || draftRef.current || !query.data) return;
    setError('');
    change(structuredClone(query.data));
  };
  const update = (transform: (document: T) => T) => {
    if ((lock.current && !committingGrid.current) || !canEdit || !draftRef.current) return;
    change(transform(draftRef.current));
  };
  const save = async () => {
    if (lock.current || !canEdit || !draftRef.current) return;
    lock.current = true;
    setBusy(true);
    setError('');
    try {
      committingGrid.current = true;
      try {
        if (grid.current && !(await grid.current.saveEdit())) return;
      } finally {
        committingGrid.current = false;
      }
      const current = draftRef.current!;
      await source.validate(current);
      const saved = await source.save(company, current);
      cache.setQueryData(documentKey, saved);
      cache.setQueryData<T[]>(listKey, (records) =>
        records?.map((record) => (record.id === saved.id ? saved : record))
      );
      change(null);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : String(cause));
    } finally {
      lock.current = false;
      setBusy(false);
    }
  };
  const cancel = () => {
    if (lock.current) return;
    grid.current?.cancelEdit();
    change(null);
    setError('');
  };
  const switchTab = async (value: string) => {
    if (lock.current || (grid.current && !(await grid.current.saveEdit()))) return;
    setTab(value);
  };
  return {
    document: draft ?? query.data,
    records: list.data ?? [],
    loading: query.isLoading,
    loadError: query.error,
    error,
    busy,
    editing,
    grid,
    tab,
    switchTab,
    filterVisible,
    onSearch: async () => {
      if (lock.current || (grid.current && !(await grid.current.saveEdit()))) return;
      setTab('lines');
      setFilterVisible((visible) => !visible);
    },
    beginEdit,
    update,
    save,
    cancel,
    refresh: () => {
      if (!draftRef.current && !lock.current) void Promise.all([query.refetch(), list.refetch()]);
    },
  };
}
