import React from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { SalesLineGridSurface } from '@modules/finance/accounts-receivable/pages/SalesLineGridSurface';
import type { DataGridHandle } from '@shared/components/data-grid/types';

function setup(commit = vi.fn().mockResolvedValue(true)) {
  const focusCell = vi.fn();
  const onAdd = vi.fn();
  const onRemove = vi.fn();
  const onFilter = vi.fn();
  const gridRef = { current: { focusCell } as unknown as DataGridHandle };
  render(
    <SalesLineGridSurface
      gridRef={gridRef}
      commit={commit}
      onAdd={onAdd}
      onRemove={onRemove}
      onFilter={onFilter}
    >
      <div role="grid" aria-rowcount={2}>
        {[0, 1].map((r) => (
          <div role="row" key={r}>
            {[0, 1].map((c) => (
              <div role="gridcell" data-row-index={r} data-col-index={c} key={c}>
                <input aria-label={`cell-${r}-${c}`} />
              </div>
            ))}
          </div>
        ))}
      </div>
    </SalesLineGridSurface>
  );
  return { focusCell, commit, onAdd, onRemove, onFilter };
}

it('waits for a save before Tab navigation and prevents duplicate navigation while saving', async () => {
  let finish!: (success: boolean) => void;
  const commit = vi.fn(
    () =>
      new Promise<boolean>((resolve) => {
        finish = resolve;
      })
  );
  const { focusCell } = setup(commit);
  fireEvent.keyDown(screen.getByLabelText('cell-0-0'), { key: 'Tab' });
  fireEvent.keyDown(screen.getByLabelText('cell-0-0'), { key: 'Tab' });
  expect(commit).toHaveBeenCalledTimes(1);
  expect(focusCell).not.toHaveBeenCalled();
  finish(true);
  await waitFor(() => expect(focusCell).toHaveBeenCalledWith(0, 1));
});

it('keeps the current cell on validation or save failure', async () => {
  const { focusCell, commit } = setup(vi.fn().mockResolvedValue(false));
  fireEvent.keyDown(screen.getByLabelText('cell-0-0'), { key: 'Enter' });
  await waitFor(() => expect(commit).toHaveBeenCalledOnce());
  expect(focusCell).not.toHaveBeenCalled();
});

it('supports Shift+Tab, Enter row wrapping, vertical arrows, and existing row actions', async () => {
  const { focusCell, onAdd, onRemove, onFilter } = setup();
  const input = screen.getByLabelText('cell-0-1');
  fireEvent.keyDown(input, { key: 'Tab', shiftKey: true });
  await waitFor(() => expect(focusCell).toHaveBeenLastCalledWith(0, 0));
  fireEvent.keyDown(input, { key: 'Enter' });
  await waitFor(() => expect(focusCell).toHaveBeenLastCalledWith(1, 0));
  fireEvent.keyDown(input, { key: 'ArrowDown' });
  await waitFor(() => expect(focusCell).toHaveBeenLastCalledWith(1, 1));
  fireEvent.keyDown(input, { key: 'Insert' });
  fireEvent.keyDown(input, { key: 'Delete', altKey: true });
  fireEvent.keyDown(input, { key: 'f', ctrlKey: true });
  await waitFor(() => expect(onAdd).toHaveBeenCalledOnce());
  expect(onRemove).toHaveBeenCalledOnce();
  expect(onFilter).toHaveBeenCalledOnce();
});

it('preserves lookup keys and lets Tab leave the last cell', () => {
  const { commit } = setup();
  const input = screen.getByLabelText('cell-0-0');
  input.setAttribute('role', 'combobox');
  input.setAttribute('aria-expanded', 'true');
  const nativeKey = vi.fn();
  input.addEventListener('keydown', nativeKey);
  expect(fireEvent.keyDown(input, { key: 'Enter' })).toBe(true);
  expect(nativeKey).toHaveBeenCalledOnce();
  expect(commit).not.toHaveBeenCalled();
  expect(fireEvent.keyDown(screen.getByLabelText('cell-1-1'), { key: 'Tab' })).toBe(true);
});

it('saves before moving with the mouse', async () => {
  const { focusCell } = setup();
  screen.getByLabelText('cell-0-0').focus();
  fireEvent.mouseDown(screen.getByLabelText('cell-1-1'));
  await waitFor(() => expect(focusCell).toHaveBeenCalledWith(1, 1));
});

it('blocks the subsequent mouse click when validation rejects a cell change', async () => {
  const { commit } = setup(vi.fn().mockResolvedValue(false));
  const input = screen.getByLabelText('cell-0-0');
  const next = screen.getByLabelText('cell-1-1');
  input.focus();
  fireEvent.mouseDown(next);
  expect(fireEvent.click(next)).toBe(false);
  await waitFor(() => expect(commit).toHaveBeenCalledOnce());
  expect(input).toHaveFocus();
});

it('copies the focused cell value while preserving native selected-text copy', () => {
  setup();
  const input = screen.getByLabelText('cell-0-0') as HTMLInputElement;
  input.value = '123.50';
  input.setSelectionRange(6, 6);
  const setData = vi.fn();
  fireEvent.copy(input, { clipboardData: { setData } });
  expect(setData).toHaveBeenCalledWith('text/plain', '123.50');
  setData.mockClear();
  input.setSelectionRange(0, 3);
  expect(fireEvent.copy(input, { clipboardData: { setData } })).toBe(true);
  expect(setData).not.toHaveBeenCalled();
});

it('does not suppress a later click if the previous mouse target unmounted before clicking', async () => {
  const { focusCell } = setup();
  const input = screen.getByLabelText('cell-0-0');
  const next = screen.getByLabelText('cell-1-1');
  input.focus();
  fireEvent.mouseDown(next);
  await waitFor(() => expect(focusCell).toHaveBeenCalledWith(1, 1));
  // Moving to an editor can replace the mousedown target, dropping its click.
  next.remove();
  fireEvent.mouseDown(input);
  expect(fireEvent.click(input)).toBe(true);
});
