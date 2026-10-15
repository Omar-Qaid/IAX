import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TrackingDimensionGroupRecord {
  id: string;
  groupId: string;
  name: string;
  useBatchNumber: boolean;
  useSerialNumber: boolean;
}

export type TrackingDimensionGroupInput = Omit<TrackingDimensionGroupRecord, 'id'>;

const endpoint = '/v1/EcoResTrackingDimensionGroup';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'Tracking dimension group request failed.', 400);
  }
  return response.data;
};

export const trackingDimensionGroupApi = {
  async list(signal?: AbortSignal): Promise<TrackingDimensionGroupRecord[]> {
    return requireData(
      (await apiClient.get<ApiResponse<TrackingDimensionGroupRecord[]>>(endpoint, { signal })).data
    );
  },
  async create(input: TrackingDimensionGroupInput): Promise<TrackingDimensionGroupRecord> {
    return requireData(
      (await apiClient.post<ApiResponse<TrackingDimensionGroupRecord>>(endpoint, input)).data
    );
  },
  async update(
    id: string,
    input: TrackingDimensionGroupInput
  ): Promise<TrackingDimensionGroupRecord> {
    return requireData(
      (
        await apiClient.put<ApiResponse<TrackingDimensionGroupRecord>>(
          `${endpoint}/${encodeURIComponent(id)}`,
          input
        )
      ).data
    );
  },
  async delete(id: string): Promise<void> {
    requireData(
      (await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(id)}`)).data
    );
  },
};
