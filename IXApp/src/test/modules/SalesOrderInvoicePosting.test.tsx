import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { SalesOrderInvoicePostingDialog } from '@modules/finance/accounts-receivable/sales-order/components/SalesOrderInvoicePostingDialog';
import { salesOrderInvoiceApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderInvoiceApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderInvoiceApi', () => ({
  salesOrderInvoiceApi: { post: vi.fn() },
}));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: { list: vi.fn() },
}));

type Lines = Awaited<ReturnType<typeof salesOrderLinesApi.list>>;

const lines = [
  {
    id: '101',
    lineNumber: 1,
    itemNumber: 'ITEM-A',
    description: 'Item A',
    quantity: 10,
    unit: 'ea',
    unitPrice: 5,
    lineTotal: 50,
    remainSalesFinancial: 4,
  },
  // Already invoiced in full, so the dialog must not offer it.
  {
    id: '102',
    lineNumber: 2,
    itemNumber: 'ITEM-B',
    description: 'Item B',
    quantity: 3,
    unit: 'ea',
    unitPrice: 7,
    lineTotal: 21,
    remainSalesFinancial: 0,
  },
] as unknown as Lines;

const renderDialog = (orderId: string, onPosted = vi.fn().mockResolvedValue(undefined)) => {
  render(
    <MemoryRouter>
      <AppProviders>
        <SalesOrderInvoicePostingDialog
          open
          orderId={orderId}
          salesId={`SO-${orderId}`}
          canPost
          onClose={vi.fn()}
          onPosted={onPosted}
        />
      </AppProviders>
    </MemoryRouter>
  );
};

it('offers only lines with a remaining financial quantity and posts the entered quantity', async () => {
  vi.mocked(salesOrderLinesApi.list).mockResolvedValue(lines);
  vi.mocked(salesOrderInvoiceApi.post).mockResolvedValue({
    invoiceId: 'INV-1',
  } as Awaited<ReturnType<typeof salesOrderInvoiceApi.post>>);
  renderDialog('7');

  expect(await screen.findByText('ITEM-A')).toBeInTheDocument();
  expect(screen.queryByText('ITEM-B')).not.toBeInTheDocument();

  // Defaults to the remaining financial quantity, not the ordered quantity.
  const quantity = screen.getByLabelText(/ITEM-A/);
  expect(quantity).toHaveValue(4);

  fireEvent.change(quantity, { target: { value: '3' } });
  fireEvent.click(screen.getByRole('button', { name: /^ok$/i }));

  await waitFor(() => expect(salesOrderInvoiceApi.post).toHaveBeenCalledTimes(1));
  const [orderId, request] = vi.mocked(salesOrderInvoiceApi.post).mock.calls[0];
  expect(orderId).toBe('7');
  expect(request.lines).toEqual([{ salesLineRecId: 101, qty: 3 }]);
  expect(request.requestId).toBeTruthy();
  expect(request.invoiceDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
});

it('refuses to post more than the remaining financial quantity', async () => {
  vi.mocked(salesOrderLinesApi.list).mockResolvedValue(lines);
  vi.mocked(salesOrderInvoiceApi.post).mockClear();
  renderDialog('8');

  fireEvent.change(await screen.findByLabelText(/ITEM-A/), { target: { value: '5' } });
  const ok = screen.getByRole('button', { name: /^ok$/i });
  await waitFor(() => expect(ok).toBeDisabled());
  expect(salesOrderInvoiceApi.post).not.toHaveBeenCalled();
});

it('says so when every line is already invoiced', async () => {
  vi.mocked(salesOrderLinesApi.list).mockResolvedValue([lines[1]] as unknown as Lines);
  renderDialog('9');

  expect(await screen.findByText(/no line has a remaining quantity/i)).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /^ok$/i })).toBeDisabled();
});

it('proposes only the delivered quantity, and the whole remainder when the parameter says all', async () => {
  // Ordered 10, of which 6 are delivered and 4 are still to ship.
  vi.mocked(salesOrderLinesApi.list).mockResolvedValue([
    {
      id: '201',
      lineNumber: 1,
      itemNumber: 'ITEM-C',
      description: 'Item C',
      quantity: 10,
      unit: 'ea',
      unitPrice: 5,
      lineTotal: 50,
      remainSalesFinancial: 10,
      remainSalesPhysical: 4,
    },
  ] as unknown as Lines);
  renderDialog('11');

  // Packing slip is the default parameter: invoice what was delivered, 10 - 4 = 6.
  const quantity = await screen.findByLabelText(/ITEM-C/);
  expect(quantity).toHaveValue(6);

  // The parameters section starts collapsed, as it does in D365.
  fireEvent.click(screen.getByText(/^parameters$/i));
  const parameter = await screen.findByRole('combobox', { name: /quantity/i });
  fireEvent.mouseDown(parameter);
  fireEvent.click(await screen.findByRole('option', { name: /^all$/i }));

  await waitFor(() => expect(screen.getByLabelText(/ITEM-C/)).toHaveValue(10));
});
