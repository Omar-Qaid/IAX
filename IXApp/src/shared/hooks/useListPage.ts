import { useState, useCallback, useEffect } from 'react';
import { useNotifications } from './useNotifications';
import { usePageRefresh } from './usePageRefresh';
import { useUnsavedChanges } from './useUnsavedChanges';
import { usePageMode, PageMode } from './usePageMode';

export interface UseListPageOptions<T> {
  initialMode?: PageMode;
  loadData: () => Promise<T[]>;
  saveData?: (data: T[]) => Promise<void>;
  deleteData?: (id: string) => Promise<void>;
}

export function useListPage<T>({
  initialMode = 'view',
  loadData,
  saveData,
  deleteData
}: UseListPageOptions<T>) {
  const [data, setData] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [isDirty, setIsDirty] = useState(false);
  
  const { notifySuccess, notifyError } = useNotifications();
  const pageMode = usePageMode(initialMode);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const result = await loadData();
      setData(result);
      setIsDirty(false);
    } catch {
      notifyError('Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [loadData, notifyError]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const { handleRefresh } = usePageRefresh({ onRefresh: fetchData, isDirty });
  useUnsavedChanges(isDirty);

  const handleSave = useCallback(async () => {
    if (!saveData) return;
    try {
      setLoading(true);
      await saveData(data);
      notifySuccess('Saved successfully');
      await fetchData();
    } catch {
      notifyError('Failed to save data');
    } finally {
      setLoading(false);
    }
  }, [data, saveData, fetchData, notifySuccess, notifyError]);

  const handleDelete = useCallback(async () => {
    if (!deleteData || selectedIds.length === 0) return;
    try {
      await deleteData(selectedIds[0]!); 
      notifySuccess('Record deleted');
      setSelectedIds([]);
      await fetchData();
    } catch {
      notifyError('Failed to delete record');
    }
  }, [selectedIds, deleteData, fetchData, notifySuccess, notifyError]);
  
  const markDirty = useCallback(() => setIsDirty(true), []);
  const clearDirty = useCallback(() => setIsDirty(false), []);

  return {
    data,
    setData,
    loading,
    setLoading,
    selectedIds,
    setSelectedIds,
    selectedId: selectedIds.length > 0 ? selectedIds[0] : null,
    isDirty,
    markDirty,
    clearDirty,
    pageMode,
    fetchData,
    handleRefresh,
    handleSave,
    handleDelete,
    notifySuccess,
    notifyError
  };
}
