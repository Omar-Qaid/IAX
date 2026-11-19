import React from 'react';
import { act, fireEvent, render, renderHook, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { generateCSV, toCsvField } from '@shared/components/data-grid/DataGridUtils';
import { useGridDataProcessing } from '@shared/components/data-grid/hooks/useGridDataProcessing';
import { defaultFilterOperator, filterOperatorsFor } from '@shared/components/data-grid/constants';
import type { ColumnDef, DataGridHandle, FilterModel } from '@shared/components/data-grid/types';

type Line = { id: string; code: string; qty: number; note?: string | null };

const lines: Line[] = [
  { id: 'a', code: 'A-1', qty: 10, note: 'first' },
  { id: 'b', code: 'B-2', qty: 100, note: null },
  { id: 'c', code: 'C-3', qty: 210, note: '' },
];

const editableColumns: ColumnDef<Line>[] = [
  { field: 'code', headerName: 'Code', editable: true },
  { field: 'qty', headerName: 'Qty', type: 'number', editable: true },
];

const process = (filters: FilterModel[], rows: Line[] = lines) =>
  renderHook(() =>
    useGridDataProcessing({
      rows,
      columns: [
        { field: 'code', headerName: 'Code' },
        { field: 'qty', headerName: 'Qty', type: 'number' },
        { field: 'note', headerName: 'Note' },
      ] as ColumnDef<Line>[],
      globalSearch: '',
      sortModel: [],
      serverSide: false,
      filters,
    })
  ).result.current.map((row) => row.id);

const renderEditable = (props: Partial<React.ComponentProps<typeof DataGrid<Line>>> = {}) => {
  const ref = React.createRef<DataGridHandle>();
  const onRowSave = vi.fn(async () => undefined);
  const utils = render(
    <AppProviders>
      <DataGrid<Line>
        ref={ref}
        rows={lines}
        columns={editableColumns}
        masterForm
        onRowSave={onRowSave}
        height={300}
        hideToolbar
        hideFilterRow
        {...props}
      />
    </AppProviders>
  );
  return { ref, onRowSave, ...utils };
};

const editingRowId = () =>
  document
    .querySelector('[data-row-id] input')
    ?.closest('[data-row-id]')
    ?.getAttribute('data-row-id');

describe('grid filters', () => {
  it('applies blank checks without a value', () => {
    expect(process([{ field: 'note', operator: 'isEmpty', value: '' }])).toEqual(['b', 'c']);
    expect(process([{ field: 'note', operator: 'isNotEmpty', value: '' }])).toEqual(['a']);
  });

  it('accepts "is one of" as an array or a comma-separated string', () => {
    expect(process([{ field: 'code', operator: 'in', value: ['a-1', 'C-3'] }])).toEqual(['a', 'c']);
    expect(process([{ field: 'code', operator: 'in', value: 'A-1, B-2' }])).toEqual(['a', 'b']);
  });

  it('keeps blank values for negative operators', () => {
    expect(process([{ field: 'note', operator: 'notEquals', value: 'first' }])).toEqual(['b', 'c']);
  });

  it('matches numbers exactly from the filter row and offers type-specific operators', () => {
    expect(defaultFilterOperator('number')).toBe('equals');
    expect(defaultFilterOperator('text')).toBe('contains');
    expect(
      process([{ field: 'qty', operator: defaultFilterOperator('number'), value: '10' }])
    ).toEqual(['a']);
    const numberOps = filterOperatorsFor('number').map((op) => op.value);
    expect(numberOps).toContain('gt');
    expect(numberOps).not.toContain('contains');
    expect(filterOperatorsFor('text').map((op) => op.value)).not.toContain('gt');
  });
});

describe('CSV export', () => {
  it('writes a UTF-8 BOM, CRLF rows and neutralises formulas', () => {
    const csv = generateCSV([{ id: '1', code: '=HYPERLINK("x")', qty: -5, note: 'عميل' }], [
      { field: 'code', headerName: 'Code' },
      { field: 'qty', headerName: 'Qty' },
      { field: 'note', headerName: 'Note' },
    ] as ColumnDef<Line>[]);
    expect(csv.startsWith('﻿')).toBe(true);
    expect(csv).toContain('\r\n');
    expect(csv).toContain(`"'=HYPERLINK(""x"")"`);
    expect(csv).toContain('"-5"');
    expect(csv).toContain('"عميل"');
    expect(toCsvField('@SUM(A1)')).toBe(`"'@SUM(A1)"`);
  });
});

describe('keyboard', () => {
  it('leaves F5 to the browser when the grid has no refresh action', () => {
    render(<DataGrid<Line> rows={lines} columns={editableColumns} height={300} hideToolbar />);
    const cell = document.querySelector<HTMLElement>('[data-row-index="0"][data-col-index="0"]')!;
    cell.focus();
    const event = new KeyboardEvent('keydown', { key: 'F5', bubbles: true, cancelable: true });
    window.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(false);
  });

  it('handles F5 when the grid has a refresh action', () => {
    const onRefresh = vi.fn();
    render(
      <DataGrid<Line>
        rows={lines}
        columns={editableColumns}
        height={300}
        hideToolbar
        onRefresh={onRefresh}
      />
    );
    document.querySelector<HTMLElement>('[data-row-index="0"][data-col-index="0"]')!.focus();
    const event = new KeyboardEvent('keydown', { key: 'F5', bubbles: true, cancelable: true });
    window.dispatchEvent(event);
    expect(event.defaultPrevented).toBe(true);
    expect(onRefresh).toHaveBeenCalledTimes(1);
  });

  it('navigates rows with arrows from the selection checkbox', async () => {
    const ref = React.createRef<DataGridHandle>();
    render(
      <DataGrid<Line>
        ref={ref}
        rows={lines}
        columns={editableColumns}
        checkboxSelection
        height={300}
        hideToolbar
        hideFilterRow
      />
    );
    await act(() => ref.current!.focusCell(0, 0));
    expect((document.activeElement as HTMLInputElement).type).toBe('checkbox');
    fireEvent.keyDown(document.activeElement!, { key: 'ArrowDown' });
    await waitFor(() =>
      expect(document.activeElement?.closest('[data-row-index]')).toHaveAttribute(
        'data-row-index',
        '1'
      )
    );
  });
});

describe('D365 line editing', () => {
  it('does not write a line that was opened but not changed', async () => {
    const { ref, onRowSave } = renderEditable();
    act(() => ref.current!.startEditRow('a'));
    expect(editingRowId()).toBe('a');
    let saved: boolean | undefined;
    await act(async () => {
      saved = await ref.current!.saveEdit();
    });
    expect(saved).toBe(true);
    expect(onRowSave).not.toHaveBeenCalled();
    expect(editingRowId()).toBeUndefined();
  });

  it('writes a changed line once', async () => {
    const { ref, onRowSave } = renderEditable();
    act(() => ref.current!.startEditRow('a'));
    const input = document.querySelector<HTMLInputElement>(
      '[data-row-id="a"] [data-field="code"] input'
    )!;
    fireEvent.change(input, { target: { value: 'A-9' } });
    await act(async () => {
      await ref.current!.saveEdit();
    });
    expect(onRowSave).toHaveBeenCalledTimes(1);
    expect(onRowSave).toHaveBeenCalledWith(
      expect.objectContaining({ id: 'a', code: 'A-9' }),
      false
    );
  });

  it('saves the line and keeps editing the next one on ArrowDown', async () => {
    const { ref, onRowSave } = renderEditable();
    act(() => ref.current!.startEditRow('a'));
    const input = document.querySelector<HTMLInputElement>(
      '[data-row-id="a"] [data-field="code"] input'
    )!;
    fireEvent.change(input, { target: { value: 'A-9' } });
    input.focus();
    fireEvent.keyDown(input, { key: 'ArrowDown' });
    await waitFor(() => expect(editingRowId()).toBe('b'));
    expect(onRowSave).toHaveBeenCalledTimes(1);
    await waitFor(() =>
      expect(document.activeElement?.closest('[data-row-id]')).toHaveAttribute('data-row-id', 'b')
    );
  });

  it('stays on the line when its save fails', async () => {
    const onRowSave = vi.fn(async () => {
      throw new Error('Quantity must be positive');
    });
    const { ref } = renderEditable({ onRowSave });
    act(() => ref.current!.startEditRow('a'));
    const input = document.querySelector<HTMLInputElement>(
      '[data-row-id="a"] [data-field="code"] input'
    )!;
    fireEvent.change(input, { target: { value: 'A-9' } });
    input.focus();
    fireEvent.keyDown(input, { key: 'ArrowDown' });
    await waitFor(() => expect(onRowSave).toHaveBeenCalledTimes(1));
    expect(editingRowId()).toBe('a');
  });

  it('moves to the next line with Tab from the last field', async () => {
    const { ref } = renderEditable();
    act(() => ref.current!.startEditRow('a'));
    const qty = document.querySelector<HTMLInputElement>(
      '[data-row-id="a"] [data-field="qty"] input'
    )!;
    qty.focus();
    fireEvent.keyDown(qty, { key: 'Tab' });
    await waitFor(() => expect(editingRowId()).toBe('b'));
  });

  it('deletes the selected record with Alt+F9', () => {
    const onDelete = vi.fn();
    render(
      <DataGrid<Line>
        rows={lines}
        columns={editableColumns}
        masterForm
        onDelete={onDelete}
        selectedIds={['b']}
        height={300}
        hideToolbar
        hideFilterRow
      />
    );
    screen.getAllByRole('gridcell')[0].focus();
    fireEvent.keyDown(document.activeElement!, { key: 'F9', altKey: true });
    expect(onDelete).toHaveBeenCalledWith(lines[1]);
  });
});
