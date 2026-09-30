import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventWarehouseRecord {
  id: string;
  inventLocationId: string;
  name: string;
  inventSiteId: string;
}

export interface InventSiteRecord {
  id: string;
  recId: number;
  siteId: string;
  name: string;
  defaultInventStatusId: string;
  timeZone: number;
  isReceivingWarehouseOverrideAllowed: boolean;
  defaultDimension: number;
  warehouses: InventWarehouseRecord[];
}

const endpoint = '/v1/InventSite';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The site response did not contain data.', 500);
  return response.data;
};
const payload = (site: InventSiteRecord) => ({
  siteId: site.siteId.trim().toUpperCase(),
  name: site.name.trim(),
  defaultInventStatusId: site.defaultInventStatusId.trim(),
  timeZone: site.timeZone,
  isReceivingWarehouseOverrideAllowed: site.isReceivingWarehouseOverrideAllowed,
  defaultDimension: site.defaultDimension,
});

export const inventSiteApi = {
  async list(signal?: AbortSignal): Promise<InventSiteRecord[]> {
    const response = await apiClient.get<ApiResponse<InventSiteRecord[]>>(endpoint, { signal });
    return requireData(response.data);
  },
  async create(site: InventSiteRecord): Promise<InventSiteRecord> {
    const response = await apiClient.post<ApiResponse<InventSiteRecord>>(endpoint, payload(site));
    return requireData(response.data);
  },
  async update(site: InventSiteRecord): Promise<InventSiteRecord> {
    const response = await apiClient.put<ApiResponse<InventSiteRecord>>(
      `${endpoint}/${site.recId}`,
      payload(site)
    );
    return requireData(response.data);
  },
  async delete(site: InventSiteRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${site.recId}`);
    requireData(response.data);
  },
};
