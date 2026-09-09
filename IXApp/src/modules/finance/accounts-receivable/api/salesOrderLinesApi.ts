import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface SalesOrderLineRecord {
  id: string;
  lineNumber: number;
  itemNumber: string;
  description: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  lineTotal: number;
  taxAmount?: number | null;
  itemType?: string;
  arabicName?: string;
  salesCategory?: number;
  lineType?: number;
  preRelatedInvoices?: string;
  deliveryType?: number;
  site?: string;
  warehouse?: string;
  deliveryDate?: string;
}
export interface SalesUnit {
  symbol: string;
}
export interface SalesItem {
  itemNumber: string;
  name: string;
  unit?: string;
  unitPrice?: number;
  itemType?: string;
}
function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'Unable to load sales order data.');
  return response.data;
}
export const salesOrderLinesApi = {
  async list(id: string, signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<SalesOrderLineRecord[]>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines`,
          { signal }
        )
      ).data
    );
  },
  async add(
    id: string,
    input: {
      itemNumber: string;
      quantity: number;
      unitPrice: number;
      description?: string;
      unit?: string;
      deliveryDate?: string;
      salesCategory?: number;
      lineType?: number;
      deliveryType?: number;
    }
  ) {
    return unwrap(
      (
        await apiClient.post<ApiResponse<SalesOrderLineRecord>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines`,
          input
        )
      ).data
    );
  },
  async update(id: string, line: SalesOrderLineRecord) {
    return unwrap(
      (
        await apiClient.put<ApiResponse<SalesOrderLineRecord>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines/${encodeURIComponent(line.id)}`,
          line
        )
      ).data
    );
  },
  async remove(id: string, lineId: string) {
    return unwrap(
      (
        await apiClient.delete<ApiResponse<{ deleted: boolean }>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines/${encodeURIComponent(lineId)}`
        )
      ).data
    );
  },
  async units({
    signal,
    ...params
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            data: SalesUnit[];
            pageNumber: number;
            totalPages: number;
            totalRecords: number;
          }>
        >('/v1/SalesTable/units', { params, signal })
      ).data
    );
  },
  async items({
    signal,
    ...params
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            data: SalesItem[];
            pageNumber: number;
            totalPages: number;
            totalRecords: number;
          }>
        >('/v1/SalesTable/items', { params, signal })
      ).data
    );
  },
};
