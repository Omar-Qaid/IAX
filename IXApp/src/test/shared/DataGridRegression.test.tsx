import React from 'react';
import { act, render, renderHook, screen } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { useGridDataProcessing } from '@shared/components/data-grid/hooks/useGridDataProcessing';
import { useGridPersistence } from '@shared/components/data-grid/hooks/useGridPersistence';
import { useDataGridState } from '@shared/components/data-grid/hooks/useDataGridState';
import type { ColumnDef } from '@shared/components/data-grid/types';

type Row = { id: string; amount: number; active: boolean };
const columns: ColumnDef<Row>[] = [
  { field: 'id', headerName: 'ID' },
  { field: 'amount', headerName: 'Amount', type: 'number' },
  { field: 'active', headerName: 'Active', type: 'boolean' },
];

it('preserves reordered columns and reads storage only once as editor callbacks change', () => {
  const getItem = vi.spyOn(Storage.prototype, 'getItem');
  const { result, rerender } = renderHook(
    ({ input }) => {
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
  rerender({ input: columns.map((column) => ({ ...column, renderCell: () => 'updated' })) });
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
