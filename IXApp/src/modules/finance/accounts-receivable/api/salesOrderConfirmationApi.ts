import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface SalesOrderConfirmationSummary {
  id: string;
  confirmId: string;
  confirmDate: string;
  currencyCode: string;
  qty: number;
  salesBalance: number;
  sumMarkup: number;
  sumTax: number;
  confirmAmount: number;
}

export interface SalesOrderConfirmationLine {
  id: string;
  lineNum: number;
  itemId: string;
  name: string;
  qty: number;
  salesUnit: string;
  salesPrice: number;
  lineAmount: number;
  taxAmount: number;
}

export interface SalesOrderConfirmationDetail {
  header: SalesOrderConfirmationSummary & {
    salesId: string;
    orderAccount: string;
    invoiceAccount: string;
    payment: string;
    deliveryName: string;
    deliveryPostalAddress: string;
    dlvMode: string;
    dlvTerm: string;
    sumLineDisc: number;
    endDisc: number;
  };
  lines: SalesOrderConfirmationLine[];
}

function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'The sales order confirmation request failed.');
  return response.data;
}

export const salesOrderConfirmationApi = {
  async list(orderId: string, signal?: AbortSignal): Promise<SalesOrderConfirmationSummary[]> {
    return unwrap((await apiClient.get<ApiResponse<SalesOrderConfirmationSummary[]>>(
      `/v1/SalesTable/${encodeURIComponent(orderId)}/confirmations`, { signal }
    )).data);
  },
  async get(orderId: string, confirmationId: string, signal?: AbortSignal): Promise<SalesOrderConfirmationDetail> {
    return unwrap((await apiClient.get<ApiResponse<SalesOrderConfirmationDetail>>(
      `/v1/SalesTable/${encodeURIComponent(orderId)}/confirmations/${encodeURIComponent(confirmationId)}`, { signal }
    )).data);
  },
  async post(orderId: string, confirmationDate: string, requestId: string): Promise<SalesOrderConfirmationDetail['header']> {
    return unwrap((await apiClient.post<ApiResponse<SalesOrderConfirmationDetail['header']>>(
      `/v1/SalesTable/${encodeURIComponent(orderId)}/confirmations`,
      { confirmationDate, posting: true, requestId }
    )).data);
  },
};
