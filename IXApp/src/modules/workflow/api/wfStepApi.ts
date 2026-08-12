import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfStepDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  processId: number;
  sortOrder: number;
  score: number;
  autoPassingHrs: number;
  allMandatory: boolean;
  sysField: boolean;
}

export interface WfStepRecord extends WfStepDto {
  id: string;
}

const endpoint = '/v1/WfStep';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The workflow-step response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: WfStepDto): WfStepRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfStepRecord): WfStepDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAR: record.nameAR?.trim() || null,
  description: record.description?.trim() || null,
  descriptionAR: record.descriptionAR?.trim() || null,
});

export const wfStepApi = {
  async list(signal?: AbortSignal): Promise<WfStepRecord[]> {
    const response = await apiClient.get<ApiResponse<WfStepDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfStepRecord): Promise<WfStepRecord> {
    const response = await apiClient.post<ApiResponse<WfStepDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfStepRecord): Promise<WfStepRecord> {
    const response = await apiClient.put<ApiResponse<WfStepDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfStepRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
