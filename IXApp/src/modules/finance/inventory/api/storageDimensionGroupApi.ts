import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface StorageDimensionGroupRecord { id: string; groupId: string; name: string; useSite: boolean; useWarehouse: boolean; useLocation: boolean }
export type StorageDimensionGroupInput = Omit<StorageDimensionGroupRecord, 'id'>;
const endpoint = '/v1/EcoResStorageDimensionGroup';
const requireData = <T>(response: ApiResponse<T>): T => { if (!response.success || response.data == null) throw new ApiError(response.message || 'Storage dimension group request failed.', 400); return response.data; };
export const storageDimensionGroupApi = {
  async list(signal?: AbortSignal): Promise<StorageDimensionGroupRecord[]> { return requireData((await apiClient.get<ApiResponse<StorageDimensionGroupRecord[]>>(endpoint, { signal })).data); },
  async create(input: StorageDimensionGroupInput): Promise<StorageDimensionGroupRecord> { return requireData((await apiClient.post<ApiResponse<StorageDimensionGroupRecord>>(endpoint, input)).data); },
  async update(id: string, input: StorageDimensionGroupInput): Promise<StorageDimensionGroupRecord> { return requireData((await apiClient.put<ApiResponse<StorageDimensionGroupRecord>>(`${endpoint}/${encodeURIComponent(id)}`, input)).data); },
  async delete(id: string): Promise<void> { requireData((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(id)}`)).data); },
};
