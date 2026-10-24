import { expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { inventSiteApi, type InventSiteRecord } from '@modules/finance/inventory/api/inventSiteApi';
import { inventLocationApi, type InventLocationRecord } from '@modules/finance/inventory/api/inventLocationApi';

vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));

it('keeps a site when the delete response says it was not deleted', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  const site = { recId: 1 } as InventSiteRecord;
  await expect(inventSiteApi.delete(site)).rejects.toThrow('could not be deleted');
});

it('keeps a warehouse when the delete response says it was not deleted', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  const warehouse = { recId: 2 } as InventLocationRecord;
  await expect(inventLocationApi.delete(warehouse)).rejects.toThrow('could not be deleted');
});
