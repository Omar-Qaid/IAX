import React from 'react';
import { describe, expect, it } from 'vitest';
import { act, fireEvent, render, screen } from '@test/testUtils';
import { DashboardPage } from '@modules/dashboard/pages/DashboardPage';
import { CurrenciesPage } from '@modules/foundation/pages/CurrenciesPage';
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
    const { unmount } = render(<CurrenciesPage />);
    expect(screen.getByText('US Dollar')).toBeDefined();
    unmount();

    render(<CustomerListPage />);
    expect(screen.getAllByText('Contoso Retail Americas').length).toBeGreaterThan(0);
    expect(screen.getByText('Standard view')).toBeDefined();
    expect(screen.getByText('Arabic name')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Filter' })));
    expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Information' })));
    expect(screen.getByRole('heading', { name: 'Related information' })).toBeDefined();
  }, 15_000);

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
  }, 15_000);

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
    act(() => fireEvent.click(screen.getByRole('button', { name: 'Edit' })));
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
});
