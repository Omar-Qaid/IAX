import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it } from 'vitest';
import { useDisclosure } from '@shared/hooks/useDisclosure';
import { useLocalStorage } from '@shared/hooks/useLocalStorage';
import { useEntityForm } from '@shared/components/forms/useEntityForm';
import { useSimpleListPage } from '@patterns/simple-list/useSimpleListPage';

describe('shared state hooks', () => {
  beforeEach(() => window.localStorage.clear());

  it('controls disclosure state through a stable generic API', () => {
    const { result } = renderHook(() => useDisclosure());
    expect(result.current.open).toBe(false);
    act(() => result.current.onOpen());
    expect(result.current.open).toBe(true);
    act(() => result.current.onToggle());
    expect(result.current.open).toBe(false);
  });

  it('persists, updates, and removes typed local storage values', () => {
    const { result } = renderHook(() => useLocalStorage('test-settings', { density: 'compact' }));
    act(() => result.current[1]({ density: 'comfortable' }));
    expect(result.current[0]).toEqual({ density: 'comfortable' });
    expect(JSON.parse(window.localStorage.getItem('test-settings') ?? '{}')).toEqual({ density: 'comfortable' });
    act(() => result.current[2]());
    expect(result.current[0]).toEqual({ density: 'compact' });
    expect(window.localStorage.getItem('test-settings')).toBeNull();
  });

  it('manages generic entity form values, validation, and reset', async () => {
    const { result } = renderHook(() => useEntityForm({ initialValues: { name: '' }, validate: values => values.name ? {} : { name: 'Required' } }));
    await act(async () => { expect(await result.current.submit()).toBe(false); });
    expect(result.current.errors.name).toBe('Required');
    act(() => result.current.setFieldValue('name', 'Contoso'));
    expect(result.current.dirty).toBe(true);
    await act(async () => { expect(await result.current.submit()).toBe(true); });
    act(() => result.current.reset());
    expect(result.current.values.name).toBe('');
  });

  it('standardizes simple-list filtering and selection', () => {
    const rows = [{ id: 1, name: 'Contoso' }, { id: 2, name: 'Fabrikam' }];
    const { result } = renderHook(() => useSimpleListPage({ rows, getRowId: row => row.id, matchesSearch: (row, query) => row.name.toLowerCase().includes(query) }));
    act(() => result.current.setQuery('fab'));
    expect(result.current.filteredRows).toEqual([rows[1]]);
    act(() => result.current.setSelectedIds([2]));
    expect(result.current.selectedRows).toEqual([rows[1]]);
  });
});
