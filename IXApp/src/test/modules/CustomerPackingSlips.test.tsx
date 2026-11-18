import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { CustPackingSlipJournal } from '@modules/finance/accounts-receivable/sales-order/pages/CustPackingSlipJournal';
import { salesOrderListApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderListApi';
import { salesOrderPackingSlipApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi';
vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderListApi', () => ({
  salesOrderListApi: { list: vi.fn() },
}));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi', () => ({
  salesOrderPackingSlipApi: { list: vi.fn(), get: vi.fn() },
}));
it('loads only the customer orders and opens lines using the selected packing slip owner order', async () => {
  vi.mocked(salesOrderListApi.list).mockResolvedValue([
    { id: '1', salesId: 'SO1', customerAccount: 'C-001', customerName: 'Customer' },
    { id: '2', salesId: 'SO2', customerAccount: 'C-001', customerName: 'Customer' },
    { id: '3', salesId: 'OTHER', customerAccount: 'C-002', customerName: 'Other' },
  ] as Awaited<ReturnType<typeof salesOrderListApi.list>>);
  vi.mocked(salesOrderPackingSlipApi.list).mockImplementation(
    async (id) =>
      [
        {
          id: id === '1' ? '11' : '22',
          salesId: `SO${id}`,
          packingSlipId: `PS-${id}`,
          deliveryDate: '',
          invoiceDueDate: '',
        },
      ] as Awaited<ReturnType<typeof salesOrderPackingSlipApi.list>>
  );
  vi.mocked(salesOrderPackingSlipApi.get).mockResolvedValue({
    lines: [{ id: '1', itemId: 'ITEM-B', quantity: 5 }],
  } as Awaited<ReturnType<typeof salesOrderPackingSlipApi.get>>);
  render(
    <MemoryRouter initialEntries={['/customer-packing-slips/C-001']}>
      <AppProviders>
        <Routes>
          <Route path="/customer-packing-slips/:accountNum" element={<CustPackingSlipJournal />} />
        </Routes>
      </AppProviders>
    </MemoryRouter>
  );
  fireEvent.click(await screen.findByText('PS-2'));
  fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
  expect(await screen.findByText('ITEM-B')).toBeInTheDocument();
  await waitFor(() =>
    expect(salesOrderPackingSlipApi.get).toHaveBeenCalledWith('2', '22', expect.anything())
  );
  expect(vi.mocked(salesOrderPackingSlipApi.list).mock.calls.map((call) => call[0])).toEqual([
    '1',
    '2',
  ]);
  expect(screen.queryByText('OTHER')).not.toBeInTheDocument();
});
