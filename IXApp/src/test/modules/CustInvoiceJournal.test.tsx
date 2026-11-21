import React from 'react';
import { cleanup, render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, expect, it, vi } from 'vitest';
import { CustInvoiceJournal } from '@modules/finance/accounts-receivable/invoice/pages/CustInvoiceJournal';

const renderJournal = (route = '/accounts-receivable/CustInvoiceJournal') =>
  render(
    <MemoryRouter initialEntries={[route]}>
      <CustInvoiceJournal />
    </MemoryRouter>
  );

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
vi.mock('@core/company/useCompanyStore', () => ({ useCompanyStore: () => 'USMF' }));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string) => key, isRtl: false }),
}));
vi.mock('@tanstack/react-query', () => ({
  useQuery: (options: unknown) => {
    state.queries.push(options);
    return {
      data: [{ recId: 42, invoiceId: 'INV-42', invoiceAmount: -291.24 }],
      isFetching: false,
      isError: false,
      refetch: vi.fn(),
    };
  },
}));
afterEach(() => {
  cleanup();
  state.permitted = true;
  state.props = null;
  state.queries.length = 0;
});
it('keeps posted invoices read-only and preserves credit amounts and record identity', () => {
  renderJournal();
  expect(state.props.enterpriseConfig.readOnly).toBe(true);
  expect(state.props.enterpriseConfig.crud).toBeUndefined();
  expect(state.props.recordTableName).toBe('CustInvoiceJour');
  const row = state.props.dataSource.rows[0];
  expect(row.id).toBe('42');
  expect(state.props.enterpriseConfig.getAuditRecordId(row)).toBe(42);
  const column = state.props.columns.find((item: any) => item.field === 'invoiceAmount');
  expect(column.renderCell({ row })).toBe('-291.24');
  expect(state.queries[0].queryKey).toEqual(['cust-invoice-journal', 'USMF']);
});
it('does not fetch or render journal data without view permission', () => {
  state.permitted = false;
  renderJournal();
  expect(state.queries.every((query: any) => query.enabled === false)).toBe(true);
  expect(state.props).toBeNull();
});

it('shows only the customer’s invoices when opened from the customer Invoice tab', () => {
  renderJournal('/accounts-receivable/CustInvoiceJournal?customerAccount=CUS-00002');
  expect(state.props.dataSource.rows).toEqual([]);
  expect(state.props.enterpriseConfig.contextLabel).toBe('custInvoiceJournal.title | CUS-00002');
});
