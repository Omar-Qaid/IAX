import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { within } from '@testing-library/react';
import { queryClient } from '@core/api/queryClient';
import { LedgerJournalTransCustPaym } from '@modules/finance/accounts-receivable/payment/pages/LedgerJournalTransCustPaym';
import {
  ledgerJournalCustPaymApi as api,
  newPaymentJournal,
  newPaymentJournalLine,
  type PaymentJournal,
  type PaymentJournalLine,
} from '@modules/finance/accounts-receivable/payment/api/ledgerJournalCustPaymApi';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi';
import { currencyApi } from '@modules/finance/foundation/currency/api/currencyApi';
import { custOpenTransApi } from '@modules/finance/accounts-receivable/settlement/api/custOpenTransApi';

const navigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react-router-dom')>()),
  useNavigate: () => navigate,
  useParams: () => ({ journalId: '21' }),
}));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: () => true, user: { id: 'u1' } }),
}));
vi.mock('@core/company/useCompanyStore', () => ({
  useCompanyStore: (select: (state: { currentCompany: string }) => unknown) =>
    select({ currentCompany: 'dat' }),
}));
vi.mock('@modules/finance/accounts-receivable/setup/api/customerPaymentSetupApi', () => ({
  customerPaymentSetupApi: () => ({ list: vi.fn().mockResolvedValue([]) }),
}));

const header: PaymentJournal = {
  ...newPaymentJournal('dat'),
  id: '21',
  recId: 21,
  journalNum: 'GJ-26-0001',
  journalName: 'CPS',
  currencyCode: 'SAR',
  offsetLedgerDimension: 7,
};
const line: PaymentJournalLine = {
  ...newPaymentJournalLine(header),
  id: '31',
  recId: 31,
  lineNum: 1,
  voucher: 'JV-26-001829',
  transDate: '2026-10-28T00:00:00',
  documentDate: '2026-10-28T00:00:00',
  due: '2026-10-28T00:00:00',
  ledgerDimensionName: 'C-001',
  amountCurCredit: 100,
  offsetLedgerDimension: 7,
  txt: 'Example payment',
  paymReference: '07:15',
  postingProfile: 'GEN',
  qty: 0,
  paymSpec: 'SPEC',
  bankTransType: '01',
  transferredBy: 'batch',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  navigate.mockReset();
  vi.spyOn(api, 'get').mockResolvedValue(header);
  vi.spyOn(api, 'journalLines').mockResolvedValue([line]);
  vi.spyOn(api, 'offsets').mockResolvedValue([
    { id: 7, code: '11010202', name: 'Bank', accountType: 0 },
  ]);
  vi.spyOn(customerQuickCreateApi, 'list').mockResolvedValue([
    { id: 'cust-1', accountNumber: 'C-001', name: 'Customer one', currencyCode: 'SAR' },
  ] as never);
  vi.spyOn(currencyApi, 'list').mockResolvedValue([{ currencyCode: 'SAR', txt: 'Riyal' }] as never);
});

const button = (name: string) => screen.findByRole('button', { name });
const loaded = async () => {
  render(<LedgerJournalTransCustPaym />);
  expect(await screen.findByText('JV-26-001829')).toBeInTheDocument();
};

describe('Customer payment journal lines actions', () => {
  it('shows the D365 action pane: implemented actions enabled, the rest disabled', async () => {
    await loaded();
    const byName = (name: string) => screen.getByRole('button', { name });
    await waitFor(() => expect(byName('Customer transactions')).toBeEnabled());
    for (const name of ['Post', 'Validate', 'Simulate posting', 'General journals'])
      expect(byName(name)).toBeEnabled();
    for (const name of [
      'Post in batch',
      'Approval',
      'Period journal',
      'Functions',
      'Fixed assets',
      'Inquiries',
      'Print',
    ])
      expect(byName(name)).toBeDisabled();
  });

  it('shows the D365 line footer for the selected line', async () => {
    await loaded();
    // Also a grid column/cell: the footer adds one more.
    expect((await screen.findAllByText('Account name')).length).toBeGreaterThan(0);
    expect((await screen.findAllByText('Customer one')).length).toBeGreaterThan(0);
    expect((await screen.findAllByText('Bank')).length).toBeGreaterThan(0);
    expect(screen.getByText('Calculated sales tax amount')).toBeInTheDocument();
    expect(screen.getByText('Actual sales tax amount')).toBeInTheDocument();
    expect(screen.getAllByText('2026-10-28').length).toBeGreaterThan(0);
    expect(screen.getByText('Budget check not performed')).toBeInTheDocument();
    expect(screen.getByRole('table', { name: 'Journal totals' })).toHaveTextContent('100.00');
  });

  it('navigates to customer transactions and back to the journals', async () => {
    await loaded();
    await waitFor(async () => expect(await button('Customer transactions')).toBeEnabled());
    fireEvent.click(await button('Customer transactions'));
    expect(navigate).toHaveBeenLastCalledWith('/accounts-receivable/CustTrans/C-001');
    fireEvent.click(await button('General journals'));
    expect(navigate).toHaveBeenLastCalledWith('/accounts-receivable/LedgerJournalListCustPaym');
  });

  it('edits a line and saves it with the full record', async () => {
    const update = vi.spyOn(api.lines, 'update').mockImplementation(async (row) => row);
    await loaded();
    await waitFor(async () => expect(await button('Edit')).toBeEnabled());
    fireEvent.click(await button('Edit'));
    const description = await screen.findByLabelText('txt');
    fireEvent.change(description, { target: { value: 'Changed payment' } });
    fireEvent.click(await button('Save'));
    await waitFor(() =>
      expect(update).toHaveBeenCalledWith(
        expect.objectContaining({
          recId: 31,
          txt: 'Changed payment',
          transDate: '2026-10-28',
          paymSpec: 'SPEC',
        })
      )
    );
  });

  it('cancels an edit without saving', async () => {
    const update = vi.spyOn(api.lines, 'update');
    await loaded();
    await waitFor(async () => expect(await button('Edit')).toBeEnabled());
    fireEvent.click(await button('Edit'));
    fireEvent.change(await screen.findByLabelText('txt'), { target: { value: 'Discard me' } });
    fireEvent.click(await button('Cancel'));
    expect(await screen.findByText('Example payment')).toBeInTheDocument();
    expect(update).not.toHaveBeenCalled();
  });

  it('refuses to save a new line without the required fields', async () => {
    const create = vi.spyOn(api.lines, 'create');
    await loaded();
    await waitFor(async () => expect(await button('+ New')).toBeEnabled());
    fireEvent.click(await button('+ New'));
    fireEvent.click(await button('Save'));
    expect(
      await screen.findByText('Date, customer account and currency are required.')
    ).toBeInTheDocument();
    expect(create).not.toHaveBeenCalled();
  });

  it('deletes the selected line after confirmation', async () => {
    const remove = vi.spyOn(api.lines, 'delete').mockResolvedValue();
    await loaded();
    await waitFor(async () => expect(await button('Delete')).toBeEnabled());
    fireEvent.click(await button('Delete'));
    const dialog = await screen.findByRole('dialog');
    fireEvent.click(within(dialog).getByRole('button', { name: 'Delete' }));
    await waitFor(() =>
      expect(remove).toHaveBeenCalledWith(expect.objectContaining({ recId: 31 }))
    );
  });

  it('marks an open invoice of the same amount and saves the settlement', async () => {
    vi.spyOn(custOpenTransApi, 'load').mockResolvedValue({
      transactions: [
        {
          transRecId: 501,
          invoice: 'INV-1',
          transDate: '2026-10-01T00:00:00',
          amountCur: 100.0000001,
          currencyCode: 'SAR',
        },
        {
          transRecId: 502,
          invoice: 'INV-2',
          transDate: '2026-10-01T00:00:00',
          amountCur: 250,
          currencyCode: 'SAR',
        },
      ],
    } as never);
    const update = vi.spyOn(api.lines, 'update').mockImplementation(async (row) => row);
    await loaded();
    await waitFor(async () => expect(await button('Settle transactions')).toBeEnabled());
    fireEvent.click(await button('Settle transactions'));
    const dialog = await screen.findByRole('dialog');
    await within(dialog).findByText(/INV-1/);
    const marks = within(dialog).getAllByRole('button', { name: 'Mark' });
    expect(marks[1]).toBeDisabled(); // different amount
    fireEvent.click(marks[0]);
    fireEvent.click(await button('Save'));
    await waitFor(() =>
      expect(update).toHaveBeenCalledWith(
        expect.objectContaining({ invoice: 'INV-1', markedInvoiceRecId: 501, settleVoucher: 2 })
      )
    );
  });

  it('validates, simulates and posts the journal (post after confirmation)', async () => {
    vi.spyOn(api, 'validate').mockResolvedValue({
      valid: true,
      errors: [],
      unverifiedChecks: [],
      scope: 'Static checks',
    });
    vi.spyOn(api, 'simulate').mockResolvedValue({
      wouldPost: true,
      refusal: null,
      totalDebit: 100,
      totalCredit: 100,
      scope: 'Rolled back',
    });
    const post = vi.spyOn(api, 'post').mockResolvedValue();
    await loaded();
    await waitFor(async () => expect(await button('Validate')).toBeEnabled());
    fireEvent.click(await button('Validate'));
    expect(await screen.findByText('Static validation passed.')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Close' }));
    fireEvent.click(await button('Simulate posting'));
    expect(await screen.findByText('Simulation succeeded.')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: 'Close' }));
    fireEvent.click(await button('Post'));
    const dialog = await screen.findByRole('dialog');
    fireEvent.click(within(dialog).getByRole('button', { name: 'Post' }));
    await waitFor(() => expect(post).toHaveBeenCalledWith(21));
  });

  it('shows the D365 line tabs with the stored line fields', async () => {
    await loaded();
    fireEvent.click(screen.getByRole('tab', { name: 'General' }));
    expect(await screen.findByText('Identification')).toBeInTheDocument();
    expect(screen.getAllByText('JV-26-001829').length).toBeGreaterThan(0);
    fireEvent.click(screen.getByRole('tab', { name: 'Invoice' }));
    expect(await screen.findByText('GEN')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('tab', { name: 'Payment' }));
    expect(await screen.findByText('SPEC')).toBeInTheDocument();
    expect(screen.getByText('01')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('tab', { name: 'History' }));
    expect(await screen.findByText('batch')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('tab', { name: 'Payment fee' }));
    expect(await screen.findByText(/Payment fees are not available/)).toBeInTheDocument();
  });

  it('keeps a posted journal read-only and opens its voucher', async () => {
    vi.mocked(api.get).mockResolvedValue({ ...header, posted: 1 });
    await loaded();
    for (const name of ['Post', 'Validate', 'Simulate posting', 'Edit', '+ New', 'Delete'])
      expect(await button(name)).toBeDisabled();
    await waitFor(async () => expect(await button('Voucher')).toBeEnabled());
    fireEvent.click(await button('Voucher'));
    expect(navigate).toHaveBeenLastCalledWith(expect.stringContaining('voucher=JV-26-001829'));
  });
});
