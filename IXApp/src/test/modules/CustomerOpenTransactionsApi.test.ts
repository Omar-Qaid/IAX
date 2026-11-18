import {beforeEach, describe, expect, it, vi} from 'vitest';
import {apiClient} from '@core/api/apiClient';
import {customerOpenTransactionsApi} from '@modules/finance/accounts-receivable/customer/api/customerOpenTransactionsApi';
vi.mock('@core/api/apiClient', () => ({apiClient: {get: vi.fn()}}));
beforeEach(() => vi.mocked(apiClient.get).mockReset());
describe('customer open transactions API', () => {
  it('passes the exact selected account and cancellation signal without combining currencies', async () => {
    const signal = new AbortController().signal;
    const result = {accountNum: 'C/001', asOfDate: '2026-11-15', reconciled: true, transactions: []};
    vi.mocked(apiClient.get).mockResolvedValue({data: {success: true, data: result}});
    expect(await customerOpenTransactionsApi.load('C/001', signal)).toBe(result);
    expect(apiClient.get).toHaveBeenCalledWith('/v1/CustTrans/open-balance', {params: {accountNum: 'C/001'}, signal});
  });
  it('refuses an empty account instead of asking for every customer', async () => {
    await expect(customerOpenTransactionsApi.load(' ')).rejects.toThrow('An account number is required.');
    expect(apiClient.get).not.toHaveBeenCalled();
  });
  it('surfaces API failures instead of displaying a successful empty balance', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({data: {success: false, message: 'Access denied'}});
    await expect(customerOpenTransactionsApi.load('C-001')).rejects.toThrow('Access denied');
  });
});
