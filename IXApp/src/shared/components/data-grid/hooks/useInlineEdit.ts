import { useState, useCallback } from 'react';

export const NEW_ROW_ID = '__new_row__';

interface InlineEditState<T> {
  editingRowId: string | number | typeof NEW_ROW_ID | null;
  editValues: Partial<T>;
  /** Values when editing started, so an untouched line is not written back (D365 only saves modified records). */
  originalValues: Partial<T>;
  saving: boolean;
}

const EMPTY_STATE = { editingRowId: null, editValues: {}, originalValues: {}, saving: false };

function isSameValue(a: unknown, b: unknown): boolean {
  if (Object.is(a, b)) return true;
  // Editors hand back strings for numbers/dates; treat '' and null/undefined as the same blank.
  const blank = (v: unknown) => v == null || v === '';
  if (blank(a) && blank(b)) return true;
  if (a == null || b == null) return false;
  if (typeof a === 'object' || typeof b === 'object')
    return JSON.stringify(a) === JSON.stringify(b);
  return String(a) === String(b);
}

export function useInlineEdit<T>() {
  const [state, setState] = useState<InlineEditState<T>>(EMPTY_STATE);

  const startEdit = useCallback((rowId: string | number, row: T) => {
    setState({
      editingRowId: rowId,
      editValues: { ...row },
      originalValues: { ...row },
      saving: false,
    });
  }, []);

  const startAdd = useCallback((defaults: Partial<T> = {}) => {
    setState({ editingRowId: NEW_ROW_ID, editValues: defaults, originalValues: {}, saving: false });
  }, []);

  const updateField = useCallback((field: string, value: unknown) => {
    setState((prev) => ({ ...prev, editValues: { ...prev.editValues, [field]: value } }));
  }, []);

  const setSaving = useCallback((saving: boolean) => {
    setState((prev) => ({ ...prev, saving }));
  }, []);

  const cancelEdit = useCallback(() => {
    setState(EMPTY_STATE);
  }, []);

  const values = state.editValues as Record<string, unknown>;
  const original = state.originalValues as Record<string, unknown>;
  const isDirty =
    state.editingRowId === NEW_ROW_ID ||
    Object.keys(values).some((key) => !isSameValue(values[key], original[key]));

  return {
    editingRowId: state.editingRowId,
    editValues: state.editValues,
    isDirty,
    saving: state.saving,
    startEdit,
    startAdd,
    updateField,
    setSaving,
    cancelEdit,
  };
}
