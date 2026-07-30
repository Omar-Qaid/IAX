import { useState, useCallback } from 'react';

export const NEW_ROW_ID = '__new_row__';

interface InlineEditState<T> {
  editingRowId: string | number | typeof NEW_ROW_ID | null;
  editValues: Partial<T>;
  saving: boolean;
}

export function useInlineEdit<T>() {
  const [state, setState] = useState<InlineEditState<T>>({
    editingRowId: null,
    editValues: {},
    saving: false,
  });

  const startEdit = useCallback((rowId: string | number, row: T) => {
    setState({ editingRowId: rowId, editValues: { ...row }, saving: false });
  }, []);

  const startAdd = useCallback((defaults: Partial<T> = {}) => {
    setState({ editingRowId: NEW_ROW_ID, editValues: defaults, saving: false });
  }, []);

  const updateField = useCallback((field: string, value: unknown) => {
    setState(prev => ({ ...prev, editValues: { ...prev.editValues, [field]: value } }));
  }, []);

  const setSaving = useCallback((saving: boolean) => {
    setState(prev => ({ ...prev, saving }));
  }, []);

  const cancelEdit = useCallback(() => {
    setState({ editingRowId: null, editValues: {}, saving: false });
  }, []);

  return {
    editingRowId: state.editingRowId,
    editValues: state.editValues,
    saving: state.saving,
    startEdit,
    startAdd,
    updateField,
    setSaving,
    cancelEdit,
  };
}
