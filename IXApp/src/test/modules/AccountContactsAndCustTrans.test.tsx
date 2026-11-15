import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AccountContactsPage } from '@modules/finance/contacts/AccountContactsPage';
import { accountContactsRoute, contactDetailsRoute } from '@modules/finance/contacts/contactRoutes';
import { CustTrans } from '@modules/finance/accounts-receivable/pages/CustTrans';
import { apiClient } from '@core/api/apiClient';
import '@core/localization/i18n';

vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@core/api/apiClient', () => ({ apiClient: { get: vi.fn(), post: vi.fn() } }));
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
        </Routes>
      </QueryClientProvider>
    </MemoryRouter>
  );
}
describe('account inquiries', () => {
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
