import { SalesOrderDetailsPageV2 } from '@modules/finance/accounts-receivable/salesorderV2/pages/SalesOrderDetailsPageV2';
import React from 'react';
import { beforeEach, expect, it, vi } from 'vitest';
import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { salesOrderQueryKeys } from '@modules/finance/accounts-receivable/queries/accountsReceivableQueryKeys';
import { AppProviders } from '@app/providers/AppProviders';
import { salesOrderListApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderListApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi';
import { salesOrderConfirmationApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderConfirmationApi';
import { salesOrderPackingSlipApi } from '@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi';
import { SalesOrderDetailsPage } from '@modules/finance/accounts-receivable/sales-order/pages/SalesOrderDetailsPage';

vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderListApi', () => ({
  salesOrderListApi: {
    cancel: vi.fn().mockResolvedValue(undefined),
    updateHeader: vi.fn().mockResolvedValue(undefined),
    // One order by ID, from the same fixture the list returns.
    get: vi.fn(async (id: string) => (await mockOrders()).find((order) => order.id === id) ?? null),
    list: vi.fn(async () => mockOrders()),
  },
}));

async function mockOrders() {
  return [
    {
      id: '1',
      recId: 1,
      salesId: 'SO-LIVE-001',
      customerAccount: 'C-001',
      customerName: 'Live customer',
      invoiceAccount: 'C-002',
      currencyCode: 'SAR',
      salesStatus: 'Backorder',
      orderDate: '2026-09-08',
      deliveryDate: '2026-09-09',
      paymentTerms: 'Net 30',
      deliveryMode: 'Road',
      customerReference: 'PO-001',
      orderTotal: 250,
    },
  ];
}

vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: {
    list: vi.fn().mockResolvedValue([]),
    totals: vi.fn().mockResolvedValue({
      subtotal: 250,
      totalDiscount: 0,
      salesTax: 0,
      totalCharges: 0,
      invoiceAmount: 250,
      quantity: 0,
      costValue: 0,
    }),
    items: vi.fn().mockResolvedValue({
      data: [{ itemNumber: 'ITEM-1', name: 'Test item' }],
      pageNumber: 1,
      totalPages: 1,
      totalRecords: 1,
    }),
    units: vi.fn().mockResolvedValue({
      data: [{ symbol: 'Pcs' }, { symbol: 'BOX' }],
      pageNumber: 1,
      totalPages: 1,
      totalRecords: 2,
    }),
    update: vi.fn().mockImplementation(async (_id, line) => line),
    remove: vi.fn().mockResolvedValue({ deleted: true }),
    add: vi.fn().mockResolvedValue({ id: '10', itemNumber: 'ITEM-1', quantity: 1 }),
  },
}));

vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderConfirmationApi', () => ({
  salesOrderConfirmationApi: {
    preview: vi.fn().mockResolvedValue({
      header: { currencyCode: 'SAR', qty: 1, salesBalance: 100, sumTax: 15, confirmAmount: 115 },
      lines: [
        {
          id: '1',
          lineNum: 1,
          itemId: 'ITEM-1',
          name: 'Test item',
          qty: 1,
          inventLocationId: 'MAIN',
          wmsLocationId: '',
          salesPrice: 100,
          lineAmount: 100,
        },
      ],
    }),
    post: vi.fn().mockResolvedValue({ id: '10' }),
  },
}));

vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderPackingSlipApi', () => ({
  salesOrderPackingSlipApi: {
    postingPreview: vi.fn().mockImplementation(async () => ({
      header: {
        id: '1',
        salesId: 'SO-LIVE-001',
        customerName: 'Live customer',
        currencyCode: 'SAR',
        siteId: 'MAIN',
      },
      lines: [
        {
          id: '10',
          lineNumber: 1,
          itemId: 'ITEM-1',
          name: 'Test item',
          quantity: 6,
          netAmount: 60,
        },
      ],
    })),
    post: vi.fn().mockResolvedValue({ id: '100' }),
  },
}));

beforeEach(() => {
  vi.clearAllMocks();
  globalThis.localStorage.setItem('ixapp.sales-order.list-visible', 'true');
});

function openOrder(id: string, listVisible?: boolean) {
  if (listVisible !== undefined)
    globalThis.localStorage.setItem('ixapp.sales-order.list-visible', String(listVisible));
  return render(
    <MemoryRouter initialEntries={[`/accounts-receivable/sales-orders/${id}`]}>
      <AppProviders>
        <Routes>
          <Route
            path="/accounts-receivable/sales-orders/:salesOrderId"
            element={<SalesOrderDetailsPage />}
          />
          <Route
            path="/accounts-receivable/sales-orders/:salesOrderId/confirmations"
            element={<div>CustConfirmJour page</div>}
          />
        </Routes>
      </AppProviders>
    </MemoryRouter>
  );
}

it('loads a numeric route ID directly from the API', async () => {
  openOrder('1');
  expect(await screen.findByText('SO-LIVE-001 : Live customer')).toBeDefined();
  expect(screen.queryByText('No sales orders are available.')).toBeNull();
  await waitFor(() =>
    expect(salesOrderLinesApi.totals).toHaveBeenCalledWith('1', expect.anything())
  );
});

it('opens CustConfirmJour from Sell > Sales order confirmations', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Sales order confirmations' }));
  expect(await screen.findByText('CustConfirmJour page')).toBeInTheDocument();
});

it('guards order cancellation while header edits are unsaved or cancellation is in flight', async () => {
  const confirmation = vi.spyOn(window, 'confirm').mockReturnValue(true);
  let rejectCancellation!: (reason: Error) => void;
  vi.mocked(salesOrderListApi.cancel).mockImplementationOnce(
    () =>
      new Promise((_, reject) => {
        rejectCancellation = reject;
      })
  );
  try {
    openOrder('1');
    await screen.findByText('SO-LIVE-001 : Live customer');

    // Order cancellation lives in the Maintain group of the Sales order ribbon. Unsaved header
    // edits disable that ribbon, so the action cannot be reached at all while editing.
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
    expect(screen.getByRole('button', { name: 'Sales order' })).toBeDisabled();
    expect(salesOrderListApi.cancel).not.toHaveBeenCalled();
    // Discard the header edits; this Cancel is the edit-mode one beside Save.
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    fireEvent.click(screen.getByRole('button', { name: 'Sales order' }));
    const cancelOrder = () => screen.getByRole('button', { name: 'Cancel' });
    fireEvent.click(cancelOrder());
    fireEvent.click(cancelOrder());
    fireEvent.click(cancelOrder());
    expect(salesOrderListApi.cancel).toHaveBeenCalledExactlyOnceWith('1');
    expect(cancelOrder()).toBeDisabled();

    rejectCancellation(new Error('Order cancellation failed'));
    expect(await screen.findByText('Order cancellation failed')).toBeInTheDocument();
    expect(cancelOrder()).toBeEnabled();
  } finally {
    confirmation.mockRestore();
  }
});

it('previews pro forma without posting', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Pro forma confirmation' }));
  expect(await screen.findByRole('dialog', { name: /Pro forma confirmation/ })).toBeInTheDocument();
  expect((await within(screen.getByRole('dialog')).findAllByRole('grid')).length).toBeGreaterThan(
    0
  );
  expect(salesOrderConfirmationApi.post).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Close preview' }));
});

it('keeps the entered packing slip quantity when the preview refetches', async () => {
  const message =
    'Available physical stock excluding other reservations is insufficient for item ITEM-001.';
  vi.mocked(salesOrderPackingSlipApi.post).mockRejectedValueOnce(new Error(message));
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Packing slip' }));
  const dialog = await screen.findByRole('dialog', { name: 'Packing slip posting' });
  const quantity = await within(dialog).findByRole('spinbutton', {
    name: 'Update quantity for ITEM-1',
  });
  expect(quantity).toHaveValue(6);
  fireEvent.change(quantity, { target: { value: '2' } });
  await act(async () => {
    await queryClient.invalidateQueries({
      queryKey: salesOrderQueryKeys.packingSlipPreview(
        useCompanyStore.getState().currentCompany,
        '1'
      ),
    });
  });
  expect(salesOrderPackingSlipApi.postingPreview).toHaveBeenCalledTimes(2);
  expect(quantity).toHaveValue(2);
  fireEvent.click(within(dialog).getByRole('button', { name: 'OK' }));
  await waitFor(() =>
    expect(salesOrderPackingSlipApi.post).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ lines: [{ salesLineId: '10', quantity: 2 }] })
    )
  );
  expect((await within(dialog).findByText(message)).closest('[role="alert"]')).toBeInTheDocument();
  expect(quantity).toHaveValue(2);
  expect(within(dialog).getByRole('button', { name: 'OK' })).toBeEnabled();
});

it('confirms immediately without opening a dialog', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirm now' }));
  await waitFor(() =>
    expect(salesOrderConfirmationApi.post).toHaveBeenCalledWith(
      '1',
      expect.any(String),
      expect.any(String)
    )
  );
  expect(screen.queryByRole('dialog')).toBeNull();
});

it('reviews the confirmation and only posts when Posting is enabled', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirmation' }));
  expect((await within(screen.getByRole('dialog')).findAllByRole('grid')).length).toBeGreaterThan(
    0
  );
  fireEvent.click(screen.getByRole('button', { name: 'Lines' }));
  expect(await within(screen.getByRole('dialog')).findByText('ITEM-1')).toBeInTheDocument();
  fireEvent.click(screen.getByRole('button', { name: 'Parameters' }));
  fireEvent.click(screen.getByLabelText('Posting'));
  fireEvent.click(screen.getByRole('button', { name: 'Close preview' }));
  expect(salesOrderConfirmationApi.post).not.toHaveBeenCalled();
  await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  if (!screen.queryByRole('button', { name: '' }))
    fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirmation' }));
  const submit = await screen.findByRole('button', { name: 'OK' });
  await waitFor(() => expect(submit).toBeEnabled());
  fireEvent.click(submit);
  await waitFor(() =>
    expect(salesOrderConfirmationApi.post).toHaveBeenCalledWith(
      '1',
      expect.any(String),
      expect.any(String)
    )
  );
});

it('does not substitute another order for an unknown ID', async () => {
  openOrder('999');
  expect(await screen.findByText('No sales orders are available.')).toBeDefined();
  expect(screen.queryByText('SO-LIVE-001 : Live customer')).toBeNull();
});

it('toggles the record sidebar and opens the header tab', async () => {
  const view = openOrder('1', false);
  await screen.findByText('SO-LIVE-001 : Live customer');
  const toggle = screen.getByRole('button', { name: /Toggle record list/i });
  expect(toggle.getAttribute('aria-pressed')).toBe('false');
  fireEvent.click(toggle);
  expect(toggle.getAttribute('aria-pressed')).toBe('true');
  expect(globalThis.localStorage.getItem('ixapp.sales-order.list-visible')).toBe('true');
  view.unmount();
  openOrder('1');
  expect(
    screen.getByRole('button', { name: /Toggle record list/i }).getAttribute('aria-pressed')
  ).toBe('true');
  const restoredToggle = screen.getByRole('button', { name: /Toggle record list/i });
  fireEvent.click(restoredToggle);
  expect(restoredToggle.getAttribute('aria-pressed')).toBe('false');
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  expect(screen.getByLabelText('Invoice account').textContent).toBe('C-002');
  expect(screen.queryByText('Sales order lines')).toBeNull();
});

it('opens advanced filters and applies and resets a sales order condition', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Filter' }));
  expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
  const input = within(
    screen.getByRole('heading', { name: 'Filters' }).parentElement!.parentElement!
  ).getByRole('textbox');
  fireEvent.change(input, { target: { value: 'no-match' } });
  fireEvent.click(screen.getByRole('button', { name: 'Apply' }));
  expect(screen.queryByText('SO-LIVE-001')).toBeNull();
  fireEvent.click(screen.getAllByRole('button', { name: 'Reset' }).at(-1)!);
  expect(screen.getByText('SO-LIVE-001')).toBeDefined();
});

it('toggles line filters through Search without opening the advanced panel', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
  fireEvent.click(screen.getByRole('button', { name: 'Search' }));
  expect(screen.getAllByPlaceholderText('Filter value').length).toBeGreaterThan(0);
  expect(screen.queryByRole('heading', { name: 'Filters' })).toBeNull();
  fireEvent.click(screen.getByRole('button', { name: 'Search' }));
  expect(screen.queryByPlaceholderText('Filter value')).toBeNull();
});

it('saves on cell blur, locks item identity and names, and removes the line', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  // Query inside the lines toolbar and grid: whole-page role queries over the
  // order form made this flow take ~50 s in jsdom.
  const toolbar = within(screen.getByRole('toolbar', { name: 'Sales order line actions' }));
  const grid = within(screen.getByRole('grid'));
  fireEvent.click(toolbar.getByRole('button', { name: 'Add line' }));
  expect(screen.queryByRole('dialog')).toBeNull();
  fireEvent.click(grid.getByRole('textbox', { name: 'Item number' }));
  fireEvent.click(await screen.findByText('ITEM-1'));
  await waitFor(() => expect(grid.queryByRole('textbox', { name: 'Item number' })).toBeNull());
  expect(grid.queryByRole('textbox', { name: 'Product name' })).toBeNull();
  expect(grid.queryByRole('textbox', { name: 'Arabic name' })).toBeNull();
  expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  const quantity = grid.getByRole('spinbutton', { name: 'Quantity' });
  fireEvent.change(quantity, { target: { value: '3' } });
  expect(salesOrderLinesApi.add).not.toHaveBeenCalled();
  fireEvent.blur(quantity);
  await waitFor(() =>
    expect(salesOrderLinesApi.add).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ itemNumber: 'ITEM-1', quantity: 3 })
    )
  );
  await waitFor(() =>
    expect(grid.getByRole('spinbutton', { name: 'Quantity' })).not.toBeDisabled()
  );
  fireEvent.change(grid.getByRole('spinbutton', { name: 'Quantity' }), {
    target: { value: '4' },
  });
  fireEvent.blur(grid.getByRole('spinbutton', { name: 'Quantity' }));
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ id: '10', quantity: 4 })
    )
  );
  await waitFor(() => expect(toolbar.getByRole('button', { name: 'Remove' })).not.toBeDisabled());
  fireEvent.click(grid.getByLabelText('Unit'));
  await waitFor(() => expect(grid.getByRole('combobox', { name: 'Unit' })).not.toBeDisabled());
  fireEvent.keyDown(grid.getByRole('combobox', { name: 'Unit' }), { key: 'ArrowDown' });
  fireEvent.click(await screen.findByRole('option', { name: /BOX/ }));
  fireEvent.blur(grid.getByRole('combobox', { name: 'Unit' }));
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ unit: 'BOX' })
    )
  );
  await waitFor(() => expect(toolbar.getByRole('button', { name: 'Remove' })).not.toBeDisabled());
  fireEvent.click(toolbar.getByRole('button', { name: 'Remove' }));
  await waitFor(() => expect(salesOrderLinesApi.remove).toHaveBeenCalledWith('1', '10'));
});

it('edits the header and saves only from the action pane', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  expect(screen.queryByRole('button', { name: 'Edit' })).toBeNull();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  fireEvent.change(screen.getByRole('textbox', { name: 'Customer reference' }), {
    target: { value: 'PO-UPDATED' },
  });
  expect(salesOrderListApi.updateHeader).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  expect(screen.getByRole('textbox', { name: 'Customer reference' })).toHaveValue('PO-UPDATED');
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() =>
    expect(salesOrderListApi.updateHeader).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ customerReference: 'PO-UPDATED', invoiceAccount: 'C-002' })
    )
  );
  await waitFor(() => expect(screen.queryByRole('button', { name: 'Save' })).toBeNull());
});

it('keeps header changes available after a failed save and supports cancel', async () => {
  vi.mocked(salesOrderListApi.updateHeader).mockRejectedValueOnce(new Error('Header save failed'));
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  expect(screen.queryByRole('button', { name: 'Edit' })).toBeNull();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  fireEvent.change(screen.getByRole('textbox', { name: 'Customer reference' }), {
    target: { value: 'RETRY' },
  });
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  expect(await screen.findByText('Header save failed')).toBeDefined();
  expect(screen.getByRole('textbox', { name: 'Customer reference' })).toHaveValue('RETRY');
  fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  expect(screen.getByRole('button', { name: 'Edit' })).toBeEnabled();
  expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  expect(screen.queryByRole('button', { name: 'Cancel' })).toBeNull();
  expect(screen.queryByRole('textbox', { name: 'Customer reference' })).toBeNull();
});

it('keeps page and line in edit mode on invalid Save, then closes both after a valid Save', async () => {
  queryClient.removeQueries({ queryKey: salesOrderQueryKeys.all });
  vi.mocked(salesOrderLinesApi.list).mockResolvedValueOnce([
    {
      id: '42',
      lineNumber: 1,
      itemNumber: 'ITEM-42',
      description: 'Save test',
      quantity: 2,
      unit: 'Pcs',
      unitPrice: 5,
      lineTotal: 10,
      deliveryDate: '2026-09-09',
    },
  ]);
  openOrder('1');
  await screen.findAllByText('ITEM-42');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  fireEvent.click(screen.getByLabelText('Quantity'));
  const quantity = screen.getByRole('spinbutton', { name: 'Quantity' });
  fireEvent.change(quantity, { target: { value: '0' } });
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() => expect(quantity).toHaveAttribute('aria-invalid', 'true'));
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  expect(salesOrderListApi.updateHeader).not.toHaveBeenCalled();
  fireEvent.change(quantity, { target: { value: '3' } });
  fireEvent.click(screen.getByRole('button', { name: 'Save' }));
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ id: '42', quantity: 3 })
    )
  );
  await waitFor(() => expect(screen.getByRole('button', { name: 'Edit' })).toBeEnabled());
  expect(within(screen.getByRole('grid')).queryByRole('spinbutton')).toBeNull();
  expect(screen.queryByRole('button', { name: 'Cancel' })).toBeNull();
});

it('keeps invalid cell edits focused and moves to Unit after a successful Tab save', async () => {
  queryClient.removeQueries({ queryKey: salesOrderQueryKeys.all });
  vi.mocked(salesOrderLinesApi.update).mockClear();
  vi.mocked(salesOrderLinesApi.list).mockResolvedValueOnce([
    {
      id: '42',
      lineNumber: 1,
      itemNumber: 'ITEM-42',
      description: 'Keyboard item',
      quantity: 2,
      unit: 'Pcs',
      unitPrice: 5,
      lineTotal: 10,
      deliveryDate: '2026-09-09',
    },
  ]);
  openOrder('1');
  await screen.findAllByText('ITEM-42');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  fireEvent.click(screen.getByLabelText('Quantity'));
  const quantity = screen.getByRole('spinbutton', { name: 'Quantity' });
  fireEvent.change(quantity, { target: { value: '0' } });
  fireEvent.keyDown(quantity, { key: 'Tab' });
  await waitFor(() => expect(quantity).toHaveAttribute('aria-invalid', 'true'));
  expect(salesOrderLinesApi.update).not.toHaveBeenCalled();
  expect(quantity).toHaveFocus();
  fireEvent.change(quantity, { target: { value: '3' } });
  fireEvent.keyDown(quantity, { key: 'Tab' });
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ id: '42', quantity: 3 })
    )
  );
  await waitFor(
    () =>
      expect(
        within(screen.getByRole('grid')).getByRole('combobox', { name: 'Unit' })
      ).toHaveFocus(),
    { timeout: 3000 }
  );
  // The focused Unit editor must keep showing the line's unit, not go blank.
  const unitEditor = within(screen.getByRole('grid')).getByRole('combobox', { name: 'Unit' });
  expect(unitEditor).toHaveValue('Pcs');
  fireEvent.mouseDown(unitEditor);
  expect(unitEditor).toHaveValue('Pcs');
  expect(salesOrderLinesApi.update).toHaveBeenCalledTimes(1);
});

it('preserves an unfinished new line when switching between Lines and Header', async () => {
  const { container } = openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  const edit = container.querySelector<HTMLButtonElement>('button[aria-label="Edit"]')!;
  await waitFor(() => expect(edit).toBeEnabled());
  fireEvent.click(edit);
  const toolbar = () =>
    within(
      container.querySelector<HTMLElement>(
        '[role="toolbar"][aria-label="Sales order line actions"]'
      )!
    );
  const grid = () => within(container.querySelector<HTMLElement>('[role="grid"]')!);
  await waitFor(() => expect(toolbar().getByRole('button', { name: 'Add line' })).toBeEnabled());
  fireEvent.click(toolbar().getByRole('button', { name: 'Add line' }));
  expect(await grid().findByRole('textbox', { name: 'Item number' })).toBeDefined();
  const tab = (name: string) =>
    Array.from(container.querySelectorAll<HTMLElement>('[role="tab"]')).find(
      (element) => element.textContent === name
    )!;
  fireEvent.click(tab('Header'));
  await waitFor(() => expect(container.querySelector('[role="grid"]')).toBeNull());
  fireEvent.click(tab('Lines'));
  await waitFor(() => expect(container.querySelector('[role="grid"]')).not.toBeNull());
  expect(await grid().findByRole('textbox', { name: 'Item number' })).toBeDefined();
  await waitFor(() => expect(toolbar().getByRole('button', { name: 'Add line' })).toBeDisabled());
});
it('adds a line from view mode and switches the page to edit mode', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  await waitFor(() => expect(screen.getByRole('button', { name: 'Add line' })).toBeEnabled());
  fireEvent.click(screen.getByRole('button', { name: 'Add line' }));
  expect(await screen.findByRole('button', { name: 'Save' })).toBeDefined();
  expect(await screen.findByRole('textbox', { name: 'Item number' })).toBeDefined();
});

it('keeps header tab dropdowns editable in edit mode', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  const combos = await screen.findAllByRole('combobox');
  expect(combos.length).toBeGreaterThan(0);
  // Enabled once the header lookup lists have loaded.
  await waitFor(() =>
    expect(screen.getAllByRole('combobox').some((c) => !(c as HTMLInputElement).disabled)).toBe(
      true
    )
  );
  // No view-mode pencils left on header fields while editing.
  expect(screen.queryByRole('button', { name: 'Edit Site' })).toBeNull();
});

it('edits a line cell from view mode with the cell pencil (D365 edit from view)', async () => {
  queryClient.removeQueries({ queryKey: salesOrderQueryKeys.all });
  vi.mocked(salesOrderLinesApi.list).mockResolvedValueOnce([
    {
      id: '43',
      lineNumber: 1,
      itemNumber: 'ITEM-43',
      description: 'Mouse item',
      quantity: 2,
      unit: 'Pcs',
      unitPrice: 5,
      lineTotal: 10,
      deliveryDate: '2026-09-09',
    },
  ]);
  openOrder('1');
  await screen.findAllByText('ITEM-43');
  expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  const grid = within(screen.getByRole('grid'));
  fireEvent.click(grid.getByRole('button', { name: 'Edit Quantity' }));
  // The page switches to edit mode and the clicked cell is being edited.
  expect(await screen.findByRole('button', { name: 'Save' })).toBeEnabled();
  await waitFor(() => expect(grid.getByRole('spinbutton', { name: 'Quantity' })).toHaveFocus());
});

it('renders the V2 sales order in the master-details pattern', async () => {
  const { container } = render(
    <MemoryRouter initialEntries={['/accounts-receivable/sales-orders/1/master-details']}>
      <AppProviders>
        <Routes>
          <Route
            path="/accounts-receivable/sales-orders/:salesOrderId/master-details"
            element={<SalesOrderDetailsPageV2 />}
          />
        </Routes>
      </AppProviders>
    </MemoryRouter>
  );
  expect(await screen.findByText('SO-LIVE-001 : Live customer')).toBeDefined();
  expect(container.querySelector('[data-master-detail-viewport]')).toBeTruthy();
  expect(screen.getByRole('tab', { name: 'Lines' })).toBeDefined();
  expect(screen.getByRole('tab', { name: 'Header' })).toBeDefined();
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  expect(await screen.findByRole('button', { name: 'Cancel' })).toBeDefined();
  fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
  expect(await screen.findByRole('button', { name: 'Edit' })).toBeDefined();
});
