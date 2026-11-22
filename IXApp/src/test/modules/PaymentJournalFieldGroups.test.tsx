import React from 'react';
import { expect, it } from 'vitest';
import { render, screen } from '@test/testUtils';
import { JournalFieldGroups } from '@modules/finance/accounts-receivable/payment/components/PaymentJournalShared';

it('renders titled field groups with text values and read-only Yes/No switches', () => {
  render(
    <JournalFieldGroups
      groups={[
        { title: 'Offset account', fields: [{ label: 'Account type', value: 'Ledger' }] },
        {
          title: 'Sales tax',
          fields: [{ label: 'Amounts include sales tax', value: 1, toggle: true }],
        },
        { title: 'Blocking', fields: [{ label: 'Locked by system', value: 0, toggle: true }] },
      ]}
    />
  );
  expect(screen.getByText('Offset account')).toBeInTheDocument();
  expect(screen.getByText('Ledger')).toBeInTheDocument();
  const tax = screen.getByRole('switch', { name: 'Amounts include sales tax' });
  expect(tax).toBeChecked();
  expect(tax).toBeDisabled();
  expect(screen.getByRole('switch', { name: 'Locked by system' })).not.toBeChecked();
  expect(screen.getByText('Yes')).toBeInTheDocument();
  expect(screen.getByText('No')).toBeInTheDocument();
});
