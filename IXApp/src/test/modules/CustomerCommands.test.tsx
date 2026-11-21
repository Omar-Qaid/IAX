import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent } from '@test/testUtils';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { buildCustomerCommands } from '@modules/finance/accounts-receivable/customer/components/customerCommands';
import type { CustomerRecord } from '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi';
import { MOCK_CUSTOMERS } from '@mocks/data/customers';

const customer: CustomerRecord = {
  ...MOCK_CUSTOMERS[0],
  recId: 1,
  party: 2,
  custCategory: '',
  paymTermId: '',
  paymModeId: '',
  dlvModeId: '',
  taxGroupId: '',
  vatNum: '',
  countryRegionId: '',
  invoiceAccount: '',
  inventSiteId: '',
  inventLocationId: '',
};
const context = () => ({
  t: (key: string, fallback?: string) =>
    fallback ??
    {
      'customerCommands.customer': 'Customer',
      'customerCommands.sell': 'Sell',
      'customerCommands.invoice': 'Invoice',
    }[key] ??
    key,
  hasPermission: () => true,
  navigate: vi.fn(),
  onContacts: vi.fn(),
  onNewSalesOrder: vi.fn(),
});

describe('shared customer commands', () => {
  it('keeps the order and routes actions to the selected customer', () => {
    const actions = context();
    const commands = buildCustomerCommands(customer, actions);
    expect(commands.map((command) => command.id)).toEqual([
      'customer',
      'sell',
      'invoice',
      'collect',
    ]);
    const sell = commands[1].groups.flatMap((group) => group.actions);
    sell.find((action) => action.id === 'new-sales-order')!.onClick!();
    expect(actions.onNewSalesOrder).toHaveBeenCalledWith(customer);
    sell.find((action) => action.id === 'orders')!.items![0].onClick();
    expect(actions.navigate).toHaveBeenCalledWith(
      expect.stringContaining(`customerAccount=${customer.accountNumber}`)
    );
    const invoiceJournal = commands[2].groups
      .flatMap((group) => group.actions)
      .find((action) => action.id === 'invoice-journal')!;
    expect(invoiceJournal.disabled).toBe(false);
    invoiceJournal.onClick!();
    expect(actions.navigate).toHaveBeenLastCalledWith(
      expect.stringContaining(customer.accountNumber)
    );
    // Collect is always shown; its payment journal needs the page's handler.
    const paymentJournal = commands[3].groups[0].actions[0];
    expect(paymentJournal).toMatchObject({ id: 'paymentJournal', disabled: true });
    expect(
      buildCustomerCommands(customer, { ...actions, hasPermission: () => false }).map(
        (command) => command.id
      )
    ).toEqual(['customer', 'collect']);
  });

  it('renders grouped ribbons on a simple list and opens the Orders dropdown', async () => {
    const actions = context();
    render(
      <SimpleListPage
        title="Customers"
        columns={[{ field: 'name', headerName: 'Name' }]}
        dataSource={{ type: 'controlled', rows: [customer] }}
        enterpriseConfig={{ getCommands: (record) => buildCustomerCommands(record, actions) }}
      />
    );
    fireEvent.click(await screen.findByRole('button', { name: 'Sell' }));
    fireEvent.click(await screen.findByRole('button', { name: /Orders/ }));
    fireEvent.click(await screen.findByRole('menuitem', { name: 'All sales orders' }));
    expect(actions.navigate).toHaveBeenCalledWith(
      expect.stringContaining(`customerAccount=${customer.accountNumber}`)
    );
    fireEvent.click(screen.getByRole('button', { name: 'Invoice' }));
    expect(await screen.findByRole('button', { name: 'Invoice journal' })).toBeEnabled();
  });
});
