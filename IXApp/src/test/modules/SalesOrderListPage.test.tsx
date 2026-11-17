import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { apiClient } from '@core/api/apiClient';
import { AppProviders } from '@app/providers/AppProviders';
import '@app/configuration/recordPreviews';
import { SalesOrderListPage } from '@modules/finance/accounts-receivable/pages/SalesOrderListPage';

const order = (id: string, salesId: string, customerAccount: string, customerName: string) => ({
  id,
  salesId,
  customerAccount,
  customerName,
  invoiceAccount: customerAccount,
  customerGroup: 'WHOLESALE',
  currencyCode: 'SAR',
  deliveryDate: '2026-10-28',
  orderTotal: 150,
  salesStatus: 'Backorder',
  documentStatus: 'None',
});

vi.mock('@modules/finance/accounts-receivable/api/salesOrderListApi', () => ({
  salesOrderListApi: {
    list: vi.fn(async () => [
      order('1', 'SO-2026-000001', 'CUST-100', 'Consulting Customer 100'),
      order('2', 'SO-2026-000002', 'CUST-200', 'Retail Customer 200'),
    ]),
  },
}));

afterEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
});

const openList = (url = '/accounts-receivable/sales-orders') =>
  render(
    <AppProviders>
      <MemoryRouter initialEntries={[url]}>
        <Routes>
          <Route path="/accounts-receivable/sales-orders" element={<SalesOrderListPage />} />
          <Route path="/accounts-receivable/sales-orders/:id" element={<div>Order details page</div>} />
        </Routes>
      </MemoryRouter>
    </AppProviders>
  );

it('lists sales orders with their customer, group, currency and status', async () => {
  openList();
  expect(await screen.findByText('SO-2026-000001')).toBeInTheDocument();
  expect(screen.getByText('SO-2026-000002')).toBeInTheDocument();
  expect(screen.getByText('Consulting Customer 100')).toBeInTheDocument();
  expect(screen.getAllByText('WHOLESALE').length).toBeGreaterThan(0);
  expect(screen.getAllByText('SAR').length).toBeGreaterThan(0);
});

it('shows only the customer orders when opened from a customer', async () => {
  openList('/accounts-receivable/sales-orders?customerAccount=CUST-200');
  expect(await screen.findByText('SO-2026-000002')).toBeInTheDocument();
  expect(screen.queryByText('SO-2026-000001')).toBeNull();
});

it('previews the customer group master record when hovering the grid cell', async () => {
  vi.spyOn(apiClient, 'get').mockResolvedValue({
    data: { success: true, data: [{ custGroupId: 'WHOLESALE', name: 'Wholesale customers' }] },
  } as never);
  openList();
  await screen.findByText('SO-2026-000001');
  const cell = screen.getAllByText('WHOLESALE')[0];
  fireEvent.mouseEnter(cell.closest('span') ?? cell);
  const card = await screen.findByRole('tooltip', {}, { timeout: 3000 });
  expect(await within(card).findByText('Wholesale customers')).toBeInTheDocument();
});
