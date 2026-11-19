import React from 'react';
import { act, fireEvent, render, renderHook, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { useListDetailsPage } from '@patterns/list-details/useListDetailsPage';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import { PageEditSessionContext } from '@shared/components/data-grid/PageEditSession';

type Record = { id: string; name: string };
const records: Record[] = [{ id: '1', name: 'Original' }];
const config: EnterpriseListDetailsConfig<Record> = {
  dataSource: { type: 'static', records },
  initialMode: 'edit',
  createRecord: () => ({ id: '', name: '' }),
  getPrimaryText: (row) => row.name,
  getValues: (row) => ({ name: row.name }),
  setValues: (row, values) => ({ ...row, name: String(values.name) }),
  headerFields: [],
  sections: [],
};

function Page({ initiallyEditing = true }: { initiallyEditing?: boolean }) {
  const state = useListDetailsPage({
    ...config,
    initialMode: initiallyEditing ? 'edit' : undefined,
  });
  return (
    <PageEditSessionContext.Provider
      value={{
        editing: state.editing,
        saving: state.saving,
        requestEdit: state.startEdit,
        registerEditor: state.registerEditor,
      }}
    >
      <span>{state.editing ? 'Page edit' : 'Page view'}</span>
      <button onClick={() => void state.save()}>Save page</button>
      <button onClick={state.cancel}>Cancel page</button>
      <TabularDetailPanel
        rows={[state.draft!]}
        columns={[{ field: 'name', headerName: 'Name', editable: true }]}
        masterForm
        addLabel="Add"
        removeLabel="Remove"
        selectedIds={[]}
        onSelectionChange={() => undefined}
        onRowSave={(row) => {
          if (!row.name?.trim()) throw new Error('Name required');
          state.changeRecord({ ...state.draft!, ...row });
        }}
      />
    </PageEditSessionContext.Provider>
  );
}

describe('list-details nested grid lifecycle', () => {
  it('requires page Edit mode before a line becomes editable', async () => {
    render(
      <AppProviders>
        <Page initiallyEditing={false} />
      </AppProviders>
    );
    fireEvent.click(screen.getByRole('gridcell'));
    expect(screen.getByText('Page view')).toBeInTheDocument();
    expect(screen.queryByDisplayValue('Original')).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Edit Name' }));
    expect(await screen.findByDisplayValue('Original')).toBeEnabled();
    expect(screen.getByText('Page edit')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Cancel page' }));
    await waitFor(() => expect(screen.queryByDisplayValue('Original')).toBeNull());
    expect(screen.getByText('Page view')).toBeInTheDocument();
  });

  it('blocks page Save on a line error and persists the latest line before leaving edit mode', async () => {
    render(
      <AppProviders>
        <Page />
      </AppProviders>
    );
    fireEvent.click(screen.getByRole('button', { name: 'Edit Name' }));
    const input = await screen.findByDisplayValue('Original');
    fireEvent.change(input, { target: { value: '' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save page' }));
    await screen.findByText('Name required');
    await waitFor(() => expect(input).toBeEnabled());
    expect(screen.getByText('Page edit')).toBeInTheDocument();
    fireEvent.change(input, { target: { value: 'Latest line' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save page' }));
    await screen.findByText('Page view');
    expect(screen.queryByRole('textbox')).toBeNull();
    expect(screen.getByText('Latest line')).toBeInTheDocument();
  });

  it('cancels all registered grid drafts with the page', () => {
    const { result } = renderHook(() => useListDetailsPage(config), { wrapper: AppProviders });
    const cancelEdit = vi.fn();
    act(() => {
      result.current.registerEditor({ saveEdit: vi.fn(), cancelEdit });
      result.current.changeRecord({ id: '1', name: 'Unsaved' });
    });
    act(() => result.current.cancel());
    expect(cancelEdit).toHaveBeenCalledOnce();
    expect(result.current.editing).toBe(false);
    expect(result.current.draft?.name).toBe('Original');
  });

  it('keeps page edit mode when a registered editor throws', async () => {
    const { result } = renderHook(() => useListDetailsPage(config), { wrapper: AppProviders });
    act(() => {
      result.current.registerEditor({
        saveEdit: async () => {
          throw new Error('Line failed');
        },
        cancelEdit: vi.fn(),
      });
    });
    await act(async () => {
      await result.current.save();
    });
    expect(result.current.error).toBe('Line failed');
    expect(result.current.editing).toBe(true);
    expect(result.current.saving).toBe(false);
  });
});
