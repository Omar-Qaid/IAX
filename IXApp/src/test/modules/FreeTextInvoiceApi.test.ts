import { describe, expect, it, vi } from 'vitest';
import { freeTextInvoiceApi } from '@modules/finance/accounts-receivable/free-text-invoice/api/freeTextInvoiceApi';
import { apiClient } from '@core/api/apiClient';
vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));
describe('free text invoice API', () => {
  it('loads a detail record and uses its database identity', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: true, data: { recId: 17, lines: [] } },
    });
    expect(await freeTextInvoiceApi.get(17)).toMatchObject({ id: '17', recId: 17 });
    expect(apiClient.get).toHaveBeenCalledWith('/v1/CustInvoiceTable/17', { signal: undefined });
  });
  it('creates and updates through the existing whole-document endpoints', async () => {
    const input = {
      customerAccount: 'C-001',
      lines: [{ description: 'Consulting', quantity: 2, unitPrice: 100, ledgerDimension: 7 }],
    };
    vi.mocked(apiClient.post).mockResolvedValue({
      data: { success: true, data: { recId: 18, lines: [] } },
    });
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { success: true, data: { recId: 18, lines: [] } },
    });
    expect(await freeTextInvoiceApi.save(input)).toMatchObject({ id: '18' });
    await freeTextInvoiceApi.save(input, 18);
    expect(apiClient.post).toHaveBeenCalledWith('/v1/CustInvoiceTable', input);
    expect(apiClient.put).toHaveBeenCalledWith('/v1/CustInvoiceTable/18', input);
  });
  it('surfaces server validation without treating a refused save as successful', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({
      data: { success: false, message: 'Ledger account not found' },
    });
    await expect(
      freeTextInvoiceApi.save({ customerAccount: 'C-001', lines: [] }, 18)
    ).rejects.toThrow('Ledger account not found');
  });
  it('uses CustInvoiceTable and preserves actual amounts, posted state and stable IDs', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: true, data: [{ recId: 17, posted: false, invoiceAmount: 2300, lines: [] }] },
    });
    expect((await freeTextInvoiceApi.list())[0]).toMatchObject({
      id: '17',
      posted: false,
      invoiceAmount: 2300,
    });
    expect(apiClient.get).toHaveBeenCalledWith('/v1/CustInvoiceTable', { signal: undefined });
  });
  it('rejects failed envelopes even if HTTP succeeded', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: false, message: 'Not available', data: [] },
    });
    await expect(freeTextInvoiceApi.list()).rejects.toThrow('Not available');
  });
  it('posts through the existing explicit posting endpoint', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({
      data: { success: true, data: { recId: 17, posted: true } },
    });
    expect(await freeTextInvoiceApi.post(17)).toMatchObject({ posted: true });
    expect(apiClient.post).toHaveBeenCalledWith('/v1/CustInvoiceTable/17/post');
  });
  it('surfaces a draft deletion refusal', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({
      data: { success: false, message: 'Posted invoice cannot be deleted' },
    });
    await expect(freeTextInvoiceApi.remove(17)).rejects.toThrow('Posted invoice cannot be deleted');
  });
});
