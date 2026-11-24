import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { within } from '@testing-library/react';
import { queryClient } from '@core/api/queryClient';
import { LedgerJournalListVendPaym } from '@modules/finance/accounts-payable/payment/pages/LedgerJournalListVendPaym';
import { LedgerJournalTransVendPaym } from '@modules/finance/accounts-payable/payment/pages/LedgerJournalTransVendPaym';
import { LedgerJournalTransDaily } from '@modules/finance/general-ledger/journal/pages/LedgerJournalDailyPages';
import { LedgerJournalLinesPage } from '@modules/finance/general-ledger/journal/pages/LedgerJournalLinesPage';
import { GENERAL_JOURNAL } from '@modules/finance/general-ledger/journal/generalJournal';
import {
  ledgerJournalApiFor,
  newPaymentJournal,
  newPaymentJournalLine,
  type PaymentJournal,
  type PaymentJournalLine,
} from '@modules/finance/general-ledger/journal/api/ledgerJournalApi';
import { vendorJournalApi } from '@modules/finance/accounts-payable/payment/api/vendorJournalApi';
import { currencyApi } from '@modules/finance/foundation/currency/api/currencyApi';
import { VendTrans } from '@modules/finance/accounts-payable/vendor/pages/VendTrans';
import { vendTransApi } from '@modules/finance/accounts-payable/vendor/api/vendTransApi';

const navigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react-router-dom')>()),
  useNavigate: () => navigate,
  useParams: () => ({ journalId: '41', accountNum: 'V-001' }),
}));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: () => true, user: { id: 'u1' } }),
}));
vi.mock('@core/company/useCompanyStore', () => ({
  useCompanyStore: (select: (state: { currentCompany: string }) => unknown) =>
    select({ currentCompany: 'dat' }),
}));

const vendApi = ledgerJournalApiFor(2);
const dailyApi = ledgerJournalApiFor(0);
const header = (journalType: 0 | 4): PaymentJournal => ({
  ...newPaymentJournal('dat', journalType),
  id: '41',
  recId: 41,
  journalNum: 'GJ-26-0041',
  journalName: journalType === 4 ? 'VPS' : 'JV',
  currencyCode: 'SAR',
  offsetLedgerDimension: 7,
});
const line = (journalType: 0 | 2, values: Partial<PaymentJournalLine>): PaymentJournalLine => ({
  ...newPaymentJournalLine(header(journalType), journalType === 4 ? 2 : 0),
  id: '51',
  recId: 51,
  lineNum: 1,
  voucher: 'JV-26-000051',
  transDate: '2026-11-01T00:00:00',
  documentDate: '2026-11-01T00:00:00',
  due: '2026-11-01T00:00:00',
  offsetLedgerDimension: 7,
  ...values,
});

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  navigate.mockReset();
  for (const api of [vendApi, dailyApi])
    vi.spyOn(api, 'offsets').mockResolvedValue([
      { id: 7, code: '11010202', name: 'Bank', accountType: 0 },
      { id: 9, code: '610100', name: 'Fuel expense', accountType: 0 },
    ]);
  vi.spyOn(currencyApi, 'list').mockResolvedValue([{ currencyCode: 'SAR', txt: 'Riyal' }] as never);
  vi.spyOn(vendorJournalApi, 'list').mockResolvedValue([
    { id: '3', accountNumber: 'V-001', name: 'Fuel supplier', currencyCode: 'SAR' },
  ]);
  vi.spyOn(vendorJournalApi, 'paymentMethods').mockResolvedValue([]);
});

describe('Vendor payment journal', () => {
  it('lists only vendor payment journals with the vendor captions', async () => {
    const list = vi.spyOn(vendApi, 'list').mockResolvedValue([header(4)]);
    vi.spyOn(vendApi, 'names').mockResolvedValue([]);
    render(<LedgerJournalListVendPaym />);
    expect(await screen.findByText('GJ-26-0041')).toBeInTheDocument();
    expect(list).toHaveBeenCalled();
    expect(screen.getAllByText('Vendor payment journal').length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: 'Enter vendor payments' })).toBeDisabled();
  });

  it('marks the vendor invoice equal to the payment debit and saves the settlement', async () => {
    vi.spyOn(vendApi, 'get').mockResolvedValue(header(4));
    vi.spyOn(vendApi, 'journalLines').mockResolvedValue([
      line(4, { ledgerDimensionName: 'V-001', amountCurDebit: 3919.73, txt: 'Fuel' }),
    ]);
    const open = vi.spyOn(vendorJournalApi, 'openTransactions').mockResolvedValue([
      {
        recId: 1,
        transRecId: 801,
        invoice: 'VINV-1',
        voucher: 'V1',
        description: '',
        currencyCode: 'SAR',
        transDate: '2026-10-01T00:00:00',
        dueDate: '2026-10-31T00:00:00',
        amountCur: -3919.73,
        amountMst: -3919.73,
      },
      {
        recId: 2,
        transRecId: 802,
        invoice: 'VINV-2',
        voucher: 'V2',
        description: '',
        currencyCode: 'SAR',
        transDate: '2026-10-01T00:00:00',
        dueDate: '2026-10-31T00:00:00',
        amountCur: -50,
        amountMst: -50,
      },
    ]);
    const update = vi.spyOn(vendApi.lines, 'update').mockImplementation(async (row) => row);
    render(<LedgerJournalTransVendPaym />);
    expect(await screen.findByText('JV-26-000051')).toBeInTheDocument();
    expect((await screen.findAllByText('Fuel supplier')).length).toBeGreaterThan(0);
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Settle transactions' })).toBeEnabled()
    );
    fireEvent.click(screen.getByRole('button', { name: 'Settle transactions' }));
    const dialog = await screen.findByRole('dialog');
    await within(dialog).findByText(/VINV-1/);
    expect(open).toHaveBeenCalledWith('V-001', expect.anything());
    const marks = within(dialog).getAllByRole('button', { name: 'Mark' });
    expect(marks[1]).toBeDisabled();
    fireEvent.click(marks[0]);
    // The dialog closes before the page behind it is reachable again.
    fireEvent.click(await screen.findByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(update).toHaveBeenCalledWith(
        expect.objectContaining({ invoice: 'VINV-1', markedInvoiceRecId: 801, settleVoucher: 2 })
      )
    );
  });
  it('adds a vendor payment line through the vendor lookup and saves it as a debit', async () => {
    vi.spyOn(vendApi, 'get').mockResolvedValue(header(4));
    vi.spyOn(vendApi, 'journalLines').mockResolvedValue([]);
    const create = vi.spyOn(vendApi.lines, 'create').mockImplementation(async (row) => ({
      ...row,
      id: '60',
      recId: 60,
    }));
    render(<LedgerJournalTransVendPaym />);
    await waitFor(() => expect(screen.getByRole('button', { name: '+ New' })).toBeEnabled());
    await waitFor(() => expect(vendorJournalApi.list).toHaveBeenCalled());
    fireEvent.click(screen.getByRole('button', { name: '+ New' }));
    fireEvent.mouseDown((await screen.findAllByRole('combobox'))[0]);
    fireEvent.click((await screen.findAllByText(/Fuel supplier/)).at(-1)!);
    fireEvent.change(screen.getByLabelText('Debit'), { target: { value: '250' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(create).toHaveBeenCalledWith(
        expect.objectContaining({
          accountType: 2,
          ledgerDimensionName: 'V-001',
          currencyCode: 'SAR',
          amountCurDebit: 250,
          amountCurCredit: 0,
          offsetLedgerDimension: 7,
        })
      )
    );
  });

  it('opens the vendor transactions of the selected line', async () => {
    vi.spyOn(vendApi, 'get').mockResolvedValue(header(4));
    vi.spyOn(vendApi, 'journalLines').mockResolvedValue([
      line(4, { ledgerDimensionName: 'V-001', amountCurDebit: 100 }),
    ]);
    render(<LedgerJournalTransVendPaym />);
    await screen.findByText('JV-26-000051');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Vendor transactions' })).toBeEnabled()
    );
    fireEvent.click(screen.getByRole('button', { name: 'Vendor transactions' }));
    expect(navigate).toHaveBeenLastCalledWith('/accounts-payable/VendTrans/V-001');
  });

  it('shows the vendor transactions with their open balance', async () => {
    const list = vi.spyOn(vendTransApi, 'list').mockResolvedValue([
      {
        id: '801',
        recId: 801,
        accountNum: 'V-001',
        transDate: '2026-10-01T00:00:00',
        voucher: 'VINV-V1',
        invoice: 'VINV-1',
        txt: 'Fuel invoice',
        currencyCode: 'SAR',
        amountCur: -3919.73,
        amountMst: -3919.73,
        settleAmountCur: -919.73,
        settleAmountMst: -919.73,
        dueDate: '2026-10-31T00:00:00',
        closed: '0001-01-01T00:00:00',
        transType: 14,
      },
    ]);
    render(<VendTrans />);
    expect(await screen.findByText('VINV-1')).toBeInTheDocument();
    expect(list).toHaveBeenCalledWith('V-001', expect.anything());
    expect(screen.getByText('-3,000.00')).toBeInTheDocument();
  });
});

describe('General journal', () => {
  it('shows ledger account names and refuses a line without a ledger account', async () => {
    vi.spyOn(dailyApi, 'get').mockResolvedValue(header(0));
    vi.spyOn(dailyApi, 'journalLines').mockResolvedValue([
      line(0, { ledgerDimension: 9, ledgerDimensionName: '610100', amountCurDebit: 100 }),
    ]);
    const create = vi.spyOn(dailyApi.lines, 'create');
    render(<LedgerJournalTransDaily />);
    expect(await screen.findByText('JV-26-000051')).toBeInTheDocument();
    expect((await screen.findAllByText('Fuel expense')).length).toBeGreaterThan(0);
    // No settlement on a general journal.
    expect(screen.getByRole('button', { name: 'Settle transactions' })).toBeDisabled();
    expect(screen.queryByRole('button', { name: 'Customer transactions' })).toBeNull();

    await waitFor(() => expect(screen.getByRole('button', { name: '+ New' })).toBeEnabled());
    fireEvent.click(screen.getByRole('button', { name: '+ New' }));
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    expect(
      await screen.findByText('Date, ledger account and currency are required.')
    ).toBeInTheDocument();
    expect(create).not.toHaveBeenCalled();
  });
  it('adds a ledger line through the ledger account lookup and saves the account combination', async () => {
    vi.spyOn(dailyApi, 'get').mockResolvedValue(header(0));
    vi.spyOn(dailyApi, 'journalLines').mockResolvedValue([]);
    const create = vi.spyOn(dailyApi.lines, 'create').mockImplementation(async (row) => ({
      ...row,
      id: '61',
      recId: 61,
    }));
    render(<LedgerJournalTransDaily />);
    await waitFor(() => expect(screen.getByRole('button', { name: '+ New' })).toBeEnabled());
    fireEvent.click(screen.getByRole('button', { name: '+ New' }));
    fireEvent.mouseDown((await screen.findAllByRole('combobox'))[0]);
    fireEvent.click((await screen.findAllByText(/Fuel expense/)).at(-1)!);
    fireEvent.change(screen.getByLabelText('Debit'), { target: { value: '75' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(create).toHaveBeenCalledWith(
        expect.objectContaining({
          accountType: 0,
          ledgerDimension: 9,
          ledgerDimensionName: '610100',
          amountCurDebit: 75,
        })
      )
    );
  });
});

describe('Journal customization', () => {
  it('lets each journal change its tabs, commands and line checks', async () => {
    vi.spyOn(dailyApi, 'get').mockResolvedValue(header(0));
    vi.spyOn(dailyApi, 'journalLines').mockResolvedValue([
      line(0, { ledgerDimension: 9, ledgerDimensionName: '610100', amountCurDebit: 100 }),
    ]);
    const update = vi.spyOn(dailyApi.lines, 'update');
    const extra = vi.fn();
    render(
      <LedgerJournalLinesPage
        kind={{
          ...GENERAL_JOURNAL,
          customize: {
            ...GENERAL_JOURNAL.customize,
            lineCommands: () => [{ id: 'allocate', label: 'Allocate', onClick: extra }],
            validateLine: (row) =>
              row.amountCurDebit > 50 ? 'Amounts over 50 need approval.' : undefined,
          },
        }}
      />
    );
    expect(await screen.findByText('JV-26-000051')).toBeInTheDocument();
    // The general journal drops the payment tabs.
    expect(screen.queryByRole('tab', { name: 'Payment' })).toBeNull();
    expect(screen.queryByRole('tab', { name: 'Remittance' })).toBeNull();
    expect(screen.getByRole('tab', { name: 'Invoice' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Allocate' }));
    expect(extra).toHaveBeenCalled();

    await waitFor(() => expect(screen.getByRole('button', { name: 'Edit' })).toBeEnabled());
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
    fireEvent.click(await screen.findByRole('button', { name: 'Save' }));
    expect(await screen.findByText('Amounts over 50 need approval.')).toBeInTheDocument();
    expect(update).not.toHaveBeenCalled();
  });
});
