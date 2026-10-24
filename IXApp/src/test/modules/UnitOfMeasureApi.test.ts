import { expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { unitOfMeasureApi } from '@modules/finance/inventory/api/unitOfMeasureApi';

vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));

it('rejects a delete response when the server reports no unit was deleted', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(unitOfMeasureApi.delete({ id: '1', recId: 1, symbol: 'EA', unitOfMeasureClass: 0,
    systemOfUnits: 0, decimalPrecision: 0, conversions: [] })).rejects.toThrow('could not be deleted');
});

it('rejects a delete response when the conversion remains', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(unitOfMeasureApi.deleteConversion(7)).rejects.toThrow('could not be deleted');
});
