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

export const inventOnHandApi = {
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
