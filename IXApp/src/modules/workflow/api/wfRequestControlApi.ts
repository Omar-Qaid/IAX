import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfRequestControlDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  processId: number;
  controlId: number;
  mandatory: boolean;
  uniqueKey: boolean;
  score: number;
  usedAsCriteria: boolean;
  sortOrder: number;
  validationRules: string | null;
  extendedProperties: string | null;
}

export interface WfRequestControlRecord extends WfRequestControlDto { id: string }
const endpoint = '/v1/WfRequestControl';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The request-control response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: WfRequestControlDto): WfRequestControlRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfRequestControlRecord): WfRequestControlDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
});

export const wfRequestControlApi = {
  async list(signal?: AbortSignal): Promise<WfRequestControlRecord[]> {
    const response = await apiClient.get<ApiResponse<WfRequestControlDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfRequestControlRecord): Promise<WfRequestControlRecord> {
    const response = await apiClient.post<ApiResponse<WfRequestControlDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfRequestControlRecord): Promise<WfRequestControlRecord> {
    const response = await apiClient.put<ApiResponse<WfRequestControlDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfRequestControlRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
