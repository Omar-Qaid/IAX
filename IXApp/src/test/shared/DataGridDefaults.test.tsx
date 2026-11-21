import React from 'react';
import { render, renderHook } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { expect, it } from 'vitest';
import { useDataGridState } from '@shared/components/data-grid/hooks/useDataGridState';
import { uiDensity } from '@shared/constants/uiDensity';
const base = { initialState: { columns: [], filters: [], sortModel: [] }, initialColumns: [], rowHeight: uiDensity.gridRowHeight };
it('hides column filter inputs by default and supports explicitly showing them', () => {
  const props = { rows: [{ id: '1', name: 'Customer' }], columns: [{ field: 'name', headerName: 'Name' }], height: 200, hideToolbar: true };
  const { container, rerender } = render(<AppProviders><DataGrid {...props} /></AppProviders>);
  expect(container.querySelector('input[data-grid-filter-field="name"]')).toBeNull();
  rerender(<AppProviders><DataGrid {...props} hideFilterRow={false} /></AppProviders>);
  expect(container.querySelector('input[data-grid-filter-field="name"]')).not.toBeNull();
});
it('uses the Customers reference dimensions and enables both borders by default', () => {
  const { result } = renderHook(() => useDataGridState(base));
  expect(result.current.showColumnBorders).toBe(true);
  expect(result.current.showCellBorders).toBe(true);
  expect(result.current.localRowHeight).toBe(31);
  expect(uiDensity.gridHeaderHeight).toBe(30);
  expect(uiDensity.gridFontSize).toBe(11);
});
it('preserves explicit and saved border preferences', () => {
  const explicit = renderHook(() => useDataGridState({ ...base, initialShowColumnBorders: false, initialShowCellBorders: false }));
  expect(explicit.result.current.showColumnBorders).toBe(false);
  expect(explicit.result.current.showCellBorders).toBe(false);
  const saved = renderHook(() => useDataGridState({ ...base, initialState: { ...base.initialState, showColumnBorders: false, showCellBorders: false } }));
  expect(saved.result.current.showColumnBorders).toBe(false);
  expect(saved.result.current.showCellBorders).toBe(false);
});
