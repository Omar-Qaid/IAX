import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfActivityControlDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  activityId: number;
  processId: number;
  controlId: number;
  mandatory: boolean;
  uniqueKey: boolean;
  score: number;
  usedAsCriteria: boolean;
  usedInSearch: boolean;
  sortOrder: number;
  validationRules: string | null;
  extendedProperties: string | null;
}

export interface WfActivityControlRecord extends WfActivityControlDto { id: string }
const endpoint = '/v1/WfActivityControl';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The activity-control response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: WfActivityControlDto): WfActivityControlRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityControlRecord): WfActivityControlDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  description: record.description?.trim() || null,
});

export const wfActivityControlApi = {
  async list(signal?: AbortSignal): Promise<WfActivityControlRecord[]> {
    const response = await apiClient.get<ApiResponse<WfActivityControlDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfActivityControlRecord): Promise<WfActivityControlRecord> {
    const response = await apiClient.post<ApiResponse<WfActivityControlDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfActivityControlRecord): Promise<WfActivityControlRecord> {
    const response = await apiClient.put<ApiResponse<WfActivityControlDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfActivityControlRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
