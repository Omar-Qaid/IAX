import { expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import {
  salesOrderListApi,
  type SalesOrderHeaderInput,
} from '@modules/finance/accounts-receivable/api/salesOrderListApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: {
    put: vi.fn().mockResolvedValue({ data: { success: true } }),
  },
}));

it('sends blank optional header dates as null', async () => {
  const input = {
    invoiceAccount: 'CUST-200',
    currencyCode: 'SAR',
    orderDate: '2026-10-13',
    deliveryDate: '2026-10-13',
    shippingDateRequested: '2026-10-13',
    shippingDateConfirmed: '',
    receiptDateConfirmed: '',
    deadline: '',
    fixedDueDate: '',
    paymentTermsBaseDate: '',
  } as SalesOrderHeaderInput;

  await salesOrderListApi.updateHeader('7', input);

  expect(apiClient.put).toHaveBeenCalledWith('/v1/SalesTable/7/header', {
    ...input,
    shippingDateRequested: '2026-10-13',
    shippingDateConfirmed: null,
    receiptDateConfirmed: null,
    deadline: null,
    fixedDueDate: null,
    paymentTermsBaseDate: null,
  });
});
