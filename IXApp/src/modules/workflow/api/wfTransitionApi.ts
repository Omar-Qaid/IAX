import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfTransitionRecord {
  id: string; recId: number; processId: number; activityId: number | null;
  requestControlId: number | null; variableId: number; operatorId: number;
  value: string; stepId: number; sortOrder: number; isActive: boolean;
  rowVersion: string | null; recVersion: number; dataAreaId: string;
}
type Dto = Omit<WfTransitionRecord, 'id'>;
const endpoint = '/v1/WfTransition';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The workflow-transition response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: Dto): WfTransitionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfTransitionRecord): Dto => record;
export const wfTransitionApi = {
  async list(signal?: AbortSignal): Promise<WfTransitionRecord[]> {
    const response = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfTransitionRecord): Promise<WfTransitionRecord> {
    const response = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfTransitionRecord): Promise<WfTransitionRecord> {
    const response = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfTransitionRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
