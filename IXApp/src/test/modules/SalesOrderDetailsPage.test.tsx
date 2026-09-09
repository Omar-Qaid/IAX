import React from 'react';
import { expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { salesOrderListApi } from '@modules/finance/accounts-receivable/api/salesOrderListApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/api/salesOrderLinesApi';
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

function openOrder(id: string) {
  return render(
    <MemoryRouter initialEntries={[`/accounts-receivable/sales-orders/${id}`]}>
      <AppProviders>
        <Routes>
          <Route
            path="/accounts-receivable/sales-orders/:salesOrderId"
            element={<SalesOrderDetailsPage />}
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
  expect(screen.getByText('250 SAR')).toBeDefined();
});

it('does not substitute another order for an unknown ID', async () => {
  openOrder('999');
  expect(await screen.findByText('No sales orders are available.')).toBeDefined();
  expect(screen.queryByText('SO-LIVE-001 : Live customer')).toBeNull();
});

it('toggles the record sidebar and opens the header tab', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  const toggle = screen.getByRole('button', { name: /Toggle record list/i });
  expect(toggle.getAttribute('aria-pressed')).toBe('true');
  fireEvent.click(toggle);
  expect(toggle.getAttribute('aria-pressed')).toBe('false');
  fireEvent.click(toggle);
  expect(toggle.getAttribute('aria-pressed')).toBe('true');
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  expect(screen.getByLabelText('Invoice account').textContent).toBe('C-002');
  expect(screen.queryByText('Sales order lines')).toBeNull();
});

it('opens advanced filters and applies and resets a sales order condition', async () => {
  openOrder('1');
  await screen.findByText('SO-LIVE-001 : Live customer');
  fireEvent.click(screen.getByRole('button', { name: 'Filter' }));
  expect(screen.getByRole('heading', { name: 'Filters' })).toBeDefined();
  const input = screen
    .getAllByRole('textbox')
    .find((element) => !element.getAttribute('placeholder'))!;
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
  fireEvent.click(screen.getByRole('button', { name: 'Add line' }));
  expect(screen.queryByRole('dialog')).toBeNull();
  fireEvent.click(screen.getByRole('textbox', { name: 'Item number' }));
  fireEvent.click(await screen.findByText('ITEM-1'));
  await waitFor(() => expect(screen.queryByRole('textbox', { name: 'Item number' })).toBeNull());
  expect(screen.queryByRole('textbox', { name: 'Product name' })).toBeNull();
  expect(screen.queryByRole('textbox', { name: 'Arabic name' })).toBeNull();
  expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  expect(screen.queryByRole('button', { name: 'Cancel' })).toBeNull();
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
