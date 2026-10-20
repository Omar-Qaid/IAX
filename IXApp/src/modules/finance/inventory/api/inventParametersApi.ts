import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventParametersRecord {
  recId: number;
  defaultUnitId: string;
  multiSiteDefaultInventSiteId: string;
  multiSiteDefaultInventLocationId: string;
  movementJournalNameId: string;
  lossProfitJournalNameId: string;
  transferJournalNameId: string;
  countJournalNameId: string;
  reserveOnOrdered: boolean;
  mandatoryItemTaxGroup: boolean;
}

export interface InventParametersLookup { id: string; name: string; }
export interface InventParametersWarehouseLookup extends InventParametersLookup { siteId: string; }
export interface InventParametersJournalLookup extends InventParametersLookup { journalType: number; }
export interface InventParametersLookups {
  units: InventParametersLookup[];
  sites: InventParametersLookup[];
  warehouses: InventParametersWarehouseLookup[];
  journalNames: InventParametersJournalLookup[];
}

const endpoint = '/v1/InventParameters';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The inventory parameters response did not contain data.', 500);
  return response.data;
};

export const inventParametersApi = {
  async get(signal?: AbortSignal): Promise<InventParametersRecord> {
    const response = await apiClient.get<ApiResponse<InventParametersRecord>>(endpoint, { signal });
    return requireData(response.data);
  },
  async lookups(signal?: AbortSignal): Promise<InventParametersLookups> {
    const response = await apiClient.get<ApiResponse<InventParametersLookups>>(`${endpoint}/lookups`, { signal });
    return requireData(response.data);
  },
  async save(value: InventParametersRecord): Promise<InventParametersRecord> {
    const response = await apiClient.put<ApiResponse<InventParametersRecord>>(endpoint, value);
    return requireData(response.data);
  },
};
