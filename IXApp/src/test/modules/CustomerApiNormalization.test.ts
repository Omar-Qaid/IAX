import { beforeEach, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { customerQuickCreateApi } from '@modules/finance/accounts-receivable/api/customerQuickCreateApi';
vi.mock('@core/api/apiClient', () => ({ apiClient: { get: vi.fn(), put: vi.fn() } }));
const dto = {
  recId: 1,
  party: 7,
  accountNumber: 'C-001',
  name: 'Customer',
  nameAr: 'عميل',
  customerGroupId: 'Retail',
  currencyCode: 'SAR',
  status: 'active',
};
beforeEach(() => {
  vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [dto] } });
  vi.mocked(apiClient.put).mockResolvedValue({ data: { success: true, data: dto } });
});
it('preserves bilingual names and displays omitted optional customer fields as empty values', async () => {
  const [record] = await customerQuickCreateApi.list();
  expect(record.nameAr).toBe('عميل');
  for (const field of [
    'paymTermId',
    'paymModeId',
    'dlvModeId',
    'taxGroupId',
    'vatNum',
    'countryRegionId',
    'invoiceAccount',
    'inventSiteId',
    'inventLocationId',
    'custCategory',
  ] as const)
    expect(record[field]).toBe('');
});
it('does not send the literal undefined string when saving a customer from a sparse response', async () => {
  const [record] = await customerQuickCreateApi.list();
  await customerQuickCreateApi.update({ ...record, name: 'Edited' });
  expect(apiClient.put).toHaveBeenCalledWith(
    '/v1/CustTable/quick-update/1',
    expect.objectContaining({
      name: 'Edited',
      nameAlias: 'عميل',
      paymTermId: '',
      vatNum: '',
      inventSiteId: '',
    }),
  );
});
