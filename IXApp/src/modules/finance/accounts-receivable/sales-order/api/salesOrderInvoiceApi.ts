import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

/** What the server returns for a posted sales invoice (`CustInvoiceJour` summary). */
export interface SalesInvoicePostResult {
  invoiceId: string;
  salesId: string;
  invoiceDate: string;
  qty: number;
  salesBalance: number;
  sumTax: number;
  invoiceAmount: number;
}

export interface SalesInvoicePostRequest {
  invoiceDate: string;
  requestId: string;
  /** Omitted or empty invoices the whole remaining financial quantity of every line. */
  lines: { salesLineRecId: number; qty: number }[];
}

function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'The sales invoice request failed.');
  return response.data;
}

export const salesOrderInvoiceApi = {
  async post(
    salesOrderId: string,
    request: SalesInvoicePostRequest
  ): Promise<SalesInvoicePostResult> {
    return unwrap(
      (
        await apiClient.post<ApiResponse<SalesInvoicePostResult>>(
          `/v1/SalesTable/${encodeURIComponent(salesOrderId)}/invoices`,
          request
        )
      ).data
    );
  },
};
