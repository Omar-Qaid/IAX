import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { CustReturnOrderDialog } from '@modules/finance/accounts-receivable/return-order/components/CustReturnOrderDialog';
import { custReturnApi } from '@modules/finance/accounts-receivable/return-order/api/custReturnApi';
import { custSalesLinesApi } from '@modules/finance/accounts-receivable/invoice/api/custSalesLinesApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi';
import { buildCustomerSellRibbonGroups } from '@modules/finance/accounts-receivable/customer/components/customerSellRibbon';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/return-order/api/custReturnApi', () => ({
  custReturnApi: {
    dispositionCodes: vi.fn(),
    reasonCodes: vi.fn(),
    create: vi.fn(),
    receive: vi.fn(),
    credit: vi.fn(),
  },
}));
vi.mock('@modules/finance/accounts-receivable/invoice/api/custSalesLinesApi', () => ({
  custSalesLinesApi: { load: vi.fn() },
}));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: { list: vi.fn() },
}));

const invoicedLines = {
  accountNum: 'C-001',
  customerName: 'Customer',
  lines: [
    {
      recId: 501,
      invoiceId: 'INV-1',
      itemId: 'ITEM-A',
      name: 'Item A',
      salesUnit: 'ea',
      qty: 10,
      lineNum: 1,
    },
    {
      recId: 502,
      invoiceId: 'INV-2',
      itemId: 'ITEM-B',
      name: 'Item B',
      salesUnit: 'ea',
      qty: 4,
      lineNum: 1,
    },
  ],
} as unknown as Awaited<ReturnType<typeof custSalesLinesApi.load>>;

const primeApis = () => {
  vi.mocked(custSalesLinesApi.load).mockResolvedValue(invoicedLines);
  vi.mocked(custReturnApi.dispositionCodes).mockResolvedValue([
    { dispositionCodeId: 'SCRAP', description: 'Scrap', dispositionAction: 0 },
  ]);
  vi.mocked(custReturnApi.reasonCodes).mockResolvedValue([
    { reasonCodeId: 'DAMAGED', description: 'Damaged', reasonCodeGroupId: 'G1' },
  ]);
  vi.mocked(salesOrderLinesApi.list).mockResolvedValue([
    { id: '900', quantity: -3 },
  ] as unknown as Awaited<ReturnType<typeof salesOrderLinesApi.list>>);
};

const renderDialog = (account: string) => {
  render(
    <MemoryRouter>
      <AppProviders>
        <CustReturnOrderDialog
          open
          custAccount={account}
          customerName="Customer"
          canPost
          onClose={vi.fn()}
        />
      </AppProviders>
    </MemoryRouter>
  );
};

const choose = async (name: RegExp, option: RegExp) => {
  const field = await screen.findByRole('combobox', { name });
  await waitFor(() => expect(field).not.toHaveAttribute('aria-disabled', 'true'));
  fireEvent.mouseDown(field);
  fireEvent.click(await screen.findByRole('option', { name: option }));
};

it('walks the return order, receipt and credit note in order', async () => {
  primeApis();
  vi.mocked(custReturnApi.create).mockResolvedValue({
    recId: 77,
    salesId: 'RET-1',
    returnItemNum: 'RMA-1',
  });
  vi.mocked(custReturnApi.receive).mockResolvedValue({ packingSlipId: 'PS-RET-1', qty: 3 });
  vi.mocked(custReturnApi.credit).mockResolvedValue({
    invoiceId: 'CN-1',
    creditAmount: 300,
    revenue: 300,
    tax: 0,
  });
  renderDialog('C-001');

  await choose(/^invoice$/i, /INV-1/);
  await choose(/disposition code/i, /SCRAP/);

  // Only the chosen invoice's lines can be returned.
  expect(await screen.findByText('ITEM-A')).toBeInTheDocument();
  expect(screen.queryByText('ITEM-B')).not.toBeInTheDocument();

  const create = screen.getByRole('button', { name: /create return order/i });
  expect(create).toBeDisabled();
  fireEvent.change(screen.getByLabelText(/return quantity for ITEM-A/i), {
    target: { value: '3' },
  });
  await waitFor(() => expect(create).toBeEnabled());
  fireEvent.click(create);

  await waitFor(() => expect(custReturnApi.create).toHaveBeenCalledTimes(1));
  expect(vi.mocked(custReturnApi.create).mock.calls[0][0]).toEqual({
    invoiceId: 'INV-1',
    returnReasonCodeId: undefined,
    lines: [{ custInvoiceTransRecId: 501, qty: 3, dispositionCodeId: 'SCRAP' }],
  });

  // The receipt posts against the return order's own lines, whose quantities are negative.
  const receipt = await screen.findByRole('button', { name: /post return receipt/i });
  await waitFor(() => expect(receipt).toBeEnabled());
  fireEvent.click(receipt);
  await waitFor(() => expect(custReturnApi.receive).toHaveBeenCalledTimes(1));
  const [recId, receiveRequest] = vi.mocked(custReturnApi.receive).mock.calls[0];
  expect(recId).toBe(77);
  expect(receiveRequest.lines).toEqual([{ salesLineRecId: 900, qty: 3 }]);
  expect(receiveRequest.requestId).toBeTruthy();

  const credit = await screen.findByRole('button', { name: /post credit note/i });
  fireEvent.click(credit);
  await waitFor(() => expect(custReturnApi.credit).toHaveBeenCalledTimes(1));
  expect(vi.mocked(custReturnApi.credit).mock.calls[0][0]).toBe(77);
  expect(await screen.findByText(/CN-1/)).toBeInTheDocument();
});

it('refuses to return more than the invoiced quantity', async () => {
  primeApis();
  vi.mocked(custReturnApi.create).mockClear();
  renderDialog('C-002');

  await choose(/^invoice$/i, /INV-1/);
  await choose(/disposition code/i, /SCRAP/);
  fireEvent.change(await screen.findByLabelText(/return quantity for ITEM-A/i), {
    target: { value: '11' },
  });

  const create = screen.getByRole('button', { name: /create return order/i });
  await waitFor(() => expect(create).toBeDisabled());
  expect(await screen.findByText(/cannot exceed the invoiced quantity/i)).toBeInTheDocument();
  expect(custReturnApi.create).not.toHaveBeenCalled();
});

it('needs a disposition code before the return order can be created', async () => {
  primeApis();
  vi.mocked(custReturnApi.create).mockClear();
  renderDialog('C-003');

  await choose(/^invoice$/i, /INV-1/);
  fireEvent.change(await screen.findByLabelText(/return quantity for ITEM-A/i), {
    target: { value: '2' },
  });

  const create = screen.getByRole('button', { name: /create return order/i });
  await waitFor(() => expect(create).toBeDisabled());
});

it('offers the return order action on the Sell ribbon only when a handler is supplied', () => {
  const find = (groups: ReturnType<typeof buildCustomerSellRibbonGroups>) =>
    groups.flatMap((group) => group.actions).find((action) => action.id === 'new-return-order');

  expect(find(buildCustomerSellRibbonGroups((_k, f) => f, {}))).toBeUndefined();
  const withHandler = find(buildCustomerSellRibbonGroups((_k, f) => f, { onReturnOrder: vi.fn() }));
  expect(withHandler?.onClick).toBeTypeOf('function');
});
