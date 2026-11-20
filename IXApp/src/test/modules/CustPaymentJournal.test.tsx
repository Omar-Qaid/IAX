import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { CustPaymentJournalDialog } from '@modules/finance/accounts-receivable/payment/components/CustPaymentJournalDialog';
import { custPaymentApi } from '@modules/finance/accounts-receivable/payment/api/custPaymentApi';
import { custOpenTransApi } from '@modules/finance/accounts-receivable/settlement/api/custOpenTransApi';
import { buildCustomerCollectRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerCollectRibbon';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/payment/api/custPaymentApi', () => ({
  custPaymentApi: { bankAccounts: vi.fn(), post: vi.fn() },
}));
vi.mock('@modules/finance/accounts-receivable/settlement/api/custOpenTransApi', () => ({
  custOpenTransApi: { load: vi.fn(), post: vi.fn() },
}));

const openTrans = {
  accountNum: 'C-001',
  customerName: 'Customer',
  customerCurrency: 'SAR',
  transactions: [
    { recId: 1, invoice: 'INV-1', transDate: '2026-01-02', dueDate: '2026-02-01', amountCur: 100 },
  ],
} as unknown as Awaited<ReturnType<typeof custOpenTransApi.load>>;

const renderDialog = (account: string) => {
  render(
    <MemoryRouter>
      <AppProviders>
        <CustPaymentJournalDialog
          open
          custAccount={account}
          customerName="Customer"
          canPost
          onClose={vi.fn()}
        />
      </AppProviders>
    </MemoryRouter>
  );
};

const primeApis = () => {
  vi.mocked(custPaymentApi.bankAccounts).mockResolvedValue([
    { accountId: 'BANK-1', name: 'Main account', currencyCode: 'SAR' },
  ]);
  vi.mocked(custOpenTransApi.load).mockResolvedValue(openTrans);
};

const selectBank = async () => {
  // The select is disabled while the bank accounts load, and a disabled select ignores
  // mouseDown, so wait for it to be ready before opening the list.
  const bank = await screen.findByRole('combobox', { name: /bank account/i });
  await waitFor(() => expect(bank).not.toHaveAttribute('aria-disabled', 'true'));
  fireEvent.mouseDown(bank);
  fireEvent.click(await screen.findByRole('option', { name: /BANK-1/ }));
};

it('posts a payment against the chosen bank account and settles the oldest invoices', async () => {
  primeApis();
  vi.mocked(custPaymentApi.post).mockResolvedValue({
    journalNum: 'PAY-1',
    voucher: 'V-1',
    amount: 100,
  });
  renderDialog('C-001');

  // The post button stays disabled until a bank account and a positive amount are given.
  const post = screen.getByRole('button', { name: /post payment/i });
  expect(post).toBeDisabled();

  await selectBank();
  fireEvent.change(screen.getByLabelText('Amount'), { target: { value: '100' } });

  await waitFor(() => expect(post).toBeEnabled());
  fireEvent.click(post);

  await waitFor(() => expect(custPaymentApi.post).toHaveBeenCalledTimes(1));
  const [request] = vi.mocked(custPaymentApi.post).mock.calls[0];
  expect(request.custAccount).toBe('C-001');
  expect(request.bankAccountId).toBe('BANK-1');
  expect(request.amount).toBe(100);
  expect(request.settleAutomatically).toBe(true);
  expect(request.prepayment).toBe(false);
  expect(request.settleInvoiceId).toBeUndefined();
  expect(request.requestId).toBeTruthy();
  expect(await screen.findByText(/PAY-1/)).toBeInTheDocument();
});

it('turns settlement off for a prepayment, which has no invoice to settle', async () => {
  primeApis();
  vi.mocked(custPaymentApi.post).mockClear();
  renderDialog('C-002');

  const settle = screen.getByLabelText(/settle automatically/i);
  expect(settle).toBeChecked();
  fireEvent.click(screen.getByLabelText(/prepayment/i));
  await waitFor(() => expect(settle).not.toBeChecked());
  expect(settle).toBeDisabled();
});

it('refuses a zero or negative amount', async () => {
  primeApis();
  vi.mocked(custPaymentApi.post).mockClear();
  renderDialog('C-003');

  await selectBank();
  fireEvent.change(screen.getByLabelText('Amount'), { target: { value: '0' } });

  const post = screen.getByRole('button', { name: /post payment/i });
  await waitFor(() => expect(post).toBeDisabled());
  expect(custPaymentApi.post).not.toHaveBeenCalled();
});

it('enables the payment journal ribbon action only when a handler is supplied', () => {
  const withoutHandler = buildCustomerCollectRibbonGroups((_key, fallback) => fallback);
  const withHandler = buildCustomerCollectRibbonGroups((_key, fallback) => fallback, {
    onPaymentJournal: vi.fn(),
  });
  const find = (groups: ReturnType<typeof buildCustomerCollectRibbonGroups>) =>
    groups.flatMap((group) => group.actions).find((action) => action.id === 'paymentJournal');

  expect(find(withoutHandler)?.disabled).toBe(true);
  expect(find(withHandler)?.disabled).toBe(false);
  expect(find(withHandler)?.onClick).toBeTypeOf('function');
});
