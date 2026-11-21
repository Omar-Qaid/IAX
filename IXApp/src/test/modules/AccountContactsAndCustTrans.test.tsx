import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { AccountContactsPage } from '@modules/finance/foundation/contacts/pages/AccountContactsPage';
import {
  accountContactsRoute,
  contactDetailsRoute,
} from '@modules/finance/foundation/contacts/routes/contactRoutes';
import { CustTrans } from '@modules/finance/accounts-receivable/customer/pages/CustTrans';
import { apiClient } from '@core/api/apiClient';
import '@core/localization/i18n';
import { PERMISSIONS } from '@core/permissions/permissions';

const denied = vi.hoisted(() => new Set<string>());
vi.mock('@core/auth/useAuth', () => ({
  useAuth: () => ({ hasPermission: (permission: string) => !denied.has(permission) }),
}));
vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));
vi.mock('@shared/components/logistics/PartyLogisticsPanels', () => ({
  PartyPostalAddressPanel: () => null,
  PartyElectronicAddressPanel: () => null,
}));
beforeEach(() => {
  vi.clearAllMocks();
  denied.clear();
});
function CurrentRoute() {
  return <output data-testid="contact-route">{useLocation().pathname}</output>;
}
function open(path: string) {
  render(
    <MemoryRouter initialEntries={[path]}>
      <QueryClientProvider
        client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}
      >
        <CurrentRoute />
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
  it('updates an existing contact through the original account-scoped API', async () => {
    let firstName = 'Sara';
    let alias = 'سارة';
    vi.mocked(apiClient.get).mockImplementation(async () => ({
      data: {
        name: 'Customer',
        contacts: [
          {
            recId: 12,
            party: 42,
            contactPersonId: 'CP12',
            name: `${firstName} Ali`,
            nameAlias: alias,
            languageId: 'en-US',
            nameParts: { firstName, middleName: '', lastNamePrefix: '', lastName: 'Ali' },
          },
        ],
      },
    }));
    vi.mocked(apiClient.put).mockImplementation(async (_url, body) => {
      const input = body as { nameParts: { firstName: string }; nameAlias: string };
      firstName = input.nameParts.firstName;
      alias = input.nameAlias;
      return { data: 12 };
    });
    open('/contacts/customer/C00015/12');
    fireEvent.click(await screen.findByRole('button', { name: 'Edit' }));
    fireEvent.change(screen.getByLabelText('First name'), { target: { value: 'Changed' } });
    fireEvent.change(screen.getByLabelText('Arabic name'), { target: { value: 'اسم جديد' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(apiClient.put).toHaveBeenCalledWith(
        '/v1/ContactPerson/accounts/customer/C00015/12',
        expect.objectContaining({ name: 'Changed Ali', nameAlias: 'اسم جديد' })
      )
    );
    expect(await screen.findByRole('button', { name: 'Edit' })).toBeEnabled();
    expect(screen.getByRole('heading', { name: 'Changed Ali' })).toBeInTheDocument();
    expect(screen.getByTestId('contact-route')).toHaveTextContent('/contacts/customer/C00015/12');
  });
  it('uses master-details for new contacts without inventing document tabs', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: { name: 'Customer', contacts: [] } });
    open('/contacts/customer/C00015/new');
    expect(await screen.findByLabelText('First name')).toBeEnabled();
    expect(screen.getByRole('heading', { name: 'New contact' })).toBeInTheDocument();
    expect(document.querySelector('[data-master-detail-viewport]')).toBeTruthy();
    expect(screen.queryByRole('tab', { name: 'Lines' })).toBeNull();
    expect(screen.queryByRole('tab', { name: 'Header' })).toBeNull();
    expect(
      within(screen.getByRole('group', { name: 'Contact' })).getByText('C00015 : Customer')
    ).toBeInTheDocument();
  });

  it('retains contact fields after validation and API failures', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: { name: 'Customer', contacts: [] } });
    vi.mocked(apiClient.post).mockRejectedValueOnce(new Error('Unable to save contact'));
    open('/contacts/customer/C00015/new');
    fireEvent.click(await screen.findByRole('button', { name: 'Save' }));
    expect(await screen.findByText('Contact name is required.')).toBeInTheDocument();
    expect(apiClient.post).not.toHaveBeenCalled();
    fireEvent.change(screen.getByLabelText('First name'), { target: { value: 'Sara' } });
    fireEvent.change(screen.getByLabelText('Arabic name'), { target: { value: 'سارة' } });
    fireEvent.change(screen.getByLabelText('Language'), { target: { value: '' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    expect(await screen.findByText('Language is required.')).toBeInTheDocument();
    expect(apiClient.post).not.toHaveBeenCalled();
    fireEvent.change(screen.getByLabelText('Language'), { target: { value: 'en-US' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    expect(await screen.findByText('Unable to save contact')).toBeInTheDocument();
    expect(screen.getByLabelText('First name')).toHaveValue('Sara');
    expect(screen.getByLabelText('Arabic name')).toHaveValue('سارة');
    expect(screen.getByRole('button', { name: 'Save' })).toBeEnabled();
  });

  it('opens the saved contact in view mode while preserving the account and both names', async () => {
    let saved = false;
    const contact = {
      recId: 12,
      party: 42,
      contactPersonId: 'CP12',
      name: 'Sara Ali',
      nameAlias: 'سارة علي',
      languageId: 'en-US',
      nameParts: { firstName: 'Sara', middleName: '', lastNamePrefix: '', lastName: 'Ali' },
    };
    vi.mocked(apiClient.get).mockImplementation(async () => ({
      data: { name: 'Customer', contacts: saved ? [contact] : [] },
    }));
    vi.mocked(apiClient.post).mockImplementation(async () => {
      saved = true;
      return { data: 12 };
    });
    open('/contacts/customer/C00015/new');
    fireEvent.change(await screen.findByLabelText('First name'), { target: { value: 'Sara' } });
    fireEvent.change(screen.getByLabelText('Last name'), { target: { value: 'Ali' } });
    fireEvent.change(screen.getByLabelText('Arabic name'), { target: { value: 'سارة علي' } });
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() =>
      expect(screen.getByTestId('contact-route')).toHaveTextContent('/contacts/customer/C00015/12')
    );
    expect(await screen.findByRole('button', { name: 'Edit' })).toBeEnabled();
    expect(screen.queryByLabelText('First name')).toBeNull();
    expect(apiClient.post).toHaveBeenCalledWith(
      '/v1/ContactPerson/accounts/customer/C00015',
      expect.objectContaining({ name: 'Sara Ali', nameAlias: 'سارة علي' })
    );
  });

  it('does not expose contact editing without edit permission', async () => {
    denied.add(PERMISSIONS.CONTACT_EDIT);
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
          },
        ],
      },
    });
    open('/contacts/customer/C00015/12');
    expect(await screen.findByRole('heading', { name: 'Sara Ali' })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Edit' })).toBeNull();
    expect(screen.queryByLabelText('First name')).toBeNull();
  });

  it('confirms deletion before removing the contact and returning to the account list', async () => {
    let deleted = false;
    vi.mocked(apiClient.get).mockImplementation(async () => ({
      data: {
        name: 'Customer',
        contacts: deleted
          ? []
          : [
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
    vi.mocked(apiClient.delete).mockImplementation(async () => {
      deleted = true;
      return { data: undefined };
    });
    open('/contacts/customer/C00015/12');
    fireEvent.click(await screen.findByRole('button', { name: 'Delete' }));
    expect(apiClient.delete).not.toHaveBeenCalled();
    fireEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Delete' }));
    expect(await screen.findByRole('button', { name: 'New contact' })).toBeInTheDocument();
    expect(apiClient.delete).toHaveBeenCalledWith('/v1/ContactPerson/12');
    expect(screen.getByTestId('contact-route')).toHaveTextContent('/contacts/customer/C00015');
  });
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
  it.each(['Cancel', 'Escape'])(
    'cancels a new contact back to its account using %s without saving',
    async (method) => {
      vi.mocked(apiClient.get).mockResolvedValue({ data: { name: 'Customer', contacts: [] } });
      const saveCalls = vi.mocked(apiClient.post).mock.calls.length;
      open('/contacts/customer/C00015/new');
      fireEvent.change(await screen.findByLabelText('First name'), {
        target: { value: 'Unsaved' },
      });
      expect(screen.getByRole('button', { name: 'Toggle record list' })).toHaveAttribute(
        'aria-pressed',
        'false'
      );
      if (method === 'Escape')
        fireEvent.keyDown(screen.getByLabelText('First name'), { key: 'Escape' });
      else fireEvent.click(screen.getByRole('button', { name: 'Cancel' }));
      expect(await screen.findByRole('button', { name: 'New contact' })).toBeInTheDocument();
      expect(apiClient.post).toHaveBeenCalledTimes(saveCalls);
    }
  );
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
    // The inquiry shows the reference form's Edit command, but it never edits a posted transaction.
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDisabled();
  });
});
