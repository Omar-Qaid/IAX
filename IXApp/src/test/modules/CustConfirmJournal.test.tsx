import React from 'react';
import { fireEvent, render, screen } from '@testing-library/react';
import { vi, describe, it, expect } from 'vitest';
import { CustConfirmJournal } from '../../modules/finance/accounts-receivable/pages/CustConfirmJournal';

vi.mock('react-router-dom', () => ({
  useParams: () => ({ salesOrderId: '42' }),
  useNavigate: () => vi.fn(),
}));
vi.mock('@tanstack/react-query', () => ({
  useQuery: ({ queryKey }: { queryKey: string[] }) => {
    if (queryKey[0] === 'accounts-receivable') return { data: [{ id: '42', salesId: 'SO-42', customerName: 'Customer', currencyCode: 'SAR' }] };
    if (queryKey[0] === 'sales-order-confirmations') return { data: [{ id: '8', salesId: 'SO-42', confirmId: 'CONF-8', confirmDate: '2026-10-13', currencyCode: 'SAR', confirmAmount: 700 }] };
    return { data: { header: { salesId: 'SO-42' }, lines: [{ id: '9', itemId: 'ITEM-1', name: 'Panel', qty: 2, pdsCwQty: 0, salesCategory: 0, inventLocationId: 'WH-1', wmsLocationId: 'BIN-1', salesPrice: 350, discPercent: 0, linePercent: 0, lineDisc: 0, lineAmount: 700 }] } };
  },
}));

describe('CustConfirmJournal', () => {
  it('shows read-only overview and historical line snapshot', () => {
    render(<CustConfirmJournal />);
    expect(screen.getByRole('table', { name: 'Sales order confirmation overview' })).toHaveTextContent('CONF-8');
    fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
    const lines = screen.getByRole('table', { name: 'Sales order confirmation lines' });
    expect(lines).toHaveTextContent('ITEM-1');
    expect(lines).toHaveTextContent('WH-1');
    expect(lines).toHaveTextContent('BIN-1');
    expect(screen.queryByRole('button', { name: /save|delete|post/i })).not.toBeInTheDocument();
  });
});
