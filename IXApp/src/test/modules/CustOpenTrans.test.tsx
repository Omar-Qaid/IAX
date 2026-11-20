import React from 'react';
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, expect, it, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { CustOpenTrans } from '@modules/finance/accounts-receivable/settlement/components/CustOpenTrans';
import {
  custOpenTransApi,
  type CustOpenTransData,
  type CustOpenTransRow,
} from '@modules/finance/accounts-receivable/settlement/api/custOpenTransApi';
import { buildCustomerCollectRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerCollectRibbon';

const permissions = vi.hoisted(() => ({ view: true, post: true }));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({
    hasPermission: (permission: string) =>
      permission.endsWith('.Post') ? permissions.post : permissions.view,
  }),
}));
vi.mock('@core/company/useCompanyStore', () => ({ useCompanyStore: () => 'dat' }));
vi.mock('@modules/finance/accounts-receivable/settlement/api/custOpenTransApi', () => ({
  custOpenTransApi: { load: vi.fn(), post: vi.fn() },
}));
vi.mock('@shared/components/data-grid/DataGrid', () => ({
  DataGrid: ({
    rows,
    columns,
    onRowClick,
  }: {
    rows: (CustOpenTransRow & { id: string })[];
    columns: ColumnDef<CustOpenTransRow>[];
    onRowClick: (row: CustOpenTransRow) => void;
  }) => (
    <div>
      {rows.map((row, rowIndex) => (
        <div key={row.recId} onClick={() => onRowClick(row)}>
          {columns.map((column) => (
            <span key={String(column.field)}>
              {column.renderCell
                ? column.renderCell({ row, rowIndex, value: undefined })
                : column.valueGetter
                  ? String(column.valueGetter({ row }))
                  : String(row[column.field as keyof CustOpenTransRow] ?? '')}
            </span>
          ))}
        </div>
      ))}
    </div>
  ),
}));
const row = (transRecId: number, amount: number, date: string): CustOpenTransRow => ({
  recId: transRecId + 1000,
  transRecId,
  invoice: amount > 0 ? 'INV-1' : '',
  voucher: amount > 0 ? 'V-1' : 'PAY-1',
  description: 'Customer transaction',
  currencyCode: 'SAR',
  paymentMode: 'BANK',
  paymentReference: '',
  paymentTerms: 'NET30',
  cashDiscCode: '',
  worker: 'W-1',
  department: 'SALES',
  costCenter: 'RIY',
  project: '',
  businessUnit: '',
  transDate: date,
  dueDate: date,
  cashDiscDate: '',
  lastSettleDate: '',
  amountCur: amount,
  amountMst: amount,
  originalAmountCur: amount,
  originalAmountMst: amount,
  settledAmountCur: 0,
  settledAmountMst: 0,
  possibleCashDisc: 0,
  cashDiscTaken: 0,
  useCashDisc: 0,
  prepayment: false,
});
const data = (): CustOpenTransData => ({
  accountNum: 'C-1',
  customerName: 'Customer One',
  dataAreaId: 'dat',
  customerCurrency: 'SAR',
  accountingCurrency: 'SAR',
  customerBalanceMst: 60,
  reconciled: true,
  transactions: [row(11, 100, '2026-11-01'), row(22, -40, '2026-11-02')],
});
let client: QueryClient;
const close = vi.fn();
const open = () => {
  client = new QueryClient({ defaultOptions: { queries: { retry: false, gcTime: 0 } } });
  return render(
    <QueryClientProvider client={client}>
      <CustOpenTrans open accountNum="C-1" customerName="Customer One" onClose={close} />
    </QueryClientProvider>
  );
};
beforeEach(() => {
  vi.clearAllMocks();
  permissions.view = true;
  permissions.post = true;
  vi.mocked(custOpenTransApi.load).mockResolvedValue(data());
});
afterEach(() => {
  cleanup();
  client?.clear();
});
async function markPair() {
  fireEvent.click(await screen.findByRole('checkbox', { name: 'Mark INV-1' }));
  fireEvent.click(screen.getByRole('checkbox', { name: 'Mark PAY-1' }));
}
it('posts transaction IDs with the latest marked date, blocks duplicates and refreshes balances', async () => {
  let finish!: (result: {
    settlementVoucher: string;
    custAccount: string;
    currency: string;
    settled: number;
  }) => void;
  vi.mocked(custOpenTransApi.post).mockReturnValue(
    new Promise((resolve) => {
      finish = resolve;
    })
  );
  open();
  expect(screen.getByRole('button', { name: 'Post' })).toBeDisabled();
  await markPair();
  expect(screen.getByRole('button', { name: 'Post' })).toBeEnabled();
  expect(screen.getAllByLabelText('Settlement balance')[0]).toHaveValue('60.00');
  fireEvent.click(screen.getByRole('button', { name: 'Post' }));
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeDisabled();
  fireEvent.click(screen.getByRole('button', { name: 'Post' }));
  expect(custOpenTransApi.post).toHaveBeenCalledExactlyOnceWith('C-1', '2026-11-02', [11, 22]);
  vi.mocked(custOpenTransApi.load).mockResolvedValue({
    ...data(),
    transactions: [row(11, 60, '2026-11-01')],
  });
  await act(async () =>
    finish({ settlementVoucher: 'SET-1', custAccount: 'C-1', currency: 'SAR', settled: 40 })
  );
  expect(await screen.findByText('Settlement SET-1 posted.')).toBeInTheDocument();
  await waitFor(() =>
    expect(screen.queryByRole('checkbox', { name: 'Mark PAY-1' })).not.toBeInTheDocument()
  );
  expect(screen.getByRole('button', { name: 'Post' })).toBeDisabled();
});
it('rejects mixed currencies without netting their transaction amounts', async () => {
  const source = data();
  source.transactions[1].currencyCode = 'USD';
  vi.mocked(custOpenTransApi.load).mockResolvedValue(source);
  open();
  await markPair();
  expect(screen.getByRole('button', { name: 'Post' })).toBeDisabled();
  expect(screen.getAllByLabelText('Settlement balance')[1]).toHaveValue('—');
  expect(custOpenTransApi.post).not.toHaveBeenCalled();
});
it('shows errors and refreshes after a rejected settlement', async () => {
  vi.mocked(custOpenTransApi.post).mockRejectedValue(new Error('Transaction is no longer open.'));
  open();
  await markPair();
  fireEvent.click(screen.getByRole('button', { name: 'Post' }));
  expect(await screen.findByText('Transaction is no longer open.')).toBeInTheDocument();
  await waitFor(() => expect(custOpenTransApi.load).toHaveBeenCalledTimes(2));
  expect(screen.getByRole('button', { name: 'Post' })).toBeDisabled();
});
it('keeps marked rows across detail tabs and supports show marked and unmark all', async () => {
  open();
  fireEvent.click(await screen.findByRole('checkbox', { name: 'Mark INV-1' }));
  fireEvent.click(screen.getByRole('tab', { name: 'Financial dimensions' }));
  expect(screen.getByLabelText('Worker ID')).toHaveValue('W-1');
  fireEvent.click(screen.getByRole('tab', { name: 'Overview' }));
  expect(screen.getByRole('checkbox', { name: 'Mark INV-1' })).toBeChecked();
  fireEvent.click(screen.getByRole('button', { name: 'Unmark all' }));
  expect(screen.getByRole('checkbox', { name: 'Mark INV-1' })).not.toBeChecked();
});
it('requires payment view and post permissions and keeps unsupported actions disabled', async () => {
  permissions.post = false;
  open();
  expect(await screen.findByRole('checkbox', { name: 'Mark INV-1' })).toBeDisabled();
  expect(screen.getByRole('button', { name: 'Settle remainder' })).toBeDisabled();
  cleanup();
  client.clear();
  vi.mocked(custOpenTransApi.load).mockClear();
  permissions.view = false;
  open();
  expect(custOpenTransApi.load).not.toHaveBeenCalled();
  expect(screen.getByRole('button', { name: 'Post' })).toBeDisabled();
});
it('enables only the implemented Collect settlement action', () => {
  const callback = vi.fn();
  const groups = buildCustomerCollectRibbonGroups((_, fallback) => fallback, {
    onSettleTransactions: callback,
  });
  const actions = groups.flatMap((group) => group.actions);
  const settle = actions.find((action) => action.id === 'settleTransactions')!;
  expect(settle.disabled).toBe(false);
  settle.onClick!();
  expect(callback).toHaveBeenCalledOnce();
  expect(
    actions
      .filter((action) => action.id !== 'settleTransactions')
      .every((action) => action.disabled)
  ).toBe(true);
});

it('uses the last settlement date when it is later than the marked transaction dates', async () => {
  const source = data();
  source.transactions[0].lastSettleDate = '2026-11-10';
  vi.mocked(custOpenTransApi.load).mockResolvedValue(source);
  open();
  await markPair();
  expect(screen.getByLabelText('Date')).toHaveValue('2026-11-10');
  expect(screen.getByRole('button', { name: 'Post' })).toBeEnabled();
});
