import { useState, useCallback } from 'react';

export type PageMode = 'view' | 'create' | 'edit' | 'copy' | 'readonly' | 'process';

export function usePageMode(initialMode: PageMode = 'view') {
  const [mode, setMode] = useState<PageMode>(initialMode);

  const setViewMode = useCallback(() => setMode('view'), []);
  const setCreateMode = useCallback(() => setMode('create'), []);
  const setEditMode = useCallback(() => setMode('edit'), []);
  const setCopyMode = useCallback(() => setMode('copy'), []);
  const setReadOnlyMode = useCallback(() => setMode('readonly'), []);
  const setProcessMode = useCallback(() => setMode('process'), []);

  return {
    mode,
    setMode,
    setViewMode,
    setCreateMode,
    setEditMode,
    setCopyMode,
    setReadOnlyMode,
    setProcessMode,
    isView: mode === 'view',
    isCreate: mode === 'create',
    isEdit: mode === 'edit',
    isCopy: mode === 'copy',
    isReadOnly: mode === 'readonly',
    isProcess: mode === 'process',
    isEditable: mode === 'create' || mode === 'edit',
  };
}
