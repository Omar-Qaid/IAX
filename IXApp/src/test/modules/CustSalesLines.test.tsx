import React from 'react';
import { fireEvent, render, screen, cleanup } from '@testing-library/react';
import { afterEach, expect, it, vi } from 'vitest';
import { CustSalesLines } from '@modules/finance/accounts-receivable/invoice/pages/CustSalesLines';

const state = vi.hoisted(() => ({ empty: false, permitted: true, account: 'C-1', enabled: false }));
vi.mock('react-router-dom', () => ({
  useParams: () => ({ accountNum: state.account }),
  useNavigate: () => vi.fn(),
  useLocation: () => ({
    pathname: '/accounts-receivable/CustSalesLines/C-1',
    search: '',
    hash: '',
    state: null,
    key: 'test',
  }),
}));
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: () => state.permitted }),
}));
vi.mock('@core/permissions/usePermission', () => ({
  usePermission: () => ({ hasPermission: true }),
}));
vi.mock('@shared/components/documents', () => ({
  RecordAttachmentsButton: () => null,
  recordTableId: () => 1,
}));
vi.mock('@shared/components/action-pane/EnterpriseCommandUtilities', () => ({
  EnterpriseCommandUtilities: () => null,
}));
vi.mock('@shared/components/dialogs/AppRecordAuditDrawer', () => ({
  AppRecordAuditDrawer: () => null,
}));
vi.mock('@tanstack/react-query', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@tanstack/react-query')>()),
  useQuery: ({ enabled }: { enabled: boolean }) => {
    state.enabled = enabled;
    return {
      data: {
        customerName: 'Customer One',
        lines: state.empty
          ? []
          : [
              {
                recId: 1,
                itemId: 'ITEM-1',
                invoiceDate: '2026-11-01',
                customerAccount: 'C-1',
                qty: 2,
                salesPrice: 50,
                linePercent: 7.56,
                lineDisc: 0,
                lineAmount: 92.44,
                name: 'First line',
                taxGroup: 'VAT15',
                departmentValue: 'SALES',
              },
              {
                recId: 2,
                itemId: 'CREDIT-ITEM',
                invoiceDate: '2026-11-02',
                customerAccount: 'C-1',
                qty: -2,
                salesPrice: 50,
                linePercent: 0,
                lineDisc: 0,
                lineAmount: -100,
                name: 'Credit line',
                taxGroup: 'ZERO',
                departmentValue: 'RETURNS',
              },
            ],
      },
      isFetching: false,
      isError: false,
      refetch: vi.fn(),
    };
  },
}));
afterEach(() => {
  cleanup();
  state.empty = false;
  state.permitted = true;
  state.account = 'C-1';
});
it('keeps the selected posted line across all read-only detail tabs', () => {
  render(<CustSalesLines />);
  expect(screen.getByText('7.56')).toBeInTheDocument();
  expect(screen.getByText('-100.00')).toBeInTheDocument();
  fireEvent.click(screen.getByText('CREDIT-ITEM'));
  fireEvent.click(screen.getByRole('tab', { name: 'General' }));
  expect(screen.getByText('Credit line')).toBeInTheDocument();
  expect(screen.queryByText('First line')).not.toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Sales order' }));
  expect(screen.getByText('Credit line')).toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Price/discount' }));
  expect(screen.getByText('-100.00')).toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Sales tax' }));
  expect(screen.getByText('ZERO')).toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Financial dimensions' }));
  expect(screen.getByText('RETURNS')).toBeInTheDocument();
  expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  expect(
    screen.queryByRole('button', { name: /^(New|Edit|Delete|Save|Post)$/i })
  ).not.toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Overview' }));
  expect(screen.getByText('ITEM-1')).toBeInTheDocument();
});
it('handles empty lines without stale selected-record details', () => {
  state.empty = true;
  render(<CustSalesLines />);
  fireEvent.click(screen.getByRole('tab', { name: 'General' }));
  expect(screen.getByText('Select a line on the Overview tab.')).toBeInTheDocument();
});
it('does not query for missing accounts or denied permissions', () => {
  state.permitted = false;
  render(<CustSalesLines />);
  expect(state.enabled).toBe(false);
  expect(screen.queryByRole('tab')).not.toBeInTheDocument();
  cleanup();
  state.permitted = true;
  state.account = '';
  render(<CustSalesLines />);
  expect(state.enabled).toBe(false);
});
