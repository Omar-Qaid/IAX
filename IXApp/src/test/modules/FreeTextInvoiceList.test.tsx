import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { CustFreeInvoiceListPage } from '@modules/finance/accounts-receivable/pages/CustFreeInvoiceListPage';
import {
  freeTextInvoiceApi,
  type FreeTextInvoiceRecord,
} from '@modules/finance/accounts-receivable/api/freeTextInvoiceApi';
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({
    hasPermission: (permission: string) => permission !== 'AccountsReceivable.Customers.View',
  }),
}));
vi.mock('@modules/finance/accounts-receivable/api/freeTextInvoiceApi', () => ({
  freeTextInvoiceApi: { list: vi.fn(), post: vi.fn(), remove: vi.fn() },
}));
const today = new Date();
const date = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
const draft: FreeTextInvoiceRecord = {
  id: '1',
  recId: 1,
  invoiceId: 'DRAFT-1',
  customerAccount: 'C-001',
  invoiceAccount: 'C-001',
  description: 'Consulting',
  invoiceDate: date,
  dueDate: date,
  currencyCode: 'SAR',
  paymentTerms: 'NET30',
  paymentMethod: 'BANK',
  postingProfile: 'GEN',
  defaultDimension: 0,
  taxGroup: 'VAT',
  posted: false,
  netAmount: 2000,
  taxAmount: 300,
  invoiceAmount: 2300,
  lines: [],
};
function open() {
  render(
    <MemoryRouter
      initialEntries={['/accounts-receivable/free-text-invoices?customerAccount=C-001']}
    >
      <AppProviders>
        <CustFreeInvoiceListPage />
      </AppProviders>
    </MemoryRouter>
  );
}
describe('free text invoice list', () => {
  it('defaults to recent drafts, filters by the selected customer and prevents deleting posted invoices', async () => {
    vi.mocked(freeTextInvoiceApi.list).mockResolvedValue([
      draft,
      { ...draft, id: '2', recId: 2, invoiceId: 'POSTED-2', posted: true },
      { ...draft, id: '3', recId: 3, invoiceId: 'OTHER-CUSTOMER', customerAccount: 'C-002' },
    ]);
    open();
    expect(await screen.findByText('DRAFT-1')).toBeInTheDocument();
    expect(screen.queryByText('OTHER-CUSTOMER')).not.toBeInTheDocument();
    expect(screen.queryByText('POSTED-2')).not.toBeInTheDocument();
    fireEvent.mouseDown(screen.getByRole('combobox', { name: 'Status' }));
    fireEvent.click(await screen.findByRole('option', { name: /^Posted$/ }));
    expect(await screen.findByText('POSTED-2')).toBeInTheDocument();
    fireEvent.click(screen.getByText('Consulting'));
    await waitFor(() => expect(screen.getByRole('button', { name: /^Delete$/ })).toBeDisabled());
    expect(screen.getByRole('button', { name: /^Totals$/ })).toBeEnabled();
    expect(screen.getByRole('button', { name: /^Post$/ })).toBeDisabled();
    expect(freeTextInvoiceApi.post).not.toHaveBeenCalled();
    expect(freeTextInvoiceApi.remove).not.toHaveBeenCalled();
  });
  it('shows stored invoice totals without posting', async () => {
    vi.mocked(freeTextInvoiceApi.list).mockResolvedValue([draft]);
    open();
    await screen.findByText('DRAFT-1');
    fireEvent.click(screen.getByRole('button', { name: /^Totals$/ }));
    expect(await screen.findByRole('dialog', { name: 'Totals' })).toBeInTheDocument();
    expect(screen.getByText('Sales tax: 300.00')).toBeInTheDocument();
    expect(freeTextInvoiceApi.post).not.toHaveBeenCalled();
  });
});
