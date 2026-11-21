import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

/** One posted ledger line of the voucher transactions inquiry. */
export interface LedgerTransVoucherRecord {
  recId: number;
  journalNumber: string;
  voucher: string;
  transDate: string;
  yearClosed: boolean;
  ledgerAccount: string;
  accountName: string;
  description: string;
  currencyCode: string;
  amountCur: number;
  amountMst: number;
  postingType: string;
}

export interface LedgerTransVoucherQuery {
  voucher?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export const ledgerTransVoucherApi = {
  async load(
    query: LedgerTransVoucherQuery,
    signal?: AbortSignal
  ): Promise<LedgerTransVoucherRecord[]> {
    const { data } = await apiClient.get<ApiResponse<LedgerTransVoucherRecord[]>>(
      '/v1/LedgerTransVoucher',
      {
        params: {
          voucher: query.voucher || undefined,
          fromDate: query.fromDate || undefined,
          toDate: query.toDate || undefined,
        },
        signal,
      }
    );
    if (!data.success || !data.data)
      throw new ApiError(data.message || 'Unable to load voucher transactions.', 500);
    return data.data;
  },
};
