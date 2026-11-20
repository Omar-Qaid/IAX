import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface CustOpenTransRow {
  recId: number;
  transRecId: number;
  invoice: string;
  voucher: string;
  description: string;
  currencyCode: string;
  paymentMode: string;
  paymentReference: string;
  paymentTerms: string;
  cashDiscCode: string;
  worker: string;
  department: string;
  costCenter: string;
  project: string;
  businessUnit: string;
  transDate: string;
  dueDate: string;
  cashDiscDate: string;
  lastSettleDate: string;
  amountCur: number;
  amountMst: number;
  originalAmountCur: number;
  originalAmountMst: number;
  settledAmountMst: number;
  settledAmountCur: number;
  possibleCashDisc: number;
  cashDiscTaken: number;
  useCashDisc: number;
  prepayment: boolean;
}
export interface CustOpenTransData {
  accountNum: string;
  customerName: string;
  dataAreaId: string;
  customerCurrency: string;
  accountingCurrency: string;
  customerBalanceMst: number;
  reconciled: boolean;
  transactions: CustOpenTransRow[];
}
export interface SettlementResult {
  settlementVoucher: string;
  custAccount: string;
  currency: string;
  settled: number;
}
export const custOpenTransApi = {
  async load(accountNum: string, signal?: AbortSignal): Promise<CustOpenTransData> {
    if (!accountNum.trim()) throw new ApiError('A customer account is required.', 400);
    const { data } = await apiClient.get<ApiResponse<CustOpenTransData>>(
      '/v1/customer-settlements/open-transactions',
      { params: { accountNum }, signal }
    );
    if (!data.success || !data.data)
      throw new ApiError(data.message || 'Unable to load open transactions.', 500);
    return data.data;
  },
  async post(
    custAccount: string,
    settleDate: string,
    transRecIds: number[]
  ): Promise<SettlementResult> {
    const { data } = await apiClient.post<ApiResponse<SettlementResult>>(
      '/v1/customer-settlements/settle-open-transactions',
      { custAccount, settleDate, transRecIds }
    );
    if (!data.success || !data.data) throw new ApiError(data.message || 'Settlement failed.', 500);
    return data.data;
  },
};
