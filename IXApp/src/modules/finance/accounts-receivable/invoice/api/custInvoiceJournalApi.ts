import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface CustInvoiceJournalRecord {
  recId: number;
  invoiceId: string;
  invoiceAccount: string;
  invoicingName: string;
  salesId: string;
  invoiceDate: string;
  ledgerVoucher: string;
  currencyCode: string;
  invoiceAmount: number;
  intercompanyCompanyId: string;
  intercompanyPosted: number;
  eInvoiceLineSpecific: number;
}

export const custInvoiceJournalApi = {
  async load(signal?: AbortSignal): Promise<CustInvoiceJournalRecord[]> {
    const { data } = await apiClient.get<ApiResponse<CustInvoiceJournalRecord[]>>(
      '/v1/CustInvoiceJour',
      { signal }
    );
    if (!data.success || !data.data)
      throw new ApiError(data.message || 'Unable to load invoice journal.', 500);
    return data.data;
  },
};
