import i18n from '@core/localization/i18n';
import { fireEvent, render, screen } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { MemoryRouter } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { CustFreeInvoiceListPage } from '@modules/finance/accounts-receivable/free-text-invoice/pages/CustFreeInvoiceListPage';
import {
  freeTextInvoiceApi,
  type FreeTextInvoiceRecord,
} from '@modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi';
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({
    hasPermission: (permission: string) => permission !== 'AccountsReceivable.Customers.View',
  }),
}));
vi.mock('@modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi', () => ({
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
describe('free text invoice locale', () => {
  beforeEach(async () => {
    await i18n.changeLanguage('en');
  });
  afterEach(async () => {
    await i18n.changeLanguage('en');
  });
  it('formats stored totals using the selected application language', async () => {
    await i18n.changeLanguage('ar');
    vi.mocked(freeTextInvoiceApi.list).mockResolvedValue([draft]);
    open();
    await screen.findByText('DRAFT-1');
    fireEvent.click(screen.getByRole('button', { name: i18n.t('freeTextInvoices.totals') }));
    const dialog = await screen.findByRole('dialog', { name: i18n.t('freeTextInvoices.totals') });
    const taxAmount = new Intl.NumberFormat('ar', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(draft.taxAmount);
    expect(dialog).toHaveTextContent(`${i18n.t('freeTextInvoices.taxAmount')}: ${taxAmount}`);
    expect(freeTextInvoiceApi.post).not.toHaveBeenCalled();
  });
});
