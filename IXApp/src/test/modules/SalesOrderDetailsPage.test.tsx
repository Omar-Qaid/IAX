import React from 'react';
import { beforeEach, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { AppProviders } from '@app/providers/AppProviders';
import { salesOrderListApi } from '@modules/finance/accounts-receivable/api/salesOrderListApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/api/salesOrderLinesApi';
import { salesOrderConfirmationApi } from '@modules/finance/accounts-receivable/api/salesOrderConfirmationApi';
import { SalesOrderDetailsPage } from '@modules/finance/accounts-receivable/pages/SalesOrderDetailsPage';

vi.mock('@modules/finance/accounts-receivable/api/salesOrderListApi', () => ({
  salesOrderListApi: {
    updateHeader: vi.fn().mockResolvedValue(undefined),
    list: vi.fn().mockResolvedValue([
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
    ]),
  },
}));

vi.mock('@modules/finance/accounts-receivable/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: {
    list: vi.fn().mockResolvedValue([]),
    totals: vi.fn().mockResolvedValue({ subtotal: 250, totalDiscount: 0, salesTax: 0, totalCharges: 0, invoiceAmount: 250, quantity: 0, costValue: 0 }),
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

vi.mock('@modules/finance/accounts-receivable/api/salesOrderConfirmationApi', () => ({
  salesOrderConfirmationApi: {
    preview: vi.fn().mockResolvedValue({
      header: { currencyCode: 'SAR', qty: 1, salesBalance: 100, sumTax: 15, confirmAmount: 115 },
      lines: [{ id: '1', lineNum: 1, itemId: 'ITEM-1', name: 'Test item', qty: 1,
        inventLocationId: 'MAIN', wmsLocationId: '', salesPrice: 100, lineAmount: 100 }],
    }),
    post: vi.fn().mockResolvedValue({ id: '10' }),
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
  await waitFor(() => expect(salesOrderLinesApi.totals).toHaveBeenCalledWith('1', expect.anything()));
});

it('opens CustConfirmJour from Sell > Sales order confirmations', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Sales order confirmations' }));
  expect(await screen.findByText('CustConfirmJour page')).toBeInTheDocument();
});

it('previews pro forma without posting', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Pro forma confirmation' }));
  expect(await screen.findByRole('dialog', { name: /Pro forma confirmation/ })).toBeInTheDocument();
  expect(await screen.findByRole('table', { name: 'Confirmation overview' })).toBeInTheDocument();
  expect(salesOrderConfirmationApi.post).not.toHaveBeenCalled();
  fireEvent.click(screen.getByRole('button', { name: 'Close preview' }));
});

it('confirms immediately without opening a dialog', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirm now' }));
  await waitFor(() => expect(salesOrderConfirmationApi.post).toHaveBeenCalledWith(
    '1', expect.any(String), expect.any(String)
  ));
  expect(screen.queryByRole('dialog')).toBeNull();
});

it('reviews the confirmation and only posts when Posting is enabled', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirmation' }));
  expect(await screen.findByRole('table', { name: 'Confirmation overview' })).toBeInTheDocument();
  fireEvent.click(screen.getByRole('button', { name: 'Lines' }));
  expect(screen.getByRole('table', { name: 'Confirmation lines' })).toBeInTheDocument();
  fireEvent.click(screen.getByRole('switch', { name: 'Posting' }));
  fireEvent.click(screen.getByRole('button', { name: 'Close preview' }));
  expect(salesOrderConfirmationApi.post).not.toHaveBeenCalled();
  await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  fireEvent.click(screen.getByRole('button', { name: 'Sell' }));
  fireEvent.click(screen.getByRole('button', { name: 'Confirmation' }));
  fireEvent.click(await screen.findByRole('button', { name: 'OK' }));
  await waitFor(() => expect(salesOrderConfirmationApi.post).toHaveBeenCalledWith(
    '1', expect.any(String), expect.any(String)
  ));
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
  expect(screen.getByRole('button', { name: /Toggle record list/i }).getAttribute('aria-pressed')).toBe('true');
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
  const input = within(screen.getByRole('heading', { name: 'Filters' }).parentElement!.parentElement!).getByRole('textbox');
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
  fireEvent.click(screen.getByRole('button', { name: 'Add line' }));
  expect(screen.queryByRole('dialog')).toBeNull();
  fireEvent.click(screen.getByRole('textbox', { name: 'Item number' }));
  fireEvent.click(await screen.findByText('ITEM-1'));
  await waitFor(() => expect(screen.queryByRole('textbox', { name: 'Item number' })).toBeNull());
  expect(screen.queryByRole('textbox', { name: 'Product name' })).toBeNull();
  expect(screen.queryByRole('textbox', { name: 'Arabic name' })).toBeNull();
  expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  const quantity = screen.getByRole('spinbutton', { name: 'Quantity' });
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
    expect(screen.getByRole('spinbutton', { name: 'Quantity' })).not.toBeDisabled()
  );
  fireEvent.change(screen.getByRole('spinbutton', { name: 'Quantity' }), {
    target: { value: '4' },
  });
  fireEvent.blur(screen.getByRole('spinbutton', { name: 'Quantity' }));
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ id: '10', quantity: 4 })
    )
  );
  await waitFor(() => expect(screen.getByRole('button', { name: 'Remove' })).not.toBeDisabled());
  fireEvent.click(screen.getByLabelText('Unit'));
  await waitFor(() => expect(screen.getByRole('combobox', { name: 'Unit' })).not.toBeDisabled());
  fireEvent.keyDown(screen.getByRole('combobox', { name: 'Unit' }), { key: 'ArrowDown' });
  fireEvent.click(await screen.findByRole('option', { name: /BOX/ }));
  fireEvent.blur(screen.getByRole('combobox', { name: 'Unit' }));
  await waitFor(() =>
    expect(salesOrderLinesApi.update).toHaveBeenCalledWith(
      '1',
      expect.objectContaining({ unit: 'BOX' })
    )
  );
  await waitFor(() => expect(screen.getByRole('button', { name: 'Remove' })).not.toBeDisabled());
  fireEvent.click(screen.getByRole('button', { name: 'Remove' }));
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

it('keeps invalid cell edits focused and moves to Unit after a successful Tab save', async () => {
  queryClient.removeQueries({ queryKey: ['sales-order-lines'] });
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
  expect(salesOrderLinesApi.update).toHaveBeenCalledTimes(1);
});

it('preserves an unfinished new line when switching between Lines and Header', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
  fireEvent.click(screen.getByRole('button', { name: 'Add line' }));
  expect(screen.getByRole('textbox', { name: 'Item number' })).toBeDefined();
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  expect(screen.queryByRole('textbox', { name: 'Item number' })).toBeNull();
  fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
  expect(await screen.findByRole('textbox', { name: 'Item number' })).toBeDefined();
  expect(screen.getByRole('button', { name: 'Add line' })).toBeDisabled();
});
