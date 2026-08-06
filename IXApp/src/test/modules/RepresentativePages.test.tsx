import React from 'react';
import { describe, expect, it } from 'vitest';
import { act, fireEvent, render, screen, waitFor } from '@test/testUtils';
import { DashboardPage } from '@modules/dashboard/pages/DashboardPage';
import { CurrencyPage } from '@modules/foundation/pages/CurrencyPage';
import { ExchangeRateTypePage } from '@modules/foundation/pages/ExchangeRateTypePage';
import { ExchangeRatePage } from '@modules/foundation/pages/ExchangeRatePage';
import { LegalEntityPage } from '@modules/organization-administration/pages/LegalEntityPage';
import { CustomerListPage } from '@modules/accounts-receivable/pages/CustomerListPage';
import { SalesOrderPage } from '@modules/accounts-receivable/pages/SalesOrderPage';
import { ApplicationSettingsPage } from '@modules/system-administration/pages/ApplicationSettingsPage';
import { CustomerGroupListPage } from '@modules/accounts-receivable/pages/CustomerGroupListPage';
import { SalesOrdersPage } from '@modules/accounts-receivable/pages/SalesOrdersPage';
import { CustPaymMode } from '@modules/accounts-receivable/pages/CustPaymModePage';
import { CustPaymTerm } from '@modules/accounts-receivable/pages/CustPaymTermPage';

describe('representative enterprise pages', () => {
  it('renders the workspace dashboard indicators', () => {
    render(<DashboardPage />);
    expect(screen.getByText('Business overview')).toBeDefined();
    expect(screen.getByText('Open sales orders')).toBeDefined();
  });

  it('renders simple-list and list-details representatives', () => {
    render(<CustomerListPage />);
    expect(screen.getAllByText('Contoso Retail Americas').length).toBeGreaterThan(0);
    expect(screen.getByText('Standard view')).toBeDefined();
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
    await waitFor(() => expect(screen.getByLabelText('Name').getAttribute('aria-invalid')).toBe('true'));
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Close' })));
    await waitFor(() => expect(screen.queryByRole('heading', { name: 'Create customer' })).toBeNull());
  }, 60_000);

  it('renders document and setup representatives as read-only pages', () => {
    const { unmount } = render(<SalesOrderPage />);
    expect(screen.getByText('Sales order SO-00101')).toBeDefined();
    expect(screen.getByText('Enterprise Server Rack Cabinet 42U')).toBeDefined();
    unmount();

    render(<ApplicationSettingsPage />);
    expect(screen.getByText('Current client configuration')).toBeDefined();
    expect(screen.getByText('API integration: ASP.NET Core REST')).toBeDefined();
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

  it('renders currency maintenance through the generic list-details pattern', () => {
    render(<CurrencyPage />);
    expect(screen.getByRole('heading', { name: 'Currencies' })).toBeDefined();
    expect(screen.getAllByText('AED').length).toBeGreaterThan(0);
    expect(screen.getByText('Currency converter')).toBeDefined();
    expect(screen.getByText('Rounding rules')).toBeDefined();
    expect(screen.getByText('Electronic invoices')).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });

  it('renders exchange rate types through the generic simple-list pattern', () => {
    render(<ExchangeRateTypePage />);
    expect(screen.getByText('Exchange rate types')).toBeDefined();
    expect(screen.getByText('Default average rate')).toBeDefined();
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

  it('renders legal entities through the generic list-details pattern', () => {
    render(<LegalEntityPage />);
    expect(screen.getByRole('heading', { name: 'Legal entities' })).toBeDefined();
    expect(screen.getAllByText('AlHayat Building Materials Company').length).toBeGreaterThan(0);
    expect(screen.getByText('Addresses')).toBeDefined();
    expect(screen.getByText('Contact information')).toBeDefined();
    expect(screen.getByText('Statutory reporting')).toBeDefined();
    expect(screen.getByRole('button', { name: 'View in hierarchy' })).toBeDefined();
    act(() => fireEvent.click(screen.getAllByRole('button', { name: 'Edit' })[0]));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });
});
