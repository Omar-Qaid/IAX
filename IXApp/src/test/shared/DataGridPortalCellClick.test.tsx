import React from 'react';
import { Portal } from '@mui/material';
import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

interface Row {
  id: string;
  itemNumber: string;
}

const rows: Row[] = [{ id: '1', itemNumber: 'ITM-1001' }];

function CurrentPath(): React.ReactElement {
  return <output data-testid="path">{useLocation().pathname}</output>;
}

/**
 * A cell that both links to the record and owns a popup, the way a grid's lookup
 * cell does: the popup renders in a portal, so its clicks travel up the React tree
 * through the cell.
 */
function Harness(): React.ReactElement {
  const columns: ColumnDef<Row>[] = [
    {
      field: 'itemNumber',
      headerName: 'Item number',
      getDetailHref: () => '/inventory/products/ITM-1001',
      renderCell: ({ row }) => (
        <>
          {row.itemNumber}
          <button type="button" onClick={(event) => event.stopPropagation()}>
            Edit item
          </button>
          <Portal>
            {/* The popup does not stop the click itself; the cell link must hold the line. */}
            <button type="button">Pick ITM-1001</button>
          </Portal>
        </>
      ),
    },
  ];
  return (
    <MemoryRouter initialEntries={['/accounts-receivable/sales-orders/1']}>
      <CurrentPath />
      <Routes>
        <Route
          path="*"
          element={<DataGrid<Row> rows={rows} columns={columns} getRowId={(row) => row.id} />}
        />
      </Routes>
    </MemoryRouter>
  );
}

describe('a grid cell that owns a portal', () => {
  it('does not navigate from an edit button that stops bubbling', () => {
    render(<Harness />);
    fireEvent.click(screen.getByRole('button', { name: 'Edit item' }));
    expect(screen.getByTestId('path').textContent).toBe('/accounts-receivable/sales-orders/1');
  });

  it('only navigates when a pointer click hits the displayed text', () => {
    const range = document.createRange();
    range.getClientRects = () =>
      [{ left: 10, right: 80, top: 10, bottom: 30 }] as unknown as DOMRectList;
    const createRange = vi.spyOn(document, 'createRange').mockReturnValue(range);
    try {
      render(<Harness />);
      const link = screen.getByRole('link', { name: /ITM-1001/ });
      fireEvent.click(link, { detail: 1, clientX: 150, clientY: 20 });
      expect(screen.getByTestId('path').textContent).toBe('/accounts-receivable/sales-orders/1');
      fireEvent.click(link, { detail: 1, clientX: 20, clientY: 20 });
      expect(screen.getByTestId('path').textContent).toBe('/inventory/products/ITM-1001');
    } finally {
      createRange.mockRestore();
    }
  });

  it('does not open the record when the click happened in the cell’s popup', () => {
    render(<Harness />);
    expect(screen.getByTestId('path').textContent).toBe('/accounts-receivable/sales-orders/1');

    fireEvent.click(screen.getByRole('button', { name: 'Pick ITM-1001' }));

    expect(screen.getByTestId('path').textContent).toBe('/accounts-receivable/sales-orders/1');
  });

  it('still opens the record when the link itself is clicked', () => {
    render(<Harness />);

    fireEvent.click(screen.getByRole('link', { name: /ITM-1001/ }));

    expect(screen.getByTestId('path').textContent).toBe('/inventory/products/ITM-1001');
  });
});
