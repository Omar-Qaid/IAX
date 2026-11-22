import React from 'react';
import { expect, it, vi } from 'vitest';
import { within } from '@testing-library/react';
import { fireEvent, render, screen } from '@test/testUtils';
import { usePaymentJournalActions } from '@modules/finance/accounts-receivable/payment/components/PaymentJournalShared';
import {
  ledgerJournalCustPaymApi,
  newPaymentJournal,
} from '@modules/finance/accounts-receivable/payment/api/ledgerJournalCustPaymApi';

function Harness() {
  const actions = usePaymentJournalActions();
  return (
    <>
      <button
        onClick={() =>
          actions.run({ ...newPaymentJournal('usmf'), recId: 8, journalNum: 'PAY-1' }, 'validate')
        }
      >
        Validate
      </button>
      {actions.dialogs}
    </>
  );
}

it('shows validation errors as errors, not as information', async () => {
  vi.spyOn(ledgerJournalCustPaymApi, 'validate').mockResolvedValue({
    valid: false,
    errors: ['Period is closed'],
    unverifiedChecks: ['Approval not checked'],
    scope: 'Static checks',
  });
  render(<Harness />);
  fireEvent.click(screen.getByRole('button', { name: 'Validate' }));

  const dialog = await screen.findByRole('dialog');
  const alertFor = (text: string) => within(dialog).getByText(text).closest('.MuiAlert-root');
  expect(alertFor('Validation failed.')).toHaveClass('MuiAlert-colorError');
  expect(alertFor('Period is closed')).toHaveClass('MuiAlert-colorError');
  expect(alertFor('Approval not checked')).toHaveClass('MuiAlert-colorInfo');
});
