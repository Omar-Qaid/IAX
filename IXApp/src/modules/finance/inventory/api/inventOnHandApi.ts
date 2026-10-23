import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventOnHandRecord {
  id: string;
  recId: number;
  itemNumber: string;
  productName: string;
  inventoryUnit: string;
  inventDimId: string;
  site: string;
  warehouse: string;
  physicalInventory: number;
  physicalReserved: number;
  availablePhysical: number;
  orderedReceipts: number;
  orderedIssues: number;
  availableOrdered: number;
  postedValue: number;
  physicalValue: number;
}

export interface InventOnHandPage {
  page: number;
  pageSize: number;
  totalCount: number;
  rows: InventOnHandRecord[];
}

export interface InventOnHandFilters {
  itemId: string;
  itemGroupId: string;
  siteId: string;
  warehouseId: string;
  includeZero: boolean;
}

export const inventOnHandApi = {
  async page(filters: InventOnHandFilters, page: number, pageSize: number, signal?: AbortSignal): Promise<InventOnHandPage> {
    const response = await apiClient.get<ApiResponse<InventOnHandPage>>('/v1/InventOnHand/paged', {
      params: { ...filters, page, pageSize },
      signal,
    });
    const result = response.data.data;
    if (!response.data.success || !result || !Array.isArray(result.rows)) {
      throw new ApiError(response.data.message || 'The on-hand inventory response did not contain a page.', 500);
    }
    return result;
  },
  async list(signal?: AbortSignal): Promise<InventOnHandRecord[]> {
    const response = await apiClient.get<ApiResponse<InventOnHandRecord[]>>('/v1/InventOnHand', {
      signal,
    });
    if (!response.data.success || !Array.isArray(response.data.data)) {
      throw new ApiError(
        response.data.message || 'The on-hand inventory response did not contain data.',
        500
      );
    }
    return response.data.data;
  },
};
