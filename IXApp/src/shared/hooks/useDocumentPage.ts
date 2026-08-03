import { useState, useCallback } from 'react';
import { useNotifications } from './useNotifications';
import { useUnsavedChanges } from './useUnsavedChanges';

interface UseDocumentPageOptions<T> {
  loadData: (id: string) => Promise<T>;
  saveData?: (id: string, data: Partial<T>) => Promise<T>;
}

export function useDocumentPage<T>(documentId: string | undefined, options: UseDocumentPageOptions<T>) {
  const { loadData } = options;
  const [document, setDocument] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [isDirty, setIsDirty] = useState<boolean>(false);
  
  const { notifyError, notifySuccess } = useNotifications();
  useUnsavedChanges(isDirty);

  const fetchDocument = useCallback(async () => {
    if (!documentId) {
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      const data = await loadData(documentId);
      setDocument(data);
      setIsDirty(false);
    } catch {
      notifyError('Failed to load document details');
    } finally {
      setLoading(false);
    }
  }, [documentId, loadData, notifyError]);

  const executeProcessAction = async (
    action: (id: string) => Promise<T>,
    successMessage: string,
    errorMessage: string
  ) => {
    if (!documentId) return;
    try {
      setLoading(true);
      const updated = await action(documentId);
      setDocument(updated);
      setIsDirty(false);
      notifySuccess(successMessage);
    } catch {
      notifyError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const markDirty = useCallback(() => setIsDirty(true), []);
  const resetDirty = useCallback(() => setIsDirty(false), []);

  return {
    document,
    setDocument,
    loading,
    setLoading,
    isDirty,
    markDirty,
    resetDirty,
    fetchDocument,
    executeProcessAction,
    notifySuccess,
    notifyError
  };
}
