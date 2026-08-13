import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfProcessAssignmentDto {
  id: number;
  processId: number;
  departmentId: number | null;
  occupationId: number | null;
  employeeId: number | null;
}

export interface WfProcessDto {
  recId: number;
  code: string | null;
  name: string | null;
  categoryId: number;
  score: number;
  canRepeat: boolean;
  mandatoryDocs: boolean;
  priorityId: number;
  processTypeId: number;
  sysField: boolean;
  sortOrder: number;
  usersProcesses: WfProcessAssignmentDto[];
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface WfProcessRecord extends WfProcessDto {
  id: string;
}

const endpoint = '/v1/WfProcess';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || 'The workflow-process response did not contain data.',
      500
    );
  }
  return response.data;
};

const toRecord = (dto: WfProcessDto): WfProcessRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfProcessRecord): WfProcessDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
});

export const wfProcessApi = {
  async list(signal?: AbortSignal): Promise<WfProcessRecord[]> {
    const response = await apiClient.get<ApiResponse<WfProcessDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async getById(recId: number, signal?: AbortSignal): Promise<WfProcessRecord> {
    const response = await apiClient.get<ApiResponse<WfProcessDto>>(`${endpoint}/${recId}`, {
      signal,
    });
    return toRecord(requireData(response.data));
  },
  async create(record: WfProcessRecord): Promise<WfProcessRecord> {
    const response = await apiClient.post<ApiResponse<WfProcessDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfProcessRecord): Promise<WfProcessRecord> {
    const response = await apiClient.put<ApiResponse<WfProcessDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfProcessRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
