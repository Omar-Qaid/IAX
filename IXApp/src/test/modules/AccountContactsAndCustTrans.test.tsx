import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AccountContactsPage } from '@modules/finance/foundation/contacts/pages/AccountContactsPage';
import { accountContactsRoute, contactDetailsRoute } from '@modules/finance/foundation/contacts/routes/contactRoutes';
import { CustTrans } from '@modules/finance/accounts-receivable/customer/pages/CustTrans';
import { apiClient } from '@core/api/apiClient';
import '@core/localization/i18n';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@core/api/apiClient', () => ({ apiClient: { get: vi.fn(), post: vi.fn() } }));
vi.mock('@shared/components/logistics/PartyLogisticsPanels', () => ({
  PartyPostalAddressPanel: () => null,
  PartyElectronicAddressPanel: () => null,
}));
function open(path: string) {
  render(
    <MemoryRouter initialEntries={[path]}>
      <QueryClientProvider
        client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}
      >
        <Routes>
          <Route path={accountContactsRoute} element={<AccountContactsPage />} />
          <Route path={contactDetailsRoute} element={<AccountContactsPage />} />
          <Route path="/CustTrans/:accountNum" element={<CustTrans />} />
          <Route
            path="/accounts-receivable/customers/91"
            element={<div>Customer destination</div>}
          />
        </Routes>
      </QueryClientProvider>
    </MemoryRouter>
  );
}
describe('account inquiries', () => {
  it('opens the owning customer record from Contact Accounts and disables an unavailable vendor form', async () => {
    vi.mocked(apiClient.get).mockImplementation(async (url) => ({
      data:
        url === '/v1/CustTable/list'
          ? {
              success: true,
              data: [{ recId: 91, accountNumber: 'C00015', name: 'Customer', party: 2 }],
            }
          : {
              name: 'Customer',
              contacts: [
                {
                  recId: 12,
                  party: 42,
                  contactPersonId: 'CP12',
                  name: 'Sara Ali',
                  nameAlias: '',
                  languageId: 'en-US',
                },
              ],
            },
    }));
    open('/contacts/customer/C00015/12');
    fireEvent.click(await screen.findByRole('button', { name: /^Contact$/ }));
    expect(screen.getByRole('button', { name: /^Vendor$/ })).toBeDisabled();
    expect(screen.getByRole('button', { name: /^Customer$/ })).toBeEnabled();
    fireEvent.click(screen.getByRole('button', { name: /^Customer$/ }));
    await waitFor(
      () => expect(apiClient.get).toHaveBeenCalledWith('/v1/CustTable/list', expect.anything()),
      { timeout: 5000 }
    );
    expect(
      await screen.findByText('Customer destination', {}, { timeout: 5000 })
    ).toBeInTheDocument();
  });
  it('cancels a new contact back to the account list without saving', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: { name: 'Customer', contacts: [] } });
    const saveCalls = vi.mocked(apiClient.post).mock.calls.length;
    open('/contacts/customer/C00015/new');
    fireEvent.change(await screen.findByLabelText('First name'), { target: { value: 'Unsaved' } });
    expect(screen.getByRole('button', { name: 'Toggle record list' })).toHaveAttribute(
      'aria-pressed',
      'false'
    );
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    expect(await screen.findByRole('button', { name: 'New contact' })).toBeInTheDocument();
    expect(apiClient.post).toHaveBeenCalledTimes(saveCalls);
  });
  it('restores contact fields when editing is cancelled', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: {
        name: 'Customer',
        contacts: [
          {
            recId: 12,
            party: 42,
            contactPersonId: 'CP12',
            name: 'Sara Ali',
            nameAlias: '',
            languageId: 'en-US',
            nameParts: { firstName: 'Sara', middleName: '', lastNamePrefix: '', lastName: 'Ali' },
          },
        ],
      },
    });
    open('/contacts/customer/C00015/12');
    fireEvent.click(await screen.findByRole('button', { name: 'Search' }));
    expect(screen.getByRole('button', { name: 'Toggle record list' })).toHaveAttribute(
      'aria-pressed',
      'true'
    );
    fireEvent.click(screen.getByRole('button', { name: /^Edit$/ }));
    expect(screen.getByRole('button', { name: 'Search' })).toBeDisabled();
    expect(screen.getByRole('button', { name: /^Contact$/ })).toBeDisabled();
    expect(screen.queryByRole('button', { name: /^New$/ })).not.toBeInTheDocument();
    fireEvent.change(screen.getByLabelText('First name'), { target: { value: 'Changed' } });
    fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
    fireEvent.click(screen.getByRole('button', { name: /^Edit$/ }));
    expect(screen.getByLabelText('First name')).toHaveValue('Sara');
  });
  it.each(['customer', 'vendor'])('creates a contact in the selected %s account', async (kind) => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { name: 'Selected account', contacts: [] },
    });
    vi.mocked(apiClient.post).mockResolvedValue({ data: 12 });
    open(`/contacts/${kind}/A-01`);
    fireEvent.click(await screen.findByRole('button', { name: 'New contact' }));
    fireEvent.change(await screen.findByLabelText('First name'), { target: { value: 'Sara' } });
    fireEvent.change(screen.getByLabelText('Last name'), { target: { value: 'Ali' } });
    fireEvent.click(screen.getByText('Show more fields'));
    fireEvent.change(screen.getByLabelText('Initials'), { target: { value: 'SA' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(apiClient.post).toHaveBeenCalledWith(
        `/v1/ContactPerson/accounts/${kind}/A-01`,
        expect.objectContaining({
          name: 'Sara Ali',
          initials: 'SA',
          nameParts: expect.objectContaining({ firstName: 'Sara', lastName: 'Ali' }),
        })
      )
    );
  });
  it('reads the selected customer ledger and displays fully settled rows', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: {
        success: true,
        data: [
          {
            recId: 17,
            voucher: 'SETTLED-17',
            invoice: 'INV17',
            transactionType: 'Sales',
            transDate: '2026-11-15',
            currencyCode: 'SAR',
            amountCur: 200,
            balanceCur: 0,
            amountMst: 200,
            balanceMst: 0,
          },
        ],
      },
    });
    open('/CustTrans/C-17');
    expect(await screen.findByText('SETTLED-17')).toBeInTheDocument();
    expect(apiClient.get).toHaveBeenCalledWith(
      '/v1/CustTrans/transactions',
      expect.objectContaining({ params: { accountNum: 'C-17' } })
    );
    expect(screen.queryByRole('button', { name: 'Edit' })).toBeNull();
  });
});
