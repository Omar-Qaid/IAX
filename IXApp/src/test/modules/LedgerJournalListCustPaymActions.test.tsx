import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { LedgerJournalListCustPaym } from '@modules/finance/accounts-receivable/payment/pages/LedgerJournalListCustPaym';
import {
  ledgerJournalCustPaymApi as api,
  newPaymentJournal,
  type PaymentJournal,
} from '@modules/finance/accounts-receivable/payment/api/ledgerJournalCustPaymApi';

const navigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react-router-dom')>()),
  useNavigate: () => navigate,
}));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: () => true, user: { id: 'u1' } }),
}));
vi.mock('@core/company/useCompanyStore', () => ({
  useCompanyStore: (select: (state: { currentCompany: string }) => unknown) =>
    select({ currentCompany: 'dat' }),
}));

const journal = (values: Partial<PaymentJournal>): PaymentJournal => ({
  ...newPaymentJournal('dat'),
  ...values,
});
const draft = journal({
  id: '21',
  recId: 21,
  journalNum: 'GJ-26-0001',
  journalName: 'CPS',
  name: 'Customer receipts',
  offsetLedgerDimension: 7,
  currencyCode: 'SAR',
  ledgerJournalInclTax: 1,
  detailSummaryPosting: 0,
  linesLimitBeforeDistribution: 1000,
  parentJournalNum: 'GJ-26-0000',
  workflowApprovalStatus: 0,
  log: 'Checked by validation',
  createdBy: 'u1',
});

beforeEach(() => {
  // The app query cache is shared by every test: start each one empty.
  queryClient.clear();
  vi.restoreAllMocks();
  navigate.mockReset();
  vi.spyOn(api, 'list').mockResolvedValue([draft]);
  vi.spyOn(api, 'names').mockResolvedValue([]);
  vi.spyOn(api, 'offsets').mockResolvedValue([
    { id: 7, code: '11010202', name: 'Bank account', accountType: 0 },
  ]);
});

const command = async (name: string) => screen.findByRole('button', { name });

describe('Customer payment journal list actions', () => {
  it('shows the D365 action pane: implemented actions enabled, the rest disabled', async () => {
    render(<LedgerJournalListCustPaym />);
    expect(await screen.findByText('GJ-26-0001')).toBeInTheDocument();
    for (const name of ['Lines', 'Validate', 'Simulate posting', 'Post'])
      await waitFor(async () => expect(await command(name)).toBeEnabled());
    for (const name of [
      'Enter customer payments',
      'Open lines in Excel',
      'Approval',
      'Inquiries',
      'Print',
      'Unlock',
    ])
      expect(await command(name)).toBeDisabled();
  });

  it('opens the journal lines', async () => {
    render(<LedgerJournalListCustPaym />);
    await screen.findByText('GJ-26-0001');
    await waitFor(async () => expect(await command('Lines')).toBeEnabled());
    fireEvent.click(await command('Lines'));
    expect(navigate).toHaveBeenCalledWith(expect.stringContaining('/21'));
  });

  it('validates and simulates posting, showing the server report', async () => {
    const validate = vi.spyOn(api, 'validate').mockResolvedValue({
      valid: false,
      errors: ['Line 1 has no account'],
      unverifiedChecks: [],
      scope: 'Static checks',
    });
    const simulate = vi.spyOn(api, 'simulate').mockResolvedValue({
      wouldPost: true,
      refusal: null,
      totalDebit: 100,
      totalCredit: 100,
      scope: 'Rolled back',
    });
    render(<LedgerJournalListCustPaym />);
    await screen.findByText('GJ-26-0001');
    await waitFor(async () => expect(await command('Validate')).toBeEnabled());

    fireEvent.click(await command('Validate'));
    expect(await screen.findByText('Line 1 has no account')).toBeInTheDocument();
    expect(validate).toHaveBeenCalledWith(21);
    fireEvent.click(screen.getByRole('button', { name: 'Close' }));

    fireEvent.click(await command('Simulate posting'));
    expect(await screen.findByText('Simulation succeeded.')).toBeInTheDocument();
    expect(simulate).toHaveBeenCalledWith(21);
  });

  it('posts only after confirmation, then reloads the journals', async () => {
    const post = vi.spyOn(api, 'post').mockResolvedValue();
    render(<LedgerJournalListCustPaym />);
    await screen.findByText('GJ-26-0001');
    await waitFor(async () => expect(await command('Post')).toBeEnabled());
    const loads = vi.mocked(api.list).mock.calls.length;

    fireEvent.click(await command('Post'));
    const dialog = await screen.findByRole('dialog');
    expect(dialog).toHaveTextContent('Post journal GJ-26-0001?');
    expect(post).not.toHaveBeenCalled();
    fireEvent.click(screen.getAllByRole('button', { name: 'Post' }).at(-1)!);
    await waitFor(() => expect(post).toHaveBeenCalledWith(21));
    await waitFor(() => expect(vi.mocked(api.list).mock.calls.length).toBeGreaterThan(loads));
  });

  it('deletes the selected unposted journal after confirmation', async () => {
    const remove = vi.spyOn(api.headers, 'delete').mockResolvedValue();
    render(<LedgerJournalListCustPaym />);
    await screen.findByText('GJ-26-0001');
    await waitFor(async () => expect(await command('Delete')).toBeEnabled());
    fireEvent.click(await command('Delete'));
    const confirm = await screen.findByRole('dialog');
    fireEvent.click(
      [...confirm.querySelectorAll('button')].find((button) =>
        /delete|yes/i.test(button.textContent ?? '')
      )!
    );
    await waitFor(() =>
      expect(remove).toHaveBeenCalledWith(expect.objectContaining({ recId: 21 }))
    );
  });

  it('shows the header tabs with the stored journal fields', async () => {
    render(<LedgerJournalListCustPaym />);
    await screen.findByText('GJ-26-0001');

    fireEvent.click(screen.getByRole('tab', { name: 'General' }));
    expect(await screen.findByText('Customer payment')).toBeInTheDocument();
    expect(screen.getByText('Current')).toBeInTheDocument();
    expect(screen.getByText('1000')).toBeInTheDocument();
    expect(screen.getByText('GJ-26-0000')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('tab', { name: 'Setup' }));
    expect(await screen.findByText('11010202 - Bank account')).toBeInTheDocument();
    expect(screen.getByRole('switch', { name: 'Amounts include sales tax' })).toBeChecked();
    expect(screen.getByText('Do not summarize')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('tab', { name: 'Blocking' }));
    expect(await screen.findByRole('switch', { name: 'Locked by system' })).not.toBeChecked();

    fireEvent.click(screen.getByRole('tab', { name: 'History' }));
    expect(await screen.findByText('Not submitted')).toBeInTheDocument();
    expect(screen.getByText('Checked by validation')).toBeInTheDocument();
  });

  it('disables journal actions on a posted journal', async () => {
    vi.mocked(api.list).mockResolvedValue([
      journal({ ...draft, posted: 1, postedDateTime: '2026-11-01T10:00:00' }),
    ]);
    render(<LedgerJournalListCustPaym />);
    // Posted journals are hidden by the default "Not posted" filter.
    fireEvent.mouseDown(await screen.findByRole('combobox', { name: 'Show' }));
    fireEvent.click(await screen.findByRole('option', { name: 'All' }));
    await screen.findByText('GJ-26-0001');
    for (const name of ['Validate', 'Simulate posting', 'Post'])
      expect(await command(name)).toBeDisabled();
    expect(await command('Lines')).toBeEnabled();
  });
});
