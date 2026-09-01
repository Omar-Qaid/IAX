import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { WorkflowMasterDto } from './workflowMasterApi';
import type { WfProcessDto } from './wfProcessApi';

export interface WfVariableDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  dataTypeId: number;
  processId: number;
  sortOrder: number;
  dataType?: WorkflowMasterDto | null;
  process?: WfProcessDto | null;
}

export interface WfVariableRecord extends WfVariableDto {
  id: string;
}

const endpoint = '/v1/WfVariable';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || 'The workflow-variable response did not contain data.',
      500
    );
  }
  return response.data;
};

const toRecord = (dto: WfVariableDto): WfVariableRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({
  id: _id,
  dataType: _dataType,
  process: _process,
  ...record
}: WfVariableRecord): WfVariableDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
  dataType: null,
  process: null,
});

export const wfVariableApi = {
  async list(signal?: AbortSignal): Promise<WfVariableRecord[]> {
    const response = await apiClient.get<ApiResponse<WfVariableDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfVariableRecord): Promise<WfVariableRecord> {
    const response = await apiClient.post<ApiResponse<WfVariableDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfVariableRecord): Promise<WfVariableRecord> {
    const response = await apiClient.put<ApiResponse<WfVariableDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfVariableRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
