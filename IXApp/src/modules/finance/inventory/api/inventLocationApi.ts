import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { LookupOption } from '@shared/components/lookups/types';
export interface InventLocationRecord {
  id: string;
  recId: number;
  inventLocationId: string;
  name: string;
  inventSiteId: string;
  inventLocationType: number;
  inventLocationLevel: number;
  inventLocationIdTransit: string;
  inventLocationIdQuarantine: string;
  inventLocationIdReqMain: string;
  itmInventLocationIdGit: string;
  itmInventLocationIdUnder: string;
  vendAccount: string;
  workflowApproval: boolean;
  manual: boolean;
  reqRefill: boolean;
  wmsLocationIdDefaultReceipt: string;
  wmsLocationIdDefaultIssue: string;
  defaultProductionInputLocation: string;
  defaultProductionFinishGoodsLocation: string;
  defaultKanbanFinishedGoodsLocation: string;
  defaultReturnCreditOnlyLocation: string;
  defaultStatusId: string;
  wmsRackFormat: string;
  wmsLevelFormat: string;
  wmsPositionFormat: string;
  whsEnabled: boolean;
  warehouseAutoReleaseReservation: boolean;
  autoUpdateShipment: boolean;
  reserveAtLoadPost: boolean;
  decrementLoadLine: boolean;
  printBolBeforeShipConfirm: boolean;
  cycleCountAllowPalletMove: boolean;
  allowLaborStandards: boolean;
  allowMarkingReservationRemoval: boolean;
  useWmsOrders: boolean;
  wmsAisleNameActive: boolean;
  wmsRackNameActive: boolean;
  wmsLevelNameActive: boolean;
  wmsPositionNameActive: boolean;
  uniqueCheckDigits: boolean;
  enableQualityManagement: boolean;
  removeInventBlockingOnStatusChange: boolean;
  prodReserveOnlyWhse: boolean;
  whsProdOrderBackflushMustUseReservedQty: boolean;
  fshStore: boolean;
  consolidateShipAtRtw: boolean;
  retailInventNegPhysical: boolean;
  retailInventNegFinancial: boolean;
  enableExternalWarehouse: boolean;
  maxPickingRouteTime: number;
  pickingLineTime: number;
}
type LocationLookup = LookupOption & { siteId?: string };
const endpoint = '/v1/InventLocation';
const data = <T>(r: ApiResponse<T>): T => {
  if (!r.success || r.data == null)
    throw new ApiError(r.message || 'Warehouse request failed.', 500);
  return r.data;
};
const payload = ({ id: _id, recId: _recId, ...record }: InventLocationRecord) => record;
export const inventLocationApi = {
  async list(signal?: AbortSignal) {
    return data(
      (await apiClient.get<ApiResponse<InventLocationRecord[]>>(endpoint, { signal })).data
    );
  },
  async lookups(signal?: AbortSignal) {
    return data(
      (
        await apiClient.get<ApiResponse<{ sites: LookupOption[]; warehouses: LocationLookup[] }>>(
          `${endpoint}/lookups`,
          { signal }
        )
      ).data
    );
  },
  async create(record: InventLocationRecord) {
    return data(
      (await apiClient.post<ApiResponse<InventLocationRecord>>(endpoint, payload(record))).data
    );
  },
  async update(record: InventLocationRecord) {
    return data(
      (
        await apiClient.put<ApiResponse<InventLocationRecord>>(
          `${endpoint}/${record.recId}`,
          payload(record)
        )
      ).data
    );
  },
  async delete(record: InventLocationRecord) {
    data((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`)).data);
  },
};
