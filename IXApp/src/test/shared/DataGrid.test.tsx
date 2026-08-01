import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { AppDataGrid } from '@shared/components/data-grid/DataGrid';
import type { ColumnDef } from '@shared/components/data-grid/types';

interface TestRow {
  id: string;
  code: string;
  name: string;
}

const testColumns: ColumnDef<TestRow>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'name', headerName: 'Name', flex: 1 },
];

const testRows: TestRow[] = [
  { id: '1', code: 'CUST-001', name: 'Acme Corporation' },
  { id: '2', code: 'CUST-002', name: 'Global Tech Ltd' },
];

describe('AppDataGrid', () => {
  it('renders data grid columns and rows', () => {
    render(
      <AppProviders>
        <AppDataGrid<TestRow>
          columns={testColumns}
          rows={testRows}
          getRowId={(row) => row.id}
        />
      </AppProviders>
    );

    expect(screen.getByText('CUST-001')).toBeDefined();
    expect(screen.getByText('Acme Corporation')).toBeDefined();
    expect(screen.getByText('CUST-002')).toBeDefined();
  });
});
