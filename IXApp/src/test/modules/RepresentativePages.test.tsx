import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@test/testUtils';
import { DashboardPage } from '@modules/dashboard/pages/DashboardPage';
import { CurrencyPage } from '@modules/finance/foundation/currency/pages/CurrencyPage';
import { ExchangeRateTypePage } from '@modules/finance/foundation/exchange-rate/pages/ExchangeRateTypePage';
import { ExchangeRatePage } from '@modules/finance/foundation/exchange-rate/pages/ExchangeRatePage';
import { LegalEntityPage } from '@modules/organization/pages/LegalEntityPage';
import { CustomerListPage } from '@modules/finance/accounts-receivable/customer/pages/CustomerListPage';
import { SalesOrderDetailsPage } from '@modules/finance/accounts-receivable/sales-order/pages/SalesOrderDetailsPage';
import { ApplicationSettingsPage } from '@modules/administration/pages/ApplicationSettingsPage';
import { CustomerGroupListPage } from '@modules/finance/accounts-receivable/customer-group/pages/CustomerGroupListPage';
import { SalesOrderListPage } from '@modules/finance/accounts-receivable/sales-order/pages/SalesOrderListPage';
import { CustPaymMode } from '@modules/finance/accounts-receivable/setup/pages/CustPaymModePage';
import { CustPaymTerm } from '@modules/finance/accounts-receivable/setup/pages/CustPaymTermPage';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render as renderWithoutProviders } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';

vi.mock('@modules/finance/foundation/currency/api/currencyApi', () => ({
  currencyApi: {
    list: vi.fn().mockResolvedValue([
      {
        id: '1',
        recId: 1,
        currencyCode: 'AED',
        currencyCodeIso: 'AED',
        txt: 'UAE Dirham',
        symbol: 'د.إ',
        isEuro: 0,
        roundOffSales: 0.01,
        roundOffTypeSales: 0,
        roundOffPurch: 0.01,
        roundOffTypePurch: 0,
        roundOffPrice: 0.01,
        roundOffTypePrice: 0,
        roundingPrecision: 0.01,
        ltmRoundOffLineAmount: 0,
        ltmRoundOffTypeLineAmount: 0,
        isActive: true,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: 'dat',
      },
    ]),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('@modules/finance/foundation/exchange-rate/api/exchangeRateTypeApi', () => ({
  exchangeRateTypeApi: {
    list: vi.fn().mockResolvedValue([
      {
        id: '1',
        recId: 1,
        type: 'Average',
        name: 'Default average rate',
        isActive: true,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: 'dat',
      },
      {
        id: '2',
        recId: 2,
        type: 'Budget',
        name: 'Default budget rate',
        isActive: true,
        rowVersion: null,
        recVersion: 1,
        dataAreaId: 'dat',
      },
    ]),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock(
  '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi',
  async (importOriginal) => ({
    ...(await importOriginal<object>()),
    customerQuickCreateApi: {
      list: vi.fn(async () => (await import('@mocks/data/customers')).MOCK_CUSTOMERS),
      groupFieldLookups: vi.fn(async () => ({ paymentTerms: [], salesTaxGroups: [] })),
      lookups: vi.fn(async () => ({
        customerGroups: [],
        salesTaxGroups: [],
        currencies: [],
        countryRegions: [],
        paymentTerms: [],
        paymentMethods: [],
        deliveryTerms: [],
        deliveryModes: [],
        salesPools: [],
        paymentSchedules: [],
      })),
    },
  })
);
vi.mock(
  '@modules/finance/accounts-receivable/customer-group/api/customerGroupApi',
  async (importOriginal) => ({
    ...(await importOriginal<object>()),
    customerGroupApi: {
      list: vi.fn(async () => [
        {
          id: '1',
          recId: 1,
          custGroupId: 'CG-MAJOR',
          name: 'Major Key Accounts',
          paymTermId: '',
          taxGroupId: '',
        },
      ]),
    },
  })
);
vi.mock('@modules/finance/accounts-receivable/setup/api/customerPaymentSetupApi', () => ({
  customerPaymentSetupApi: (kind: string) => ({
    list: async () => [
      {
        id: '1',
        recId: 1,
        code: kind === 'terms' ? '07 Days' : 'Cash',
        description: kind === 'terms' ? 'Seven days' : 'Cash payment',
        days: 7,
        months: 0,
        source: {},
      },
    ],
  }),
}));
async function ordersFixture() {
  return (await import('@mocks/data/salesOrders')).MOCK_SALES_ORDERS.map((row) => ({
    ...row,
    salesId: row.salesOrderNumber,
    currencyCode: row.currency,
    invoiceAccount: row.customerAccount,
    salesStatus: 'Backorder',
    deliveryDate: row.requestedDeliveryDate,
  }));
}
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderListApi', () => ({
  salesOrderListApi: {
    list: vi.fn(async () => ordersFixture()),
    get: vi.fn(async (id: string) => (await ordersFixture()).find((row) => row.id === id) ?? null),
  },
}));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: {
    list: vi.fn(
      async (id: string) => (await ordersFixture()).find((row) => row.id === id)?.lines ?? []
    ),
    totals: vi.fn(async () => ({
      subtotal: 12000,
      totalDiscount: 500,
      salesTax: 1150,
      totalCharges: 0,
      invoiceAmount: 12650,
      quantity: 6,
      costValue: 0,
    })),
    deliveryAddresses: vi.fn(async () => []),
    inventoryDimensions: vi.fn(async () => ({ sites: [], warehouses: [], locations: [] })),
    taxGroups: vi.fn(async () => []),
  },
}));

describe('representative enterprise pages', () => {
  it('remembers the filter row independently for each grid', async () => {
    const openGrid = (storageKey: string) =>
      render(
        <SimpleListPage
          title="Customers"
          columns={[{ field: 'id', headerName: 'Account' }]}
          dataSource={{ type: 'controlled', rows: [{ id: 'C-001' }] }}
          dataGridProps={{ storageKey, hideToolbar: true }}
        />
      );
    const first = openGrid('test.filter-customers');
    expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
    fireEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect((await screen.findAllByPlaceholderText('Filter value')).length).toBeGreaterThan(0);
    first.unmount();

    const other = openGrid('test.filter-orders');
    expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
    other.unmount();
    const second = openGrid('test.filter-customers');
    expect((await screen.findAllByPlaceholderText('Filter value')).length).toBeGreaterThan(0);
    fireEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
    second.unmount();

    openGrid('test.filter-customers');
    expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
  });

  it('renders the workspace dashboard indicators', () => {
    render(<DashboardPage />);
    expect(screen.getByText('Business overview')).toBeDefined();
    expect(screen.getByText('Open sales orders')).toBeDefined();
  });

  it('renders simple-list and list-details representatives', async () => {
    render(<CustomerListPage />);
    expect((await screen.findAllByText('Contoso Retail Americas')).length).toBeGreaterThan(0);
    expect(screen.queryByText('Standard view')).toBeNull();
    expect(screen.getByText('Arabic name')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Filter' })));
    expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: /Add/ })));
    expect(screen.getAllByText('Customer account (Account)')).toHaveLength(2);
    const operatorSelects = screen.getAllByRole('combobox');
    act(() => fireEvent.mouseDown(operatorSelects.at(-1)!));
    act(() => fireEvent.click(screen.getByRole('option', { name: 'equals' })));
    expect(screen.getAllByText('equals').length).toBeGreaterThan(0);
    act(() => fireEvent.click(screen.getAllByRole('button', { name: 'Reset' })[0]));
    expect(screen.getAllByText('Customer account (Account)')).toHaveLength(1);
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Information' })));
    expect(screen.getByRole('heading', { name: 'Related information' })).toBeDefined();
  }, 60_000);

  it('renders all routed accounts-receivable list pages', async () => {
    const { unmount } = render(<CustomerGroupListPage />);
    expect(await screen.findByText('Major Key Accounts')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByDisplayValue('CG-MAJOR')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'New' })).toBeNull();
    expect(screen.getByRole('button', { name: 'Filter' })).toHaveProperty('disabled', true);
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Cancel' })));
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByText('CG-MAJOR')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Filter' })));
    expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'Information' })).toBeNull();
    unmount();

    render(<SalesOrderListPage />);
    expect(await screen.findByText('SO-00101')).toBeDefined();
  }, 60_000);

  it('opens customer quick create through the generic dialog fast-tabs pattern', async () => {
    render(<CustomerListPage />);
    act(() => fireEvent.click(screen.getByRole('button', { name: 'New' })));
    expect(screen.getByRole('heading', { name: 'Create customer' })).toBeDefined();
    expect(screen.getByText('Details')).toBeDefined();
    expect(screen.getByText('Address')).toBeDefined();
    expect(screen.getByDisplayValue('—')).toBeDefined();
    await screen.findByLabelText('Name');
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Save' })));
    expect(screen.getByRole('heading', { name: 'Create customer' })).toBeDefined();
    await waitFor(() =>
      expect(screen.getByLabelText('Name').getAttribute('aria-invalid')).toBe('true')
    );
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Close' })));
    await waitFor(() =>
      expect(screen.queryByRole('heading', { name: 'Create customer' })).toBeNull()
    );
  }, 60_000);

  it('renders document and setup representatives', async () => {
    const { unmount } = render(<SalesOrderDetailsPage />);
    expect(await screen.findByText('SO-00101 : Contoso Retail Americas')).toBeDefined();
    expect(
      (await screen.findAllByText('Enterprise Server Rack Cabinet 42U')).length
    ).toBeGreaterThan(0);
    unmount();

    render(<ApplicationSettingsPage />);
    expect(await screen.findByText('Current client configuration')).toBeDefined();
    expect(screen.getByRole('navigation', { name: 'Setup sections' })).toBeDefined();
    expect(screen.getByRole('textbox', { name: /Application name/ })).toBeDefined();
  });

  it('filters master records and shows the selected sales line details', async () => {
    render(<SalesOrderDetailsPage />);
    expect(await screen.findByText('Line details')).toBeDefined();
    const switchLine = await screen.findByText('High Performance Switch 48-Port');
    act(() => fireEvent.click(switchLine));
    expect(screen.getByText('ITEM-B20').closest('[role="row"]')).toHaveAttribute(
      'aria-selected',
      'true'
    );
    // The record list remembers whether it is open; open it only when it is closed.
    const toggle = screen.getByRole('button', { name: 'Toggle record list' });
    if (toggle.getAttribute('aria-pressed') !== 'true') act(() => fireEvent.click(toggle));
    act(() =>
      fireEvent.change(screen.getByRole('textbox', { name: 'Filter' }), {
        target: { value: 'Fabrikam' },
      })
    );
    expect(screen.queryByText('SO-00101')).toBeNull();
    expect(screen.getByText('SO-00102')).toBeDefined();
    expect(await screen.findByText('SO-00101 : Contoso Retail Americas')).toBeDefined();
  });

  it('shows an error instead of substituting another order for an invalid route id', async () => {
    renderWithoutProviders(
      <MemoryRouter initialEntries={['/accounts-receivable/sales-orders/missing-order']}>
        <AppProviders>
          <Routes>
            <Route
              path="/accounts-receivable/sales-orders/:salesOrderId"
              element={<SalesOrderDetailsPage />}
            />
          </Routes>
        </AppProviders>
      </MemoryRouter>
    );

    expect(await screen.findByText('No sales orders are available.')).toBeDefined();
    expect(screen.queryByText('Sales order SO-00101')).toBeNull();
  });

  it('uses the generic enterprise list-details edit lifecycle', async () => {
    render(<CustPaymMode />);
    expect(await screen.findByText('Methods of payment - customers')).toBeDefined();
    expect(screen.getAllByText('Cash').length).toBeGreaterThan(0);
    act(() => fireEvent.click(screen.getAllByRole('button', { name: 'Edit' })[0]));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'New' })).toBeNull();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Cancel' })));
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByPlaceholderText('Filter')).toBeDefined();
    const filterCommand = screen.getByRole('button', { name: 'Filter' });
    act(() => fireEvent.click(filterCommand));
    expect(screen.queryByPlaceholderText('Filter')).toBeNull();
    act(() => fireEvent.click(filterCommand));
    expect(screen.getByPlaceholderText('Filter')).toBeDefined();
  });

  it('renders payment terms through the generic list-details pattern', async () => {
    render(<CustPaymTerm />);
    expect(await screen.findByRole('heading', { name: 'Terms of payment' })).toBeDefined();
    expect(screen.getAllByText('07 Days').length).toBeGreaterThan(0);
    expect(screen.getByText('Setup')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });

  it('renders currency maintenance through the generic list-details pattern', async () => {
    render(<CurrencyPage />);
    expect((await screen.findAllByText('AED')).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Currencies').length).toBeGreaterThan(0);
    expect(screen.getByText('Rounding rules')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });

  it('renders exchange rate types through the generic simple-list pattern', async () => {
    render(<ExchangeRateTypePage />);
    expect(screen.getByText('Exchange rate types')).toBeDefined();
    expect(await screen.findByText('Default average rate')).toBeDefined();
    expect(screen.getByText('Default budget rate')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Exchange rates' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });

  it('renders exchange rates through the tabular list-details pattern', () => {
    render(<ExchangeRatePage />);
    expect(screen.getByRole('heading', { name: 'Currency exchange rates' })).toBeDefined();
    expect(screen.getAllByText('AED').length).toBeGreaterThan(0);
    expect(screen.getByText('Add or remove exchange rates')).toBeDefined();
    expect(screen.getByText('1.02339 SAR for 1 AED')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Add' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Remove' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });

  it('renders contract-backed legal entities through the generic list-details pattern', async () => {
    render(<LegalEntityPage />);
    expect(await screen.findByRole('heading', { name: 'Legal entities' })).toBeDefined();
    expect(screen.getAllByText('AlHayat Building Materials Company').length).toBeGreaterThan(0);
    expect(screen.getByText('Statutory reporting')).toBeDefined();
    expect(screen.getByText('Currency')).toBeDefined();
    act(() => fireEvent.click(screen.getAllByRole('button', { name: 'Edit' })[0]));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });
});
