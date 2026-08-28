import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { AppDataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';
import i18n from '@core/localization/i18n';

interface TestRow {
  id: string;
  code: string;
  name: string;
  description: string;
}

const testColumns: ColumnDef<TestRow>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'name', headerName: 'Name', flex: 1 },
];

const testRows: TestRow[] = [
  { id: '1', code: 'CUST-001', name: 'Acme Corporation', description: 'First customer' },
  { id: '2', code: 'CUST-002', name: 'Global Tech Ltd', description: 'Second customer' },
];

describe('AppDataGrid', () => {
  it('renders data grid columns and rows', () => {
    render(
      <AppProviders>
        <AppDataGrid<TestRow> columns={testColumns} rows={testRows} getRowId={(row) => row.id} />
      </AppProviders>
    );

    expect(screen.getByText('CUST-001')).toBeDefined();
    expect(screen.getByText('Acme Corporation')).toBeDefined();
    expect(screen.getByText('CUST-002')).toBeDefined();
    const grid = screen.getByRole('grid');
    expect(grid.getAttribute('aria-rowcount')).toBe('2');
    expect(grid.getAttribute('aria-colcount')).toBe('2');
    expect(grid.getAttribute('aria-busy')).toBe('false');
  });

  it('opens a column filter without rendering MenuItems outside a menu context', () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined);

    render(
      <AppProviders>
        <AppDataGrid<TestRow> columns={testColumns} rows={testRows} getRowId={(row) => row.id} />
      </AppProviders>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Open filter for Code' }));

    expect(screen.getByText('Sort ascending')).toBeDefined();
    expect(screen.getByText('Sort descending')).toBeDefined();
    expect(
      consoleError.mock.calls.some((call) => String(call[0]).includes('MenuListContext is missing'))
    ).toBe(false);
    consoleError.mockRestore();
  });

  it('renders pinned and flexible columns in RTL without crashing', async () => {
    await act(() => i18n.changeLanguage('ar'));
    let unmount = () => {};
    try {
      ({ unmount } = render(
        <AppProviders>
          <AppDataGrid<TestRow>
            columns={[
              { field: 'code', headerName: 'Code', width: 120, pinned: 'left' },
              { field: 'name', headerName: 'Name', minWidth: 160, flex: 1 },
              { field: 'description', headerName: 'Description', width: 160, pinned: 'right' },
            ]}
            rows={testRows}
            getRowId={(row) => row.id}
            storageKey="test.rtl-grid"
          />
        </AppProviders>
      ));

      expect(screen.getByText('CUST-001')).toBeDefined();
      expect(
        screen.getByRole('grid').closest('[dir="rtl"]') ?? document.documentElement.dir
      ).toBeTruthy();

      const resizeHandle = document.querySelector<HTMLElement>('[data-grid-resize-handle="code"]');
      expect(resizeHandle).not.toBeNull();
      expect(getComputedStyle(resizeHandle!.parentElement!).width).toBe('120px');
      fireEvent.mouseDown(resizeHandle!, { clientX: 120 });
      fireEvent.mouseMove(window, { clientX: 90 });
      fireEvent.mouseUp(window);
      await waitFor(() => {
        expect(getComputedStyle(resizeHandle!.parentElement!).width).toBe('150px');
      });

      const endResizeHandle = document.querySelector<HTMLElement>(
        '[data-grid-resize-handle="description"]'
      );
      expect(endResizeHandle).not.toBeNull();
      expect(getComputedStyle(endResizeHandle!.parentElement!).width).toBe('160px');
      fireEvent.mouseDown(endResizeHandle!, { clientX: 400 });
      fireEvent.mouseMove(window, { clientX: 430 });
      fireEvent.mouseUp(window);
      await waitFor(() => {
        expect(getComputedStyle(endResizeHandle!.parentElement!).width).toBe('190px');
      });

      fireEvent.click(screen.getByRole('button', { name: 'فتح قائمة العمود Name' }));
      fireEvent.click(screen.getByRole('menuitem', { name: 'تثبيت العمود' }));
      expect(screen.getByRole('menuitem', { name: 'تثبيت كأول عمود' })).toBeDefined();
      expect(screen.getByRole('menuitem', { name: 'تثبيت كآخر عمود' })).toBeDefined();
    } finally {
      unmount();
      await act(() => i18n.changeLanguage('en'));
    }
  });
});
