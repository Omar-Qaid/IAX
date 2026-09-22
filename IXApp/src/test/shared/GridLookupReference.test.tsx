import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { afterEach, describe, it, expect, vi } from 'vitest';
import i18n from '@core/localization/i18n';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LookupGrid } from '@shared/components/lookups/LookupGrid';
import { LookupGridField } from '@shared/components/lookups/LookupGridField';
import type { GridLookupColumn, LookupPage } from '@shared/components/lookups/types';
import { AuthProvider } from '@core/auth/AuthProvider';
import { FormProvider, useForm } from 'react-hook-form';

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

const mockFetchPage = async (params: {
  pageNumber: number;
  pageSize: number;
  search: string;
}): Promise<LookupPage<TestItem>> => {
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

function ControlledLookupWithinForm({
  onChange,
}: {
  onChange: (value: string | null, row?: TestItem | null) => void;
}) {
  const form = useForm({ defaultValues: { customerId: 'form-value' } });
  return (
    <FormProvider {...form}>
      <LookupGridField<TestItem>
        name="customerId"
        label="Customer"
        columns={mockColumns}
        fetchPage={mockFetchPage}
        queryKey={['controlled-grid-lookup']}
        value="1"
        onChange={onChange}
      />
    </FormProvider>
  );
}

describe('Grid Lookup Reference Integration', () => {
  afterEach(async () => { await i18n.changeLanguage('en'); });

  it('uses nameAlias by default in RTL and changes the selected display when language changes', async () => {
    await i18n.changeLanguage('ar');
    const row = { id: '1', name: 'Manager', nameAlias: 'المدير' };
    render(<QueryClientProvider client={createTestQueryClient()}><AuthProvider>
      <LookupGridField name="role" label="Role" value="1" onChange={vi.fn()}
        columns={[{ field: 'name', header: 'Name' }]} queryKey={['localized-role']}
        fetchById={async () => row}
        fetchPage={async () => ({ data: [row], pageNumber: 1, totalPages: 1, totalRecords: 1 })} />
    </AuthProvider></QueryClientProvider>);
    await waitFor(() => expect(screen.getByLabelText('Role')).toHaveValue('المدير'));
    await i18n.changeLanguage('en');
    await waitFor(() => expect(screen.getByLabelText('Role')).toHaveValue('Manager'));
  });

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

  it('clears an externally controlled value inside an unrelated form context', () => {
    const queryClient = createTestQueryClient();
    const handleChange = vi.fn();

    render(
      <AuthProvider>
        <QueryClientProvider client={queryClient}>
          <ControlledLookupWithinForm onChange={handleChange} />
        </QueryClientProvider>
      </AuthProvider>
    );

    fireEvent.click(screen.getByRole('button', { name: 'Clear' }));

    expect(handleChange).toHaveBeenCalledWith(null, null);
  });
});
