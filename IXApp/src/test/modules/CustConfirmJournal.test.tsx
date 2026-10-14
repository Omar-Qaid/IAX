import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { vi, describe, it, expect } from 'vitest';
import { CustConfirmJournal } from '../../modules/finance/accounts-receivable/pages/CustConfirmJournal';

const journalState = vi.hoisted(() => ({ empty: false }));

vi.mock('react-router-dom', () => ({
  useParams: () => ({ salesOrderId: '42' }),
  useNavigate: () => vi.fn(),
}));
vi.mock('@core/permissions/usePermission', () => ({ usePermission: () => ({ hasPermission: true }) }));
vi.mock('@shared/components/documents', () => ({ RecordAttachmentsButton: () => null, recordTableId: () => 1 }));
vi.mock('@shared/components/action-pane/EnterpriseCommandUtilities', () => ({ EnterpriseCommandUtilities: () => null }));
vi.mock('@shared/components/dialogs/AppRecordAuditDrawer', () => ({ AppRecordAuditDrawer: () => null }));
vi.mock('@tanstack/react-query', async (importOriginal) => ({
  ...await importOriginal<typeof import('@tanstack/react-query')>(),
  useQuery: ({ queryKey }: { queryKey: string[] }) => {
    if (queryKey[0] === 'accounts-receivable') return { data: [{ id: '42', salesId: 'SO-42', customerName: 'Customer', currencyCode: 'SAR' }] };
    if (queryKey[0] === 'sales-order-confirmations') return { data: journalState.empty ? [] : [
      { id: '8', salesId: 'SO-42', confirmId: 'CONF-8', confirmDate: '2026-10-13', currencyCode: 'SAR', confirmAmount: 700 },
      { id: '10', salesId: 'SO-42', confirmId: 'CONF-10', confirmDate: '2026-10-14', currencyCode: 'SAR', confirmAmount: 900 },
    ] };
    const second = queryKey[2] === '10';
    return { data: { header: { salesId: 'SO-42' }, lines: [{ id: second ? '11' : '9', itemId: second ? 'ITEM-2' : 'ITEM-1', name: 'Panel', qty: 2, pdsCwQty: 0, salesCategory: 0, inventLocationId: 'WH-1', wmsLocationId: 'BIN-1', salesPrice: 350, discPercent: 0, linePercent: 0, lineDisc: 0, lineAmount: 700 }] } };
  },
}));

describe('CustConfirmJournal', () => {
  it('shows read-only overview and historical line snapshot', () => {
    render(<CustConfirmJournal />);
    expect(screen.getByText('CONF-8')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
    expect(screen.getByText('ITEM-1')).toBeInTheDocument();
    expect(screen.getByText('WH-1')).toBeInTheDocument();
    expect(screen.getByText('BIN-1')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /save|delete|post/i })).not.toBeInTheDocument();
  });

  it('shows lines for the selected confirmation and returns to overview', () => {
    render(<CustConfirmJournal />);
    fireEvent.click(screen.getByText('CONF-10'));
    fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
    expect(screen.getByText('ITEM-2')).toBeInTheDocument();
    expect(screen.queryByText('ITEM-1')).not.toBeInTheDocument();
    fireEvent.click(screen.getByRole('tab', { name: 'Overview' }));
    expect(screen.getByText('CONF-8')).toBeInTheDocument();
    expect(screen.getByText('CONF-10')).toBeInTheDocument();
  });

  it('disables Lines when the order has no posted confirmations', () => {
    journalState.empty = true;
    try {
      render(<CustConfirmJournal />);
      expect(screen.getByRole('tab', { name: 'Lines' })).toBeDisabled();
      expect(screen.queryByText('CONF-8')).not.toBeInTheDocument();
    } finally {
      journalState.empty = false;
    }
  });
});
