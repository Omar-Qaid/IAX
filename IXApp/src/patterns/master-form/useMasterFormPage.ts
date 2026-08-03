import { useCallback, useState } from 'react';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';

export type MasterFormMode = 'view' | 'edit' | 'create';
export function useMasterFormPage<T extends object>(initialRecord: T) {
  const [record, setRecord] = useState(initialRecord);
  const [draft, setDraft] = useState(initialRecord);
  const [mode, setMode] = useState<MasterFormMode>('view');
  const dirty = mode !== 'view' && JSON.stringify(record) !== JSON.stringify(draft);
  useUnsavedChanges(dirty);
  const edit = useCallback(() => { setDraft(record); setMode('edit'); }, [record]);
  const create = useCallback((next: T) => { setDraft(next); setMode('create'); }, []);
  const cancel = useCallback(() => { setDraft(record); setMode('view'); }, [record]);
  const commit = useCallback((saved: T = draft) => { setRecord(saved); setDraft(saved); setMode('view'); }, [draft]);
  return { record, setRecord, draft, setDraft, mode, isEditing: mode !== 'view', dirty, edit, create, cancel, commit };
}
