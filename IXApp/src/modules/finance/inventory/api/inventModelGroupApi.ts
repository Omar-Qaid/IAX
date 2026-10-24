import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventModelGroupRecord {
  id: string;
  modelGroupId: string;
  name: string;
  inventModel: number;
  stockedProduct: boolean;
  postOnHandPhysical: boolean;
  postOnHandFinancial: boolean;
  postPhysicalRevenue: boolean;
  postPhysicalPurchase: boolean;
  standardCost: boolean;
  mandatoryRegister: boolean;
  mandatoryReceive: boolean;
  mandatoryPick: boolean;
  mandatoryDeduct: boolean;
  mandatoryWmsOrder: boolean;
  negativePhysical: boolean;
  negativeFinancial: boolean;
  quarantineControl: boolean;
  inclPhysicalValueInCost: boolean;
  includePhysicalValueForAverage: boolean;
  itemProdReservationActive: boolean;
  itemProdReservation: number;
  reserveByDate: boolean;
  reserveReversed: boolean;
  mcrReservation: boolean;
  pdsDispositionCode: string;
  pdsCheck: boolean;
  pdsConsReq: boolean;
  pdsReqVendBatchDetail: boolean;
  pdsSameLot: boolean;
  pdsVendorCheckItem: boolean;
  pdsPickCriteria: number;
  pickingListBatchExpirationDateValidationRule: number;
}

const endpoint = '/v1/InventModelGroup';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'Item model group request failed.', 400);
  return response.data;
};
export const inventModelGroupApi = {
  async list(signal?: AbortSignal): Promise<InventModelGroupRecord[]> {
    return requireData(
      (await apiClient.get<ApiResponse<InventModelGroupRecord[]>>(endpoint, { signal })).data
    );
  },
  async create(input: InventModelGroupRecord): Promise<InventModelGroupRecord> {
    return requireData(
      (await apiClient.post<ApiResponse<InventModelGroupRecord>>(endpoint, input)).data
    );
  },
  async update(input: InventModelGroupRecord): Promise<InventModelGroupRecord> {
    return requireData(
      (
        await apiClient.put<ApiResponse<InventModelGroupRecord>>(
          `${endpoint}/${encodeURIComponent(input.id)}`,
          input
        )
      ).data
    );
  },
  async delete(input: InventModelGroupRecord): Promise<void> {
    if (requireData(
      (await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(input.id)}`))
        .data
    ) !== true) throw new ApiError('The item model group could not be deleted.', 400);
  },
};
