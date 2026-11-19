import React from 'react';
import {
  act,
  fireEvent,
  render,
  renderHook,
  screen,
  waitFor,
  within,
} from '@testing-library/react';
import { beforeEach, expect, it, vi } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { queryClient } from '@core/api/queryClient';
import { CustFreeInvoiceDetailsPage } from '@modules/finance/accounts-receivable/free-text-invoice/pages/CustFreeInvoiceDetailsPage';
import { useFreeTextInvoiceDocument } from '@modules/finance/accounts-receivable/free-text-invoice/details/useFreeTextInvoiceDocument';
import { newInvoice } from '@modules/finance/accounts-receivable/free-text-invoice/details/freeTextInvoiceModel';
import { freeTextInvoiceApi } from '@modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi', () => ({
  customerQuickCreateApi: { list: vi.fn().mockResolvedValue([]) },
}));
vi.mock('@modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi', () => ({
  freeTextInvoiceApi: {
    headerLookups: vi.fn().mockResolvedValue({}),
    taxGroups: vi.fn().mockResolvedValue({ salesTaxGroups: [], itemSalesTaxGroups: [] }),
    ledgerDimensions: vi.fn().mockResolvedValue({ data: [], totalCount: 0 }),
    list: vi.fn().mockResolvedValue([]),
    save: vi.fn(),
  },
}));
function Wrapper({ children }: { children: React.ReactNode }) {
  return (
    <AppProviders>
      <MemoryRouter>{children}</MemoryRouter>
    </AppProviders>
  );
}
beforeEach(() => {
  queryClient.clear();
  vi.clearAllMocks();
});

it('loads the extracted details editor through the free-text invoice route', async () => {
  const { container } = render(
    <AppProviders>
      <MemoryRouter initialEntries={['/accounts-receivable/free-text-invoices/new']}>
        <Routes>
          <Route
            path="/accounts-receivable/free-text-invoices/:invoiceRecId"
            element={<CustFreeInvoiceDetailsPage />}
          />
        </Routes>
      </MemoryRouter>
    </AppProviders>
  );
  expect(await screen.findByRole('button', { name: 'Save' })).toBeInTheDocument();
  expect(container.querySelector('[data-master-detail-viewport]')).toBeTruthy();
  expect(screen.getByRole('tab', { name: 'Header' })).toBeInTheDocument();
  const lineTabs = within(screen.getByRole('tablist', { name: 'Line details' }));
  fireEvent.click(lineTabs.getByRole('tab', { name: 'Setup' }));
  expect(screen.getByRole('tabpanel', { name: 'Setup' })).toBeInTheDocument();
  fireEvent.click(screen.getByRole('tab', { name: 'Header' }));
  await waitFor(() => expect(screen.queryByRole('grid')).toBeNull());
  fireEvent.click(screen.getByRole('tab', { name: 'Lines' }));
  expect(await screen.findByRole('grid')).toBeInTheDocument();
  expect(
    within(screen.getByRole('tablist', { name: 'Line details' })).getByRole('tab', {
      name: 'Setup',
    })
  ).toHaveAttribute('aria-selected', 'true');
});

it('retains the invoice draft after invalid Save and accepts a corrected line', async () => {
  const invoice = newInvoice('C-001');
  const { result } = renderHook(
    () =>
      useFreeTextInvoiceDocument({
        company: 'dat',
        invoice,
        isNew: true,
        startEditing: false,
      }),
    { wrapper: Wrapper }
  );
  await act(async () => {
    await result.current.save();
  });
  expect(result.current.editing).toBe(true);
  expect(result.current.error).not.toBe('');
  expect(freeTextInvoiceApi.save).not.toHaveBeenCalled();
  const corrected = {
    ...invoice,
    lines: [
      { ...invoice.lines[0], description: 'Consulting', ledgerDimension: 10, unitPrice: 100 },
    ],
  };
  vi.mocked(freeTextInvoiceApi.save).mockResolvedValue({ ...corrected, id: '1', recId: 1 });
  act(() => result.current.changeDraft(corrected));
  await act(async () => {
    await result.current.save();
  });
  expect(freeTextInvoiceApi.save).toHaveBeenCalledOnce();
  expect(result.current.editing).toBe(false);
});
