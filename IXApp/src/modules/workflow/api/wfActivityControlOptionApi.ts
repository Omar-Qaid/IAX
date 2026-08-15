import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfActivityControlOptionRecord {
  id: string;
  recId: number;
  activityControlId: number;
  value: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfActivityControlOptionRecord, 'id'>;
const endpoint = '/v1/WfActivityControlsOption';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The activity-control option response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: Dto): WfActivityControlOptionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityControlOptionRecord): Dto => record;

export const wfActivityControlOptionApi = {
  async list(signal?: AbortSignal): Promise<WfActivityControlOptionRecord[]> {
    const response = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfActivityControlOptionRecord): Promise<WfActivityControlOptionRecord> {
    const response = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfActivityControlOptionRecord): Promise<WfActivityControlOptionRecord> {
    const response = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfActivityControlOptionRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
