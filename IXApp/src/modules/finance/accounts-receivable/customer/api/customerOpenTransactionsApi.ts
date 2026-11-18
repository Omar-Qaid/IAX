import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface CustomerOpenTransaction {
  recId: number;
  invoice: string;
  voucher: string;
  transDate: string;
  dueDate: string;
  currencyCode: string;
  amountCur: number;
  settleAmountCur: number;
  openAmountCur: number;
  openAmountMst: number;
  daysOverdue: number;
}
export interface CustomerOpenBalance {
  accountNum: string;
  asOfDate: string;
  reconciled: boolean;
  transactions: CustomerOpenTransaction[];
}
export const customerOpenTransactionsApi = {
  async load(accountNum: string, signal?: AbortSignal): Promise<CustomerOpenBalance> {
    if (!accountNum.trim()) throw new ApiError('An account number is required.', 400);
    const { data } = await apiClient.get<ApiResponse<CustomerOpenBalance>>(
      '/v1/CustTrans/open-balance',
      { params: { accountNum }, signal }
    );
    if (!data.success || !data.data)
      throw new ApiError(data.message || 'Unable to load open transactions.', 500);
    return data.data;
  },
};
