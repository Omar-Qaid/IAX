import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface ReturnDispositionCode {
  dispositionCodeId: string;
  description: string;
  dispositionAction: number;
}

export interface ReturnReasonCode {
  reasonCodeId: string;
  description: string;
  reasonCodeGroupId: string;
}

export interface CustReturnCreateRequest {
  invoiceId: string;
  returnReasonCodeId?: string;
  /** One entry per invoice line being returned, keyed by its `CustInvoiceTrans` record. */
  lines: { custInvoiceTransRecId: number; qty: number; dispositionCodeId: string }[];
}

export interface CustReturnCreateResult {
  recId: number;
  salesId: string;
  returnItemNum: string;
}

export interface CustReturnReceiptResult {
  packingSlipId: string;
  qty: number;
}

export interface CustCreditNoteResult {
  invoiceId: string;
  creditAmount: number;
  revenue: number;
  tax: number;
}

function unwrap<T>(response: ApiResponse<T>, fallback: string): T {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || fallback, 500);
  return response.data;
}

const list = async <T>(path: string, what: string, signal?: AbortSignal): Promise<T[]> => {
  const { data } = await apiClient.get<ApiResponse<T[]>>(path, { signal });
  return unwrap(data, `Unable to load ${what}.`);
};

export const custReturnApi = {
  dispositionCodes: (signal?: AbortSignal) =>
    list<ReturnDispositionCode>('/v1/ReturnDispositionCode', 'disposition codes', signal),
  reasonCodes: (signal?: AbortSignal) =>
    list<ReturnReasonCode>('/v1/ReturnReasonCode', 'return reason codes', signal),

  async create(request: CustReturnCreateRequest): Promise<CustReturnCreateResult> {
    const { data } = await apiClient.post<ApiResponse<CustReturnCreateResult>>(
      '/v1/return-orders',
      request
    );
    return unwrap(data, 'The return order could not be created.');
  },

  /** Receives the returned goods, which posts a negative packing slip. */
  async receive(
    recId: number,
    request: {
      requestId: string;
      receiptDate: string;
      lines: { salesLineRecId: number; qty: number }[];
    }
  ): Promise<CustReturnReceiptResult> {
    const { data } = await apiClient.post<ApiResponse<CustReturnReceiptResult>>(
      `/v1/return-orders/${recId}/receipts`,
      request
    );
    return unwrap(data, 'The return receipt could not be posted.');
  },

  /** Invoices the received return, which posts the credit note. */
  async credit(
    recId: number,
    request: { invoiceDate: string; requestId: string }
  ): Promise<CustCreditNoteResult> {
    const { data } = await apiClient.post<ApiResponse<CustCreditNoteResult>>(
      `/v1/return-orders/${recId}/credit-notes`,
      request
    );
    return unwrap(data, 'The credit note could not be posted.');
  },
};
