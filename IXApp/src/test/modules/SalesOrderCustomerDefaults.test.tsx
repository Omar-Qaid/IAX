import { describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { SalesOrderQuickCreate } from '@modules/finance/accounts-receivable/components/SalesOrderQuickCreate';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/api/customerQuickCreateApi';
import { salesOrderLinesApi } from '@modules/finance/accounts-receivable/api/salesOrderLinesApi';
import { salesOrderListApi } from '@modules/finance/accounts-receivable/api/salesOrderListApi';

vi.mock('@modules/finance/accounts-receivable/api/customerQuickCreateApi', () => ({
  customerQuickCreateApi: {
    list: vi.fn(), lookups: vi.fn(), salesOrderDefaults: vi.fn(),
  },
}));
vi.mock('@modules/finance/accounts-receivable/api/salesOrderLinesApi', () => ({
  salesOrderLinesApi: { inventoryDimensions: vi.fn() },
}));
vi.mock('@modules/finance/accounts-receivable/api/salesOrderListApi', () => ({
  salesOrderListApi: { create: vi.fn() },
}));

describe('sales order customer selection', () => {
  it('shows customer defaults and submits the selected customer data', async () => {
    vi.mocked(customerQuickCreateApi.list).mockResolvedValue([{
      accountNumber: 'CUST-200', name: 'Retail Customer 200', nameAr: '', party: 9007199254740992,
      currencyCode: 'SAR', invoiceAccount: 'CUST-200', paymTermId: 'NET30',
      paymModeId: 'BANK', dlvModeId: 'GROUND', inventSiteId: '', inventLocationId: '',
    }] as Awaited<ReturnType<typeof customerQuickCreateApi.list>>);
    vi.mocked(customerQuickCreateApi.lookups).mockResolvedValue({
      customerGroups: [], salesTaxGroups: [], currencies: [],
      paymentTerms: [{ value: 'NET30', label: 'Net 30' }],
      paymentMethods: [{ value: 'BANK', label: 'Bank' }],
      deliveryTerms: [], deliveryModes: [{ value: 'GROUND', label: 'Ground' }],
      salesPools: [], paymentSchedules: [],
    });
    vi.mocked(customerQuickCreateApi.salesOrderDefaults).mockResolvedValue({
      address: '123 Market Road, Riyadh',
      contacts: [{ type: 'Email', number: 'retail200@example.com', primary: true }],
    });
    vi.mocked(salesOrderLinesApi.inventoryDimensions).mockResolvedValue({
      sites: [], warehouses: [],
    } as Awaited<ReturnType<typeof salesOrderLinesApi.inventoryDimensions>>);
    vi.mocked(salesOrderListApi.create).mockResolvedValue({ id: '1' } as Awaited<ReturnType<typeof salesOrderListApi.create>>);

    render(<SalesOrderQuickCreate open onClose={vi.fn()} onSave={vi.fn()} />);
    fireEvent.mouseDown(await screen.findByRole('combobox', { name: 'Customer account' }));
    fireEvent.click(await screen.findByText('CUST-200 - Retail Customer 200'));

    await waitFor(() => expect(customerQuickCreateApi.salesOrderDefaults).toHaveBeenCalledWith('CUST-200', expect.anything()));
    await waitFor(() => expect(screen.getByRole('textbox', { name: 'Address' })).toHaveValue('123 Market Road, Riyadh'));
    expect(screen.getByRole('textbox', { name: 'Delivery address' })).toHaveValue('123 Market Road, Riyadh');
    expect(screen.getByRole('combobox', { name: 'Contacts' })).toHaveTextContent('Email: retail200@example.com');

    fireEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() => expect(salesOrderListApi.create).toHaveBeenCalledWith(expect.objectContaining({
      customerAccount: 'CUST-200', invoiceAccount: 'CUST-200', currencyCode: 'SAR',
      paymentTerms: 'NET30', paymentMethod: 'BANK', deliveryMode: 'GROUND',
      contact: 'retail200@example.com', contactType: 'Email',
    })));
  });
});
