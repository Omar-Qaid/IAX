import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { AppDataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

interface Line {
  id: string;
  code: string;
  description: string;
}

const columns: ColumnDef<Line>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'description', headerName: 'Description', width: 220, editable: true },
];
const rows: Line[] = [{ id: '1', code: 'L-1', description: 'Consulting' }];

/** Page owning edit mode, like a document page: the pencil switches it on. */
function Page({ onRequest }: { onRequest: (row: Line, field: string) => void }) {
  const [editing, setEditing] = React.useState(false);
  return (
    <AppDataGrid<Line>
      columns={columns}
      rows={rows}
      height={200}
      hideToolbar
      masterForm={editing}
      onCellEditRequest={
        editing
          ? undefined
          : (row, field) => {
              onRequest(row, field);
              setEditing(true);
            }
      }
    />
  );
}

describe('DataGrid edit from view (pencil on a cell)', () => {
  it('shows a pencil on editable view cells only, and edits the clicked cell after edit mode starts', async () => {
    const onRequest = vi.fn();
    const { container } = render(
      <AppProviders>
        <Page onRequest={onRequest} />
      </AppProviders>
    );
    const row = container.querySelector<HTMLElement>('[data-row-id="1"]')!;
    const codeCell = row.querySelector<HTMLElement>('[data-field="code"]')!;
    const descriptionCell = row.querySelector<HTMLElement>('[data-field="description"]')!;
    expect(within(codeCell).queryByRole('button', { name: /^Edit/ })).toBeNull();

    fireEvent.click(within(descriptionCell).getByRole('button', { name: 'Edit Description' }));

    expect(onRequest).toHaveBeenCalledWith(rows[0], 'description');
    await waitFor(() =>
      expect(document.activeElement).toBe(
        container.querySelector('[data-row-id="1"] [data-field="description"] input')
      )
    );
    expect(screen.queryByRole('button', { name: 'Edit Description' })).toBeNull();
  });
});

describe('DataGrid edit from view in always-editable grids', () => {
  it('edits the clicked cell directly when the grid is masterForm', async () => {
    const { container } = render(
      <AppProviders>
        <AppDataGrid<Line> columns={columns} rows={rows} height={200} hideToolbar masterForm />
      </AppProviders>
    );
    const cell = container.querySelector<HTMLElement>(
      '[data-row-id="1"] [data-field="description"]'
    )!;
    fireEvent.click(within(cell).getByRole('button', { name: 'Edit Description' }));
    await waitFor(() =>
      expect(document.activeElement).toBe(
        container.querySelector('[data-row-id="1"] [data-field="description"] input')
      )
    );
  });

  it('shows no pencil in read-only grids', () => {
    render(
      <AppProviders>
        <AppDataGrid<Line> columns={columns} rows={rows} height={200} hideToolbar />
      </AppProviders>
    );
    expect(screen.queryByRole('button', { name: 'Edit Description' })).toBeNull();
  });
});
