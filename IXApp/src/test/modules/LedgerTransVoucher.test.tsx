import React from 'react';
import { cleanup, render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, expect, it, vi } from 'vitest';
import { LedgerTransVoucher } from '@modules/finance/general-ledger/voucher/pages/LedgerTransVoucher';

const renderPage = (route = '/general-ledger/LedgerTransVoucher') =>
  render(
    <MemoryRouter initialEntries={[route]}>
      <LedgerTransVoucher />
    </MemoryRouter>
  );

const row = {
  recId: 7,
  journalNumber: 'HBMC-277853',
  voucher: 'CINV-VC-26-00006336',
  transDate: '2026-09-21T00:00:00',
  yearClosed: false,
  ledgerAccount: '21062301-SAL-RIY-WRH',
  accountName: 'VAT Payable - Sales',
  description: '',
  currencyCode: 'SAR',
  amountCur: -16.8,
  amountMst: -16.8,
  postingType: 'SalesTax',
};
const state = vi.hoisted(() => ({ permitted: true, props: null as any, queries: [] as any[] }));
vi.mock('@patterns/simple-list/SimpleListPage', () => ({
  SimpleListPage: (props: unknown) => {
    state.props = props;
    return null;
  },
}));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: () => state.permitted }),
}));
vi.mock('@core/company/useCompanyStore', () => ({ useCompanyStore: () => 'HBMC' }));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string) => key, isRtl: false }),
}));
vi.mock('@tanstack/react-query', () => ({
  useQuery: (options: unknown) => {
    state.queries.push(options);
    return { data: [row], isFetching: false, isError: false, refetch: vi.fn() };
  },
}));
afterEach(() => {
  cleanup();
  state.permitted = true;
  state.props = null;
  state.queries.length = 0;
});

it('reads posted ledger lines of one voucher as a read-only inquiry', () => {
  renderPage('/general-ledger/LedgerTransVoucher?voucher=CINV-VC-26-00006336');
  expect(state.props.enterpriseConfig.readOnly).toBe(true);
  expect(state.props.enterpriseConfig.crud).toBeUndefined();
  expect(state.props.recordTableName).toBe('GeneralJournalAccountEntry');
  expect(state.queries[0].queryKey).toEqual([
    'ledger-trans-voucher',
    'HBMC',
    'CINV-VC-26-00006336',
  ]);
  const listed = state.props.dataSource.rows[0];
  expect(listed.id).toBe('7');
  expect(state.props.enterpriseConfig.getAuditRecordId(listed)).toBe(7);
});

it('shows the columns of the voucher transactions form, in its order', () => {
  renderPage();
  expect(state.props.columns.map((column: any) => column.field)).toEqual([
    'journalNumber',
    'voucher',
    'transDate',
    'yearClosed',
    'ledgerAccount',
    'accountName',
    'description',
    'currencyCode',
    'amountCur',
    'amountMst',
    'postingType',
  ]);
  const column = (field: string) =>
    state.props.columns.find((candidate: any) => candidate.field === field);
  expect(column('transDate').valueGetter({ row })).toBe('2026-09-21');
  expect(column('amountCur').renderCell({ row })).toBe('-16.80');
  // An open year leaves the cell empty; the check mark only marks a closed one.
  expect(column('yearClosed').renderCell({ row })).toBeNull();
  expect(column('yearClosed').renderCell({ row: { ...row, yearClosed: true } })).not.toBeNull();
});

it('names the record the voucher came from in the breadcrumb', () => {
  renderPage(
    '/general-ledger/LedgerTransVoucher?voucher=CINV-VC-26-00006336&reference=INV-26-00006334%20%3A%20C-0000002'
  );
  expect(state.props.enterpriseConfig.contextLabel).toBe(
    'ledgerTransVoucher.title | INV-26-00006334 : C-0000002'
  );
});

it('falls back to the voucher itself when no record is named', () => {
  renderPage('/general-ledger/LedgerTransVoucher?voucher=CINV-VC-26-00006336');
  expect(state.props.enterpriseConfig.contextLabel).toBe(
    'ledgerTransVoucher.title | CINV-VC-26-00006336'
  );
});

it('does not fetch or render voucher data without view permission', () => {
  state.permitted = false;
  renderPage();
  expect(state.queries.every((query: any) => query.enabled === false)).toBe(true);
  expect(state.props).toBeNull();
});
