import React from 'react';
import { act, fireEvent, render, renderHook, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { expect, it, vi } from 'vitest';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { useGridDataProcessing } from '@shared/components/data-grid/hooks/useGridDataProcessing';
import { useGridPersistence } from '@shared/components/data-grid/hooks/useGridPersistence';
import { useDataGridState } from '@shared/components/data-grid/hooks/useDataGridState';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';

type Row = { id: string; amount: number; active: boolean };
const columns: ColumnDef<Row>[] = [
  { field: 'id', headerName: 'ID' },
  { field: 'amount', headerName: 'Amount', type: 'number' },
  { field: 'active', headerName: 'Active', type: 'boolean' },
];

it('opens the linked record from its key cell without triggering the row action', async () => {
  const onRowClick = vi.fn();
  const linkedColumns: ColumnDef<Row>[] = [
    { field: 'id', headerName: 'Product number',
      getDetailHref: (row) => row.active ? `/products/${row.id}` : undefined },
    columns[1],
  ];
  render(
    <MemoryRouter initialEntries={['/products']}>
      <Routes>
        <Route path="/products" element={<DataGrid columns={linkedColumns} rows={[
          { id: 'ITEM-001', amount: 1, active: true },
          { id: 'ITEM-002', amount: 2, active: false },
        ]} onRowClick={onRowClick} />} />
        <Route path="/products/ITEM-001" element={<h1>Edit ITEM-001</h1>} />
      </Routes>
    </MemoryRouter>
  );
  expect(await screen.findByText('ITEM-002')).toBeInTheDocument();
  expect(screen.queryByRole('link', { name: 'ITEM-002' })).toBeNull();
  const link = screen.getByRole('link', { name: 'ITEM-001' });
  expect(link).toHaveAttribute('href', '/products/ITEM-001');
  fireEvent.click(link);
  expect(await screen.findByRole('heading', { name: 'Edit ITEM-001' })).toBeInTheDocument();
  expect(onRowClick).not.toHaveBeenCalled();
});

it('preserves reordered columns and reads storage only once as editor callbacks change', () => {
  const getItem = vi.spyOn(Storage.prototype, 'getItem');
  const committed = vi.fn();
  const { result, rerender } = renderHook(
    ({ input }) => {
      React.useLayoutEffect(() => {
        committed();
      });
      const { initialState } = useGridPersistence('regression.columns', input);
      return useDataGridState({ initialState, initialColumns: input, rowHeight: 33 });
    },
    { initialProps: { input: columns } }
  );
  act(() =>
    result.current.setColumns([
      columns[1],
      { ...columns[0], width: 230, pinned: 'left' },
      columns[2],
    ])
  );
  committed.mockClear();
  rerender({ input: columns.map((column) => ({ ...column, renderCell: () => 'updated' })) });
  expect(committed).toHaveBeenCalledTimes(1);
  expect(result.current.columns.map((column) => column.field)).toEqual(['amount', 'id', 'active']);
  expect(result.current.columns[1]).toMatchObject({ width: 230, pinned: 'left' });
  expect(
    result.current.columns[0].renderCell?.({
      row: { id: 'a', amount: 1, active: true },
      value: 1,
      rowIndex: 0,
    })
  ).toBe('updated');
  expect(getItem.mock.calls.filter(([key]) => key === 'regression.columns')).toHaveLength(1);
  getItem.mockRestore();
});

it('filters zero and false values instead of treating them as empty filters', () => {
  const rows = [
    { id: 'a', amount: 0, active: false },
    { id: 'b', amount: 20, active: true },
  ];
  const { result } = renderHook(() =>
    useGridDataProcessing({
      rows,
      columns,
      globalSearch: '',
      sortModel: [],
      serverSide: false,
      filters: [
        { field: 'amount', operator: 'equals', value: 0 },
        { field: 'active', operator: 'equals', value: false },
      ],
    })
  );
  expect(result.current.map((row) => row.id)).toEqual(['a']);
  expect(rows).toHaveLength(2);
});

it('bounds initial DOM work for a large grid before the viewport is measured', () => {
  const rows = Array.from({ length: 10000 }, (_, i) => ({
    id: String(i),
    amount: i,
    active: true,
  }));
  render(
    <DataGrid rows={rows} columns={columns.slice(0, 2)} height={232} hideToolbar hideFilterRow />
  );
  expect(screen.getByRole('grid')).toHaveAttribute('aria-rowcount', '10000');
  expect(screen.getAllByRole('gridcell').length).toBeLessThanOrEqual(60);
});

it('reuses processed rows when only cell rendering and layout change', () => {
  const getter = vi.fn(({ row }: { row: Row }) => row.amount);
  const input = columns.map((column) =>
    column.field === 'amount' ? { ...column, valueGetter: getter } : column
  );
  const rows = [
    { id: 'a', amount: 10, active: true },
    { id: 'b', amount: 2, active: false },
  ];
  const sortModel = [{ field: 'amount', sort: 'asc' as const }];
  const filters: [] = [];
  const { result, rerender } = renderHook(
    ({ definitions }) =>
      useGridDataProcessing({
        rows,
        columns: definitions,
        sortModel,
        filters,
        serverSide: false,
        globalSearch: '',
      }),
    { initialProps: { definitions: input } }
  );
  expect(result.current.map((row) => row.id)).toEqual(['b', 'a']);
  const processed = result.current;
  getter.mockClear();
  rerender({
    definitions: input.map((column) => ({ ...column, width: 240, renderCell: () => 'editing' })),
  });
  expect(result.current).toBe(processed);
  expect(getter).not.toHaveBeenCalled();
});

it('focuses visible cells immediately without selecting the same row again', () => {
  const ref = React.createRef<DataGridHandle>();
  const onSelectionChange = vi.fn();
  render(
    <DataGrid
      ref={ref}
      rows={[{ id: 'a', amount: 4, active: true }]}
      columns={columns}
      selectedIds={['a']}
      onSelectionChange={onSelectionChange}
      height={232}
      hideToolbar
      hideFilterRow
    />
  );
  act(() => {
    void ref.current?.focusCell(0, 1);
  });
  expect(document.activeElement).toHaveAttribute('data-field', 'amount');
  act(() => {
    void ref.current?.focusCell(0, 0);
  });
  expect(document.activeElement).toHaveAttribute('data-field', 'id');
  expect(onSelectionChange).not.toHaveBeenCalled();
});

it('waits for a saved record to render before focusing its permanent ID', async () => {
  const ref = React.createRef<DataGridHandle>();
  const initial = [{ id: 'temporary', amount: 4, active: true }];
  const { rerender } = render(
    <DataGrid
      ref={ref}
      rows={initial}
      columns={columns.slice(0, 2)}
      height={232}
      hideToolbar
      hideFilterRow
    />
  );
  let navigation: Promise<void> | undefined;
  act(() => {
    navigation = ref.current?.focusRecordCell({ rowId: 'confirmed', field: 'amount' });
  });
  rerender(
    <DataGrid
      ref={ref}
      rows={[{ ...initial[0], id: 'confirmed' }]}
      columns={columns.slice(0, 2)}
      height={232}
      hideToolbar
      hideFilterRow
    />
  );
  await waitFor(() => {
    expect(document.activeElement).toHaveAttribute('data-field', 'amount');
    expect(document.activeElement?.parentElement).toHaveAttribute('data-row-id', 'confirmed');
  });
  await navigation;
});
