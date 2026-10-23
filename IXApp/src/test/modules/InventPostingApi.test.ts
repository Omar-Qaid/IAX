import { beforeEach, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { inventPostingApi } from '@modules/finance/inventory/api/inventPostingApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

beforeEach(() => vi.clearAllMocks());

it('passes the selected posting type and cancellation signal to the backend', async () => {
  const signal = new AbortController().signal;
  vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [] } });
  expect(await inventPostingApi.list(9, signal)).toEqual([]);
  expect(apiClient.get).toHaveBeenCalledWith('/v1/InventPosting', {
    params: { accountType: 9 }, signal,
  });
});

it('surfaces lookup failures rather than returning missing data', async () => {
  vi.mocked(apiClient.get).mockResolvedValue({
    data: { success: false, data: null, message: 'Select a legal entity.' },
  });
  await expect(inventPostingApi.lookups()).rejects.toThrow('Select a legal entity.');
});

it('preserves the server validation message when creating a duplicate rule', async () => {
  vi.mocked(apiClient.post).mockResolvedValue({
    data: { success: false, data: null, message: 'Duplicate posting rule.' },
  });
  await expect(inventPostingApi.create({ id: 'new-1', inventAccountType: 9,
    itemCode: 2, itemRelation: '', categoryRelation: '0', custVendCode: 2,
    custVendRelation: '', taxGroupId: '', ledgerDimension: '72', mainAccount: '' }))
    .rejects.toThrow('Duplicate posting rule.');
});
