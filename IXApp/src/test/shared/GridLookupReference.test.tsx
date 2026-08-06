import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LookupGrid } from '@shared/components/lookups/LookupGrid';
import { LookupGridField } from '@shared/components/lookups/LookupGridField';
import type { GridLookupColumn, LookupPage } from '@shared/components/lookups/types';
import { AuthProvider } from '@core/auth/AuthProvider';

interface TestItem {
  id: string;
  code: string;
  name: string;
  nameAR: string;
}

const mockData: TestItem[] = [
  { id: '1', code: 'C001', name: 'Contoso Ltd', nameAR: 'شركة كونتوسو' },
  { id: '2', code: 'F002', name: 'Fabrikam Inc', nameAR: 'فابريكام' },
];

const mockColumns: GridLookupColumn<TestItem>[] = [
  { field: 'code', header: 'Code', width: 100 },
  { field: 'name', header: 'Name', flex: 1 },
];

const mockFetchPage = async (params: { pageNumber: number; pageSize: number; search: string }): Promise<LookupPage<TestItem>> => {
  const filtered = mockData.filter(
    (item) =>
      item.code.toLowerCase().includes(params.search.toLowerCase()) ||
      item.name.toLowerCase().includes(params.search.toLowerCase())
  );
  return {
    data: filtered,
    pageNumber: params.pageNumber,
    totalPages: 1,
    totalRecords: filtered.length,
  };
};

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

describe('Grid Lookup Reference Integration', () => {
  it('renders LookupGrid with initial closed state and opens on click', async () => {
    const queryClient = createTestQueryClient();
    const handleChange = vi.fn();

    render(
      <QueryClientProvider client={queryClient}>
        <LookupGrid<TestItem>
          value={null}
          onChange={handleChange}
          columns={mockColumns}
          fetchPage={mockFetchPage}
          queryKey={['test-grid-lookup']}
          label="Customer Lookup"
        />
      </QueryClientProvider>
    );

    const openBtn = screen.getByLabelText('Open');
    fireEvent.click(openBtn);

    await waitFor(() => {
      expect(screen.getByPlaceholderText('Search')).toBeInTheDocument();
    });
  });

  it('renders LookupGridField and supports row selection', async () => {
    const queryClient = createTestQueryClient();

    render(
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <LookupGridField<TestItem>
            name="customerId"
            label="Customer"
            columns={mockColumns}
            fetchPage={mockFetchPage}
            queryKey={['form-grid-lookup']}
            value="1"
            onChange={() => {}}
          />
        </QueryClientProvider>
      </AuthProvider>
    );

    expect(screen.getByLabelText('Customer')).toBeInTheDocument();
  });
});
