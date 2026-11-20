import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface CustPaymentBankAccount {
  accountId: string;
  name: string;
  currencyCode: string;
}

export interface CustPaymentPostRequest {
  custAccount: string;
  bankAccountId: string;
  amount: number;
  paymentDate: string;
  paymMode?: string;
  /** An invoice to settle; omitted, the server settles the oldest open invoices. */
  settleInvoiceId?: string;
  settleAutomatically: boolean;
  prepayment: boolean;
  requestId: string;
}

export interface CustPaymentPostResult {
  journalNum: string;
  voucher: string;
  amount: number;
  /** What the payment could not settle and stays open on the customer. */
  unsettled?: number;
  settled?: number;
}

function unwrap<T>(response: ApiResponse<T>, fallback: string): T {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || fallback, 500);
  return response.data;
}

export const custPaymentApi = {
  async bankAccounts(signal?: AbortSignal): Promise<CustPaymentBankAccount[]> {
    const { data } = await apiClient.get<ApiResponse<CustPaymentBankAccount[]>>(
      '/v1/customer-payments/bank-accounts',
      { signal }
    );
    return unwrap(data, 'Unable to load bank accounts.');
  },
  async post(request: CustPaymentPostRequest): Promise<CustPaymentPostResult> {
    const { data } = await apiClient.post<ApiResponse<CustPaymentPostResult>>(
      '/v1/customer-payments',
      request
    );
    return unwrap(data, 'The customer payment could not be posted.');
  },
};
