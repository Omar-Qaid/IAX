import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@test/testUtils';
import { DashboardPage } from '@modules/dashboard/pages/DashboardPage';
import { CurrencyPage } from '@modules/finance/foundation/pages/CurrencyPage';
import { ExchangeRateTypePage } from '@modules/finance/foundation/pages/ExchangeRateTypePage';
import { ExchangeRatePage } from '@modules/finance/foundation/pages/ExchangeRatePage';
import { LegalEntityPage } from '@modules/organization/pages/LegalEntityPage';
import { CustomerListPage } from '@modules/finance/accounts-receivable/pages/CustomerListPage';
import { SalesOrderPage } from '@modules/finance/accounts-receivable/pages/SalesOrderPage';
import { ApplicationSettingsPage } from '@modules/administration/pages/ApplicationSettingsPage';
import { CustomerGroupListPage } from '@modules/finance/accounts-receivable/pages/CustomerGroupListPage';
import { SalesOrdersPage } from '@modules/finance/accounts-receivable/pages/SalesOrdersPage';
import { CustPaymMode } from '@modules/finance/accounts-receivable/pages/CustPaymModePage';
import { CustPaymTerm } from '@modules/finance/accounts-receivable/pages/CustPaymTermPage';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { render as renderWithoutProviders } from '@testing-library/react';
import { AppProviders } from '@app/providers/AppProviders';

vi.mock('@modules/finance/foundation/api/currencyApi', () => ({
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

vi.mock('@modules/finance/foundation/api/exchangeRateTypeApi', () => ({
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

describe('representative enterprise pages', () => {
  it('renders the workspace dashboard indicators', () => {
    render(<DashboardPage />);
    expect(screen.getByText('Business overview')).toBeDefined();
    expect(screen.getByText('Open sales orders')).toBeDefined();
  });

  it('renders simple-list and list-details representatives', () => {
    render(<CustomerListPage />);
    expect(screen.getAllByText('Contoso Retail Americas').length).toBeGreaterThan(0);
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

  it('renders all routed accounts-receivable list pages', () => {
    const { unmount } = render(<CustomerGroupListPage />);
    expect(screen.getByText('Major Key Accounts')).toBeDefined();
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

    render(<SalesOrdersPage />);
    expect(screen.getByText('SO-00101')).toBeDefined();
  }, 60_000);

  it('opens customer quick create through the generic dialog fast-tabs pattern', async () => {
    render(<CustomerListPage />);
    act(() => fireEvent.click(screen.getByRole('button', { name: 'New' })));
    expect(screen.getByRole('heading', { name: 'Create customer' })).toBeDefined();
    expect(screen.getByText('Details')).toBeDefined();
    expect(screen.getByText('Address')).toBeDefined();
    expect(screen.getByDisplayValue('C-0004304')).toBeDefined();
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
    const { unmount } = render(<SalesOrderPage />);
    expect(screen.getByText('Sales order SO-00101')).toBeDefined();
    expect(screen.getByText('Enterprise Server Rack Cabinet 42U')).toBeDefined();
    unmount();

    render(<ApplicationSettingsPage />);
    expect(await screen.findByText('Current client configuration')).toBeDefined();
    expect(screen.getByRole('navigation', { name: 'Setup sections' })).toBeDefined();
    expect(screen.getByRole('textbox', { name: /Application name/ })).toBeDefined();
  });

  it('shows an error instead of substituting another order for an invalid route id', () => {
    renderWithoutProviders(
      <MemoryRouter initialEntries={['/accounts-receivable/sales-orders/missing-order']}>
        <AppProviders>
          <Routes>
            <Route
              path="/accounts-receivable/sales-orders/:salesOrderId"
              element={<SalesOrderPage />}
            />
          </Routes>
        </AppProviders>
      </MemoryRouter>
    );

    expect(screen.getByText('No sales orders are available.')).toBeDefined();
    expect(screen.queryByText('Sales order SO-00101')).toBeNull();
  });

  it('uses the generic enterprise list-details edit lifecycle', () => {
    render(<CustPaymMode />);
    expect(screen.getByText('Methods of payment - customers')).toBeDefined();
    expect(screen.getAllByText('Cash').length).toBeGreaterThan(0);
    act(() => fireEvent.click(screen.getAllByRole('button', { name: 'Edit' })[0]));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'New' })).toBeNull();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Cancel' })));
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByPlaceholderText('Filter')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Filter' })));
    expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Information' })));
    expect(screen.getByRole('heading', { name: 'Related information' })).toBeDefined();
  });

  it('renders payment terms through the generic list-details pattern', () => {
    render(<CustPaymTerm />);
    expect(screen.getByRole('heading', { name: 'Terms of payment' })).toBeDefined();
    expect(screen.getAllByText('07 Days').length).toBeGreaterThan(0);
    expect(screen.getByText('Setup')).toBeDefined();
    expect(screen.getByText('Other')).toBeDefined();
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
