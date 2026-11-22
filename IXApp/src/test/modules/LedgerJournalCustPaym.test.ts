import { describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import {
  ledgerJournalCustPaymApi as api,
  newPaymentJournal,
  newPaymentJournalLine,
  paymentJournalTotals,
  sameAmount,
} from '@modules/finance/accounts-receivable/payment/api/ledgerJournalCustPaymApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

describe('customer payment journal API', () => {
  it('only lists customer payment journals and refuses other journal types on direct routes', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({
      data: {
        success: true,
        data: [
          { recId: 1, journalType: 0 },
          { recId: 2, journalType: 1 },
        ],
      },
    });
    expect(await api.list()).toEqual([{ recId: 2, id: '2', journalType: 1 }]);
    vi.mocked(apiClient.get).mockResolvedValueOnce({
      data: { success: true, data: { recId: 1, journalType: 0 } },
    });
    await expect(api.get('1')).rejects.toThrow('not a customer payment journal');
  });
  it('saves the full line and dimension ID without posting or losing hidden fields', async () => {
    const line = {
      ...newPaymentJournalLine(newPaymentJournal('usmf')),
      recId: 8,
      id: '8',
      offsetLedgerDimension: 412,
      ledgerDimensionName: 'C-001',
      rowVersion: 'abc',
      taxGroup: 'KEEP',
    };
    vi.mocked(apiClient.put).mockResolvedValueOnce({ data: { success: true, data: line } });
    await api.lines.update(line);
    expect(apiClient.put).toHaveBeenCalledWith(
      '/v1/LedgerJournalTrans/8',
      expect.objectContaining({
        offsetLedgerDimension: 412,
        rowVersion: 'abc',
        taxGroup: 'KEEP',
        dataAreaId: 'usmf',
      })
    );
    expect(apiClient.post).not.toHaveBeenCalled();
  });
  it('loads only the opened journal lines from the server, in the journal scope', async () => {
    vi.mocked(apiClient.get).mockResolvedValueOnce({
      data: { success: true, data: [{ recId: 5, journalNum: 'PAY/01', lineNum: 1 }] },
    });
    expect(await api.journalLines('PAY/01')).toEqual([
      { recId: 5, id: '5', journalNum: 'PAY/01', lineNum: 1 },
    ]);
    expect(apiClient.get).toHaveBeenLastCalledWith(
      '/v1/LedgerJournalTrans/by-journal/PAY%2F01',
      expect.anything()
    );
  });
  it('falls back to loading all journals and lines on an API without the scoped endpoints', async () => {
    // Same shape as the API client's ApiError.
    const notFound = Object.assign(new Error('Not found'), { status: 404 });
    vi.mocked(apiClient.get)
      .mockRejectedValueOnce(notFound)
      .mockResolvedValueOnce({
        data: {
          success: true,
          data: [
            { recId: 1, journalType: 0 },
            { recId: 2, journalType: 1 },
          ],
        },
      });
    expect(await api.list()).toEqual([{ recId: 2, id: '2', journalType: 1 }]);
    vi.mocked(apiClient.get)
      .mockRejectedValueOnce(notFound)
      .mockResolvedValueOnce({
        data: {
          success: true,
          data: [
            { recId: 5, journalNum: 'PAY-1' },
            { recId: 6, journalNum: 'PAY-2' },
          ],
        },
      });
    expect(await api.journalLines('PAY-1')).toEqual([{ recId: 5, id: '5', journalNum: 'PAY-1' }]);
  });
  it('matches invoice amounts to the cent, ignoring floating point noise', () => {
    expect(sameAmount(100.1, 100.10000000001)).toBe(true);
    expect(sameAmount(0.1 + 0.2, 0.3)).toBe(true);
    expect(sameAmount(100.1, 100.11)).toBe(false);
  });
  it('reads the existing unwrapped validation response', async () => {
    const validation = {
      valid: false,
      errors: ['Period is closed'],
      unverifiedChecks: [],
      scope: 'Static checks',
    };
    vi.mocked(apiClient.get).mockResolvedValueOnce({ data: validation });
    expect(await api.validate(8)).toEqual(validation);
  });
  it('includes offset sides in totals, matching the reference vouchers', () => {
    const base = newPaymentJournalLine(newPaymentJournal('hbmc'));
    const rows = [
      { ...base, voucher: 'V1', amountCurDebit: 3375, offsetLedgerDimension: 1 },
      { ...base, voucher: 'V2', amountCurCredit: 3375, offsetLedgerDimension: 1 },
    ];
    expect(paymentJournalTotals(rows.slice(0, 1))).toEqual({
      debit: 3375,
      credit: 3375,
      balance: 0,
    });
    expect(paymentJournalTotals(rows)).toEqual({ debit: 6750, credit: 6750, balance: 0 });
    expect(paymentJournalTotals([{ ...base, amountCurCredit: 100 }])).toEqual({
      debit: 0,
      credit: 100,
      balance: -100,
    });
  });
});
