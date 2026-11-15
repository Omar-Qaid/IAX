import { describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { customerPaymentSetupApi } from '@modules/finance/accounts-receivable/api/customerPaymentSetupApi';
vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), put: vi.fn(), post: vi.fn(), delete: vi.fn() },
}));
describe.each(['terms', 'methods'] as const)('customer payment %s adapter', (kind) => {
  it('round-trips unedited stored fields while persisting basic edits', async () => {
    const term = kind === 'terms';
    const endpoint = term ? '/v1/PaymTerm' : '/v1/CustPaymMode';
    const dto = {
      recId: 12,
      [term ? 'paymTermId' : 'paymMode']: 'NET30',
      [term ? 'description' : 'name']: 'Existing',
      [term ? 'numOfDays' : 'discGraceDays']: 30,
      numOfMonths: 2,
      cashLedgerDimension: 987,
      customerUpdateDueDate: 1,
    };
    vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [dto] } });
    vi.mocked(apiClient.put).mockImplementation(async (_path, body) => ({
      data: { success: true, data: body },
    }));
    const api = customerPaymentSetupApi(kind);
    const [record] = await api.list();
    await api.update({ ...record, days: 45, description: 'Updated' });
    expect(apiClient.put).toHaveBeenLastCalledWith(endpoint + '/12', {
      ...dto,
      [term ? 'numOfDays' : 'discGraceDays']: 45,
      [term ? 'description' : 'name']: 'Updated',
    });
  });
});
