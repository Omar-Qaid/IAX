import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import {
  customerParametersApi,
  parameterValues,
} from '@modules/finance/accounts-receivable/setup/api/customerParametersApi';
vi.mock('@core/api/apiClient', () => ({ apiClient: { get: vi.fn(), put: vi.fn() } }));
const record = {
  recId: 4,
  key: 0,
  dataAreaId: 'USMF',
  rowVersion: 'AQ==',
  recVersion: 2,
  custPostingProfile: 'GEN',
  creditLimit: 250,
  taxOnPrepayment: 0,
  reservation: 2,
  salesOrderType: 3,
  creditLimitCheck: 1,
  partition: 44,
};
beforeEach(() => vi.resetAllMocks());
describe('customer parameters API', () => {
  it('selects only the singleton for the requested company', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: {
        success: true,
        data: [{ ...record, dataAreaId: 'DEMF' }, { ...record, key: 1 }, record],
      },
    });
    expect(await customerParametersApi.get('USMF')).toEqual(record);
    expect(apiClient.get).toHaveBeenCalledWith(
      '/v1/CustParameters',
      expect.objectContaining({ headers: { 'X-Company': 'USMF' } })
    );
  });
  it('does not manufacture defaults when the company has no parameter record', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [] } });
    expect(await customerParametersApi.get('USMF')).toBeNull();
    expect(apiClient.put).not.toHaveBeenCalled();
  });
  it('preserves unexposed fields and concurrency metadata in an update', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { success: true, data: { ...record, rowVersion: 'Ag==', recVersion: 3 } },
    });
    const saved = await customerParametersApi.save(record, {
      ...parameterValues(record),
      custPostingProfile: 'NEW',
      taxOnPrepayment: true,
    });
    expect(apiClient.put).toHaveBeenCalledWith(
      '/v1/CustParameters/4',
      expect.objectContaining({
        custPostingProfile: 'NEW',
        taxOnPrepayment: 1,
        rowVersion: 'AQ==',
        recVersion: 2,
        reservation: 2,
        salesOrderType: 3,
        creditLimitCheck: 1,
        partition: 44,
      }),
      { headers: { 'X-Company': 'USMF' } }
    );
    expect(saved.rowVersion).toBe('Ag==');
  });
  it('rejects duplicate singleton records and failed save responses', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: true, data: [record, { ...record, recId: 5 }] },
    });
    await expect(customerParametersApi.get('USMF')).rejects.toThrow('Multiple');
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { success: false, message: 'Version conflict', data: null },
    });
    await expect(customerParametersApi.save(record, parameterValues(record))).rejects.toThrow(
      'Version conflict'
    );
  });
});
