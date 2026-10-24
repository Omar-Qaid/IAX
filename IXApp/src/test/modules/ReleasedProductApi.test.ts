import { beforeEach, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { releasedProductApi } from '@modules/finance/inventory/api/releasedProductApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));
beforeEach(() => vi.clearAllMocks());

it('rejects a barcode deletion that the server did not perform', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(releasedProductApi.deleteBarcode('42', 7)).rejects.toThrow('Barcode could not be deleted.');
});

it('releases the shared product to the explicitly selected company', async () => {
  const input = { productId: '123', itemNumber: 'PAPER', nameAlias: '' };
  const record = { id: '456', itemNumber: 'PAPER' };
  vi.mocked(apiClient.post).mockResolvedValue({ data: { success: true, data: record } });
  expect(await releasedProductApi.releaseToCompany(input, 'DEMO')).toEqual(record);
  expect(apiClient.post).toHaveBeenCalledWith('/v1/InventTable/released-products/release', input,
    { headers: { 'X-Company': 'DEMO' } });
});

it('preserves leading zeros and represented quantity when creating a barcode', async () => {
  const input = { itemBarcode: '001234', unitId: 'BOX', qty: 2, description: '', inventDimId: '',
    useForInput: true, useForPrinting: true, blocked: false };
  vi.mocked(apiClient.post).mockResolvedValue({ data: { success: true, data: input } });
  expect((await releasedProductApi.createBarcode('42', input)).itemBarcode).toBe('001234');
  expect(apiClient.post).toHaveBeenCalledWith('/v1/InventTable/released-products/42/barcodes', input);
});

it('returns invalid readiness results as data for the user to correct', async () => {
  const validation = { isValid: false, errors: ['Inventory unit is required.'], warnings: [] };
  const signal = new AbortController().signal;
  vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: validation } });
  expect(await releasedProductApi.validate('42', signal)).toEqual(validation);
  expect(apiClient.get).toHaveBeenCalledWith('/v1/InventTable/released-products/42/validate', { signal });
});

it('rejects malformed product lists and preserves release validation errors', async () => {
  vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: null } });
  await expect(releasedProductApi.list()).rejects.toThrow('did not contain data');
  vi.mocked(apiClient.post).mockResolvedValue({ data: { success: false, message: 'Item already released.' } });
  await expect(releasedProductApi.release({ productId: '123', itemNumber: 'PAPER', nameAlias: '' }))
    .rejects.toThrow('Item already released.');
});
