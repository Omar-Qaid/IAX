import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { LookupOption, LookupPage } from '@shared/components/lookups/types';
export interface FreeTextInvoiceLine {
  recId: number;
  lineNum: number;
  description: string;
  quantity: number;
  unitPrice: number;
  netAmount: number;
  taxAmount: number;
  grossAmount: number;
  ledgerDimension: number;
  defaultDimension: number;
  taxGroup: string;
  taxItemGroup: string;
}
export interface FreeTextInvoice {
  recId: number;
  invoiceId: string;
  customerAccount: string;
  invoiceAccount: string;
  description: string;
  invoiceDate: string;
  dueDate: string;
  currencyCode: string;
  paymentTerms: string;
  paymentMethod: string;
  postingProfile: string;
  defaultDimension: number;
  taxGroup: string;
  posted: boolean;
  netAmount: number;
  taxAmount: number;
  invoiceAmount: number;
  lines: FreeTextInvoiceLine[];
}
export type FreeTextInvoiceLineInput = Pick<
  FreeTextInvoiceLine,
  'description' | 'quantity' | 'unitPrice' | 'ledgerDimension'
> & {
  defaultDimension?: number;
  taxGroup?: string;
  taxItemGroup?: string;
};
export interface FreeTextInvoiceInput {
  customerAccount: string;
  invoiceAccount?: string;
  invoiceDate?: string;
  currencyCode?: string;
  paymentTerms?: string;
  paymentMethod?: string;
  postingProfile?: string;
  defaultDimension?: number;
  taxGroup?: string;
  description?: string;
  lines: FreeTextInvoiceLineInput[];
}
export type FreeTextInvoiceRecord = FreeTextInvoice & { id: string };
const dataOf = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'Unable to load free text invoice.');
  return response.data;
};
export const freeTextInvoiceApi = {
  async headerLookups(signal?: AbortSignal) {
    const endpoints = ['/v1/Currency', '/v1/PaymTerm', '/v1/CustPaymMode', '/v1/TaxGroup'];
    const rows = await Promise.all(
      endpoints.map(async (endpoint) => {
        const value = dataOf(
          (await apiClient.get<ApiResponse<Record<string, unknown>[]>>(endpoint, { signal })).data
        );
        if (!Array.isArray(value)) throw new Error('Invalid invoice lookup response.');
        return value;
      })
    );
    const options = (items: Record<string, unknown>[], code: string, name: string) =>
      items.map((item) => ({
        value: String(item[code] ?? ''),
        label: String(item[name] ?? item[code] ?? ''),
      }));
    return {
      currencies: options(rows[0], 'currencyCode', 'txt'),
      paymentTerms: options(rows[1], 'paymTermId', 'description'),
      paymentMethods: options(rows[2], 'paymMode', 'name'),
      salesTaxGroups: options(rows[3], 'taxGroup', 'taxGroupName'),
    };
  },
  async taxGroups(signal?: AbortSignal) {
    return dataOf(
      (
        await apiClient.get<
          ApiResponse<{ salesTaxGroups: LookupOption[]; itemSalesTaxGroups: LookupOption[] }>
        >('/v1/CustInvoiceTable/tax-groups', { signal })
      ).data
    );
  },
  async ledgerDimensions({
    pageNumber,
    pageSize,
    search,
    signal,
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return dataOf(
      (
        await apiClient.get<ApiResponse<LookupPage<LookupOption>>>(
          '/v1/CustInvoiceTable/ledger-dimensions',
          { params: { pageNumber, pageSize, search }, signal }
        )
      ).data
    );
  },
  async get(recId: number, signal?: AbortSignal): Promise<FreeTextInvoiceRecord> {
    const row = dataOf(
      (
        await apiClient.get<ApiResponse<FreeTextInvoice>>(`/v1/CustInvoiceTable/${recId}`, {
          signal,
        })
      ).data
    );
    return { ...row, id: String(row.recId) };
  },
  async save(input: FreeTextInvoiceInput, recId?: number): Promise<FreeTextInvoiceRecord> {
    const response = recId
      ? await apiClient.put<ApiResponse<FreeTextInvoice>>(`/v1/CustInvoiceTable/${recId}`, input)
      : await apiClient.post<ApiResponse<FreeTextInvoice>>('/v1/CustInvoiceTable', input);
    const row = dataOf(response.data);
    return { ...row, id: String(row.recId) };
  },
  async list(signal?: AbortSignal): Promise<FreeTextInvoiceRecord[]> {
    const rows = dataOf(
      (await apiClient.get<ApiResponse<FreeTextInvoice[]>>('/v1/CustInvoiceTable', { signal })).data
    );
    if (!Array.isArray(rows))
      throw new Error('The free text invoice response did not contain a list.');
    return rows.map((row) => ({ ...row, id: String(row.recId) }));
  },
  async post(recId: number): Promise<FreeTextInvoice> {
    return dataOf(
      (await apiClient.post<ApiResponse<FreeTextInvoice>>(`/v1/CustInvoiceTable/${recId}/post`))
        .data
    );
  },
  async remove(recId: number): Promise<void> {
    const response = (await apiClient.delete<ApiResponse<unknown>>(`/v1/CustInvoiceTable/${recId}`))
      .data;
    if (!response.success)
      throw new Error(response.message || 'Unable to delete free text invoice.');
  },
};
