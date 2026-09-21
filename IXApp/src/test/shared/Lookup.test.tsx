import React, { useState } from 'react';
import { afterEach, describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LookupField } from '@shared/components/lookups/LookupField';
import type { LookupOption } from '@shared/components/lookups/types';
import i18n from '@core/localization/i18n';

const sampleOptions: LookupOption[] = [
  { id: '1', code: 'US', name: 'United States' },
  { id: '2', code: 'CA', name: 'Canada' },
];

const renderWithQueryClient = (element: React.ReactElement) => {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(<QueryClientProvider client={queryClient}>{element}</QueryClientProvider>);
};

describe('LookupField', () => {
  afterEach(async () => {
    await i18n.changeLanguage('en');
  });

  it('renders input with label and opens lookup dialog on click', () => {
    render(
      <LookupField name="country" label="Country" options={sampleOptions} displayMode="dialog" />
    );

    const input = screen.getByLabelText(/Country/i);
    expect(input).toBeDefined();

    fireEvent.click(input);
    expect(screen.getByText('Select Country')).toBeDefined();
    expect(screen.getByText('US - United States')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Close' })).toBeDefined();
  });

  it('localizes the lookup dialog defaults in Arabic', async () => {
    await i18n.changeLanguage('ar');
    render(
      <LookupField name="country" label="الدولة" options={sampleOptions} displayMode="dialog" />
    );

    fireEvent.click(screen.getByLabelText('الدولة'));
    expect(screen.getByText('اختر الدولة')).toBeDefined();
    expect(screen.getByPlaceholderText('البحث في الخيارات…')).toBeDefined();
    expect(screen.getByRole('button', { name: 'إغلاق' })).toBeDefined();
  });

  it('supports a searchable dropdown in the default select mode', async () => {
    const onChange = vi.fn();
    renderWithQueryClient(
      <LookupField name="country" label="Country" options={sampleOptions} onChange={onChange} />
    );

    fireEvent.click(screen.getByRole('button', { name: 'Open' }));
    const searchField = screen.getByRole('combobox', { name: 'Country' });
    fireEvent.change(searchField, { target: { value: 'Canada' } });
    fireEvent.click(await screen.findByText('Canada'));

    expect(onChange).toHaveBeenCalledWith('2', sampleOptions[1]);
    expect(screen.queryByText('Select Country')).toBeNull();
  });

  it('commits a numeric selection and keeps its label visible in controlled mode', async () => {
    const numericOptions: LookupOption[] = [
      { id: 1, code: 'MGR', name: 'Manager' },
      { id: 2, code: 'SUP', name: 'Supervisor' },
    ];

    function ControlledLookup() {
      const [value, setValue] = useState<number>(0);
      return (
        <LookupField
          name="occupation"
          label="Occupation"
          value={value}
          options={numericOptions}
          onChange={(next) => setValue(Number(next) || 0)}
        />
      );
    }

    renderWithQueryClient(<ControlledLookup />);
    fireEvent.click(screen.getByRole('button', { name: 'Open' }));
    fireEvent.click(await screen.findByText('Manager'));

    expect(screen.getByLabelText('Occupation')).toHaveValue('Manager');
    expect(screen.queryByRole('listbox')).toBeNull();
  });

  it('opens the shared popup and shows loading state while options load', () => {
    render(
      <LookupField name="customer" label="Customer" options={[]} loading displayMode="dialog" />
    );

    fireEvent.click(screen.getByLabelText('Customer'));

    expect(screen.getByText('Select Customer')).toBeDefined();
    expect(screen.getByRole('progressbar')).toBeDefined();
  });

  it('hides search when searchable is false', () => {
    render(
      <LookupField
        name="country"
        label="Country"
        options={sampleOptions}
        searchable={false}
        displayMode="dialog"
      />
    );

    fireEvent.click(screen.getByLabelText('Country'));

    expect(screen.queryByPlaceholderText('Search options…')).toBeNull();
    expect(screen.getByText('US - United States')).toBeDefined();
  });

  it('filters supplied options locally without fetching', () => {
    const onFetchOptions = vi.fn();
    render(
      <LookupField
        name="country"
        label="Country"
        options={sampleOptions}
        searchable
        sideMode="client"
        displayMode="dialog"
        onFetchOptions={onFetchOptions}
      />
    );

    fireEvent.click(screen.getByLabelText('Country'));
    fireEvent.change(screen.getByPlaceholderText('Search options…'), {
      target: { value: 'Canada' },
    });

    expect(screen.queryByText('US - United States')).toBeNull();
    expect(screen.getByText('CA - Canada')).toBeDefined();
    expect(onFetchOptions).not.toHaveBeenCalled();
  });

  it('debounces server search and uses the backend result without local filtering', async () => {
    const fetchPage = vi.fn(async ({ search }: { search: string }) => ({
      data: search
        ? [{ id: '3', code: 'OMR', name: `Server result for ${search}` }]
        : sampleOptions,
      pageNumber: 1,
      totalPages: 1,
      totalRecords: search ? 1 : 2,
    }));

    renderWithQueryClient(
      <LookupField
        name="customer"
        label="Customer"
        searchable
        sideMode="server"
        displayMode="dialog"
        fetchPage={fetchPage}
        queryKey={['lookup-test', 'server-search']}
        searchDebounceMs={300}
      />
    );

    fireEvent.click(screen.getByLabelText('Customer'));
    await waitFor(() =>
      expect(fetchPage).toHaveBeenCalledWith(expect.objectContaining({ search: '' }))
    );

    fireEvent.change(screen.getByPlaceholderText('Search options…'), {
      target: { value: 'omar' },
    });
    expect(fetchPage).toHaveBeenCalledTimes(1);

    await waitFor(
      () => expect(fetchPage).toHaveBeenCalledWith(expect.objectContaining({ search: 'omar' })),
      { timeout: 1000 }
    );
    expect(await screen.findByText('OMR - Server result for omar')).toBeDefined();
  });

  it('appends the next server page when lazy loading reaches the list bottom', async () => {
    const fetchPage = vi.fn(async ({ pageNumber }: { pageNumber: number }) => ({
      data:
        pageNumber === 1
          ? [{ id: '1', code: 'P1', name: 'First page' }]
          : [{ id: '2', code: 'P2', name: 'Second page' }],
      pageNumber,
      totalPages: 2,
      totalRecords: 2,
    }));

    renderWithQueryClient(
      <LookupField
        name="customer"
        label="Customer"
        searchable={false}
        sideMode="server"
        lazyLoading
        displayMode="dialog"
        fetchPage={fetchPage}
        queryKey={['lookup-test', 'lazy-loading']}
        pageSize={1}
      />
    );

    fireEvent.click(screen.getByLabelText('Customer'));
    expect(await screen.findByText('P1 - First page')).toBeDefined();

    const list = screen.getByRole('list');
    Object.defineProperties(list, {
      scrollHeight: { configurable: true, value: 300 },
      clientHeight: { configurable: true, value: 200 },
      scrollTop: { configurable: true, value: 100 },
    });
    fireEvent.scroll(list);

    expect(await screen.findByText('P2 - Second page')).toBeDefined();
    expect(screen.getByText('P1 - First page')).toBeDefined();
    expect(fetchPage).toHaveBeenCalledWith(expect.objectContaining({ pageNumber: 2, pageSize: 1 }));
  });
});
