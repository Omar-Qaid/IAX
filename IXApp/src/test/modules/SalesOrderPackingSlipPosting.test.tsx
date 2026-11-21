import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { SalesOrderPackingSlipPostingDialog } from '@modules/finance/accounts-receivable/sales-order/components/SalesOrderPackingSlipPostingDialog';
import { salesOrderPackingSlipApi as api } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi';
vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi', () => ({
  salesOrderPackingSlipApi: { postingPreview: vi.fn(), post: vi.fn() },
}));
it('keeps the reference sections and footer while posting the entered partial quantity', async () => {
  vi.mocked(api.postingPreview).mockResolvedValue({
    header: {
      id: '1',
      salesId: 'SO-001',
      customerName: 'Customer',
      currencyCode: 'SAR',
      siteId: 'JED',
    },
    lines: [
      {
        id: '11',
        lineNumber: 1,
        itemId: 'ITEM-001',
        name: 'Stocked item',
        salesCategory: '',
        productNumber: 'ITEM-001',
        inventSiteId: 'JED',
        inventLocationId: 'MAIN',
        wmsLocationId: '',
        catchWeightQuantity: 0,
        quantity: 4,
        netAmount: 100,
      },
    ],
  });
  vi.mocked(api.post).mockResolvedValue({ id: '21' } as Awaited<ReturnType<typeof api.post>>);
  const closed = vi.fn();
  const posted = vi.fn().mockResolvedValue(undefined);
  render(
    <MemoryRouter>
      <AppProviders>
        <SalesOrderPackingSlipPostingDialog
          open
          orderId="1"
          salesId="SO-001"
          canPost
          onClose={closed}
          onPosted={posted}
        />
      </AppProviders>
    </MemoryRouter>
  );
  const quantity = await screen.findByRole('spinbutton', { name: /ITEM-001/ });
  expect(screen.getByRole('dialog', { name: 'Packing slip posting' })).toBeInTheDocument();
  for (const name of ['Parameters', 'Setup', 'Line details', 'Sales orders'])
    expect(screen.getByRole('button', { name: new RegExp(`^${name}`) })).toHaveAttribute(
      'aria-expanded',
      'false'
    );
  expect(screen.getByRole('button', { name: 'Overview' })).toHaveAttribute('aria-expanded', 'true');
  expect(screen.getByRole('button', { name: 'Lines' })).toHaveAttribute('aria-expanded', 'true');
  fireEvent.change(quantity, { target: { value: '2' } });
  fireEvent.click(screen.getByRole('button', { name: /^OK$/ }));
  await waitFor(() =>
    expect(api.post).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({
        quantityMode: 'all',
        posting: true,
        lines: [{ salesLineId: '11', quantity: 2 }],
      })
    )
  );
  await waitFor(() => expect(posted).toHaveBeenCalledOnce());
  expect(closed).toHaveBeenCalledOnce();
});
