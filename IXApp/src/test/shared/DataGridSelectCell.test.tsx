import React from 'react';
import { act, fireEvent, render, renderHook, screen, within } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { AppProviders } from '@app/providers/AppProviders';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { generateCSV, normalizeValueOptions } from '@shared/components/data-grid/DataGridUtils';
import { useGridDataProcessing } from '@shared/components/data-grid/hooks/useGridDataProcessing';
import type { ColumnDef, DataGridHandle, FilterModel } from '@shared/components/data-grid/types';

type Group = { id: string; groupId: string; module: number | null };

const moduleOptions = [
  { value: 0, label: 'Customer' },
  { value: 1, label: 'Vendor' },
];
const columns: ColumnDef<Group>[] = [
  { field: 'groupId', headerName: 'Group', editable: true },
  { field: 'module', headerName: 'Module', type: 'singleSelect', valueOptions: moduleOptions, editable: true },
];
const rows: Group[] = [
  { id: 'a', groupId: 'G1', module: 0 },
  { id: 'b', groupId: 'G2', module: 1 },
];

const renderGrid = (gridRows: Group[] = rows) => {
  const ref = React.createRef<DataGridHandle>();
  const onRowSave = vi.fn(async () => undefined);
  render(
    <AppProviders>
      <DataGrid<Group>
        ref={ref}
        rows={gridRows}
        columns={columns}
        masterForm
        onRowSave={onRowSave}
        height={300}
        hideToolbar
        hideFilterRow
      />
    </AppProviders>
  );
  return { ref, onRowSave };
};

const moduleSelect = (rowId: string) =>
  within(document.querySelector<HTMLElement>(`[data-row-id="${rowId}"] [data-field="module"]`)!).getByRole(
    'combobox'
  );

describe('dropdown cell editor', () => {
  it('saves the option value with its type, not as a string', async () => {
    const { ref, onRowSave } = renderGrid();
    act(() => ref.current!.startEditRow('a'));
    fireEvent.mouseDown(moduleSelect('a'));
    fireEvent.click(await screen.findByRole('option', { name: 'Vendor' }));
    await act(async () => {
      await ref.current!.saveEdit();
    });
    expect(onRowSave).toHaveBeenCalledWith(expect.objectContaining({ module: 1 }), false);
  });

  it('shows the label of the current value and lists every option', async () => {
    const { ref } = renderGrid();
    act(() => ref.current!.startEditRow('b'));
    expect(moduleSelect('b')).toHaveTextContent('Vendor');
    fireEvent.mouseDown(moduleSelect('b'));
    const listbox = await screen.findByRole('listbox');
    expect(within(listbox).getAllByRole('option').map((o) => o.textContent)).toEqual([
      'Customer',
      'Vendor',
    ]);
  });

  it('keeps a stored value that is not in the options visible instead of blank', () => {
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined);
    const { ref } = renderGrid([{ id: 'x', groupId: 'G9', module: 7 }]);
    act(() => ref.current!.startEditRow('x'));
    expect(moduleSelect('x')).toHaveTextContent('7');
    expect(
      consoleWarn.mock.calls.some((call) => String(call[0]).includes('out-of-range value'))
    ).toBe(false);
    consoleWarn.mockRestore();
  });

  it('opens the list on Enter without saving the line', async () => {
    const { ref, onRowSave } = renderGrid();
    act(() => ref.current!.startEditRow('a'));
    const select = moduleSelect('a');
    select.focus();
    fireEvent.keyDown(select, { key: 'Enter' });
    expect(await screen.findByRole('listbox')).toBeDefined();
    expect(onRowSave).not.toHaveBeenCalled();
    expect(document.querySelector('[data-row-id="a"] [data-field="module"] [role="combobox"]')).not.toBeNull();
  });

  it('does not write the line when the same option is picked again', async () => {
    const { ref, onRowSave } = renderGrid();
    act(() => ref.current!.startEditRow('b'));
    fireEvent.mouseDown(moduleSelect('b'));
    fireEvent.click(await screen.findByRole('option', { name: 'Vendor' }));
    await act(async () => {
      await ref.current!.saveEdit();
    });
    expect(onRowSave).not.toHaveBeenCalled();
  });
});

describe('dropdown columns in search, filters and export', () => {
  const process = (filters: FilterModel[], globalSearch = '') =>
    renderHook(() =>
      useGridDataProcessing({ rows, columns, globalSearch, filters, sortModel: [], serverSide: false })
    ).result.current.map((row) => row.id);

  it('matches the label users see as well as the stored value', () => {
    expect(process([{ field: 'module', operator: 'contains', value: 'vend' }])).toEqual(['b']);
    expect(process([{ field: 'module', operator: 'equals', value: '1' }])).toEqual(['b']);
    expect(process([], 'customer')).toEqual(['a']);
  });

  it('applies "not equal" to both value and label', () => {
    expect(process([{ field: 'module', operator: 'notEquals', value: 'Vendor' }])).toEqual(['a']);
  });

  it('exports the label instead of the stored code', () => {
    const csv = generateCSV(rows, columns);
    expect(csv).toContain('"G2","Vendor"');
    expect(csv).not.toContain('"G2","1"');
  });

  it('normalizes plain and object options and keeps value types', () => {
    expect(normalizeValueOptions(['A', { value: 2, label: 'Two' }, { value: null, label: 'None' }])).toEqual([
      { key: 'A', value: 'A', label: 'A', disabled: undefined },
      { key: '2', value: 2, label: 'Two', disabled: false },
      { key: '', value: null, label: 'None', disabled: false },
    ]);
  });
});
