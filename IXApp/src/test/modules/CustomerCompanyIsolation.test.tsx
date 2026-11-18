import { afterEach, expect, it, vi } from 'vitest';
import { render, act } from '@testing-library/react';
import { screen, waitFor } from '@test/testUtils';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AppProviders } from '@app/providers/AppProviders';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi';
import { CustTablePage } from '@modules/finance/accounts-receivable/customer/pages/CustTablePage';
vi.mock('@core/auth/useAuth', () => ({ useAuth: () => ({ hasPermission: () => true }) }));
vi.mock('@shared/components/logistics/PartyLogisticsPanels', () => ({
  PartyPostalAddressPanel: () => null,
  PartyElectronicAddressPanel: () => null,
}));
vi.mock('@modules/finance/accounts-receivable/sales-order/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: { inventoryDimensions: async () => ({ sites: [], warehouses: [] }) },
}));
vi.mock('@modules/finance/accounts-receivable/customer/api/customerQuickCreateApi', () => ({
  customerQuickCreateApi: {
    list: vi.fn(),
    lookups: async () => ({}),
    remove: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
  },
}));
const initialCompany = useCompanyStore.getState().currentCompany;
afterEach(() => useCompanyStore.setState({ currentCompany: initialCompany }));
it('reloads the same customer route in the new company instead of retaining the previous company record', async () => {
  useCompanyStore.setState({ currentCompany: 'dat' });
  vi.mocked(customerQuickCreateApi.list).mockImplementation(async () => {
    const company = useCompanyStore.getState().currentCompany;
    return [
      {
        id: '1',
        recId: 1,
        party: 0,
        accountNumber: 'C-' + company,
        name: 'Customer ' + company,
        nameAr: '',
        customerGroupId: 'Retail',
        currencyCode: 'SAR',
        status: 'active',
        custCategory: '',
        paymTermId: '',
        paymModeId: '',
        dlvModeId: '',
        taxGroupId: '',
        vatNum: '',
        countryRegionId: '',
        invoiceAccount: '',
        inventSiteId: '',
        inventLocationId: '',
        createdAt: '',
      },
    ];
  });
  render(
    <MemoryRouter initialEntries={['/customers/1']}>
      <AppProviders>
        <Routes>
          <Route path="/customers/:customerId" element={<CustTablePage />} />
        </Routes>
      </AppProviders>
    </MemoryRouter>,
  );
  await screen.findByText('C-dat : Customer dat');
  act(() => useCompanyStore.setState({ currentCompany: 'usmf' }));
  await screen.findByText('C-usmf : Customer usmf');
  await waitFor(() => expect(screen.queryByText('C-dat : Customer dat')).toBeNull());
});
