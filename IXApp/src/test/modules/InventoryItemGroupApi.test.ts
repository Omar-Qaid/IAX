import { expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { inventItemGroupApi, type InventItemGroupRecord } from '@modules/finance/inventory/api/inventItemGroupApi';

vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));

it('reports a failed item group deletion when the server returns false', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(inventItemGroupApi.delete({ id: '9' } as InventItemGroupRecord)).rejects.toThrow('could not be deleted');
});
