import React from 'react';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { MasterDetailLinesGrid } from '@patterns/master-detail/MasterDetailLinesGrid';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';

type Line = { id: string; description: string };
const rows: Line[] = [{ id: '1', description: 'Consulting' }];
const columns: ColumnDef<Line>[] = [
  { field: 'description', headerName: 'Description', editable: true },
];

describe('master-detail page and grid edit mode', () => {
  it('keeps a failed row draft through busy state and discards it on page cancellation', async () => {
    const ref = React.createRef<DataGridHandle>();
    const onRowSave = vi.fn(async () => {
      throw new Error('Description required');
    });
    const page = (editing: boolean, busy = false) => (
      <AppProviders>
        <span>{editing ? 'Page editing' : 'Page view'}</span>
        <MasterDetailLinesGrid
          ref={ref}
          editing={editing}
          busy={busy}
          canEdit
          onEdit={vi.fn()}
          rows={rows}
          columns={columns}
          onRowSave={onRowSave}
        />
      </AppProviders>
    );
    const view = render(page(true));
    act(() => ref.current!.startEditRow('1'));
    const input = screen.getByDisplayValue('Consulting');
    fireEvent.change(input, { target: { value: 'Invalid draft' } });
    view.rerender(page(true, true));
    expect(screen.getByDisplayValue('Invalid draft')).toBeDisabled();
    let saved: boolean | undefined;
    await act(async () => {
      saved = await ref.current!.saveEdit();
    });
    expect(saved).toBe(false);
    view.rerender(page(true));
    expect(screen.getByText('Page editing')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Invalid draft')).toBeEnabled();
    view.rerender(page(false));
    await waitFor(() => expect(screen.queryByDisplayValue('Invalid draft')).toBeNull());
    expect(screen.getByText('Page view')).toBeInTheDocument();
    // A later edit must start with the original values, not the rejected draft.
    view.rerender(page(true));
    expect(screen.queryByDisplayValue('Invalid draft')).toBeNull();
    act(() => ref.current!.startEditRow('1'));
    expect(screen.getByDisplayValue('Consulting')).toBeInTheDocument();
  });

  it('does not allow the grid ref to start editing while the page is in view mode', () => {
    const ref = React.createRef<DataGridHandle>();
    render(
      <AppProviders>
        <MasterDetailLinesGrid
          ref={ref}
          editing={false}
          canEdit
          onEdit={vi.fn()}
          rows={rows}
          columns={columns}
        />
      </AppProviders>
    );
    act(() => ref.current!.startEditRow('1'));
    expect(screen.queryByRole('textbox')).toBeNull();
  });
});
