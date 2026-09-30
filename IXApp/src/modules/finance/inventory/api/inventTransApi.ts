import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventTransRecord {
  id: string;
  recId: number;
  itemNumber: string;
  physicalDate: string | null;
  financialDate: string | null;
  reference: string;
  referenceNumber: string;
  inventTransId: string;
  receiptStatus: string;
  issueStatus: string;
  quantity: number;
  unitPrice: number;
  unitCost: number;
  costAmount: number;
  currencyCode: string;
  site: string;
  warehouse: string;
  batchNumber: string;
  serialNumber: string;
  expectedDate: string | null;
  voucher: string;
  dataAreaId: string;
}

type InventTransDto = Omit<InventTransRecord, 'id'>;

export const inventTransApi = {
  async list(signal?: AbortSignal): Promise<InventTransRecord[]> {
    const response = await apiClient.get<ApiResponse<InventTransDto[]>>('/v1/InventTrans/list', {
      signal,
    });
    if (!response.data.success || !Array.isArray(response.data.data)) {
      throw new ApiError(
        response.data.message || 'The inventory transaction response did not contain data.',
        500
      );
    }
    return response.data.data.map((transaction) => ({
      ...transaction,
      id: String(transaction.recId),
    }));
  },
};
