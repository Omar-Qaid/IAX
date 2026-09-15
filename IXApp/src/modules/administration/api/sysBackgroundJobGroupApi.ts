import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface SysBackgroundJobGroupRecord {
  id: string;
  recId: number;
  groupCode: string;
  description: string | null;
  schedulingPriority: number;
  maxConcurrency: number;
  isActive: boolean;
  rowVersion: string | null;
}

type GroupDto = Omit<SysBackgroundJobGroupRecord, 'id'>;
const endpoint = '/v1/SysBackgroundJobGroup';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The batch-group response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: GroupDto): SysBackgroundJobGroupRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: SysBackgroundJobGroupRecord): GroupDto => ({
  ...record,
  groupCode: record.groupCode.trim(),
  description: record.description?.trim() || null,
});

export const sysBackgroundJobGroupApi = {
  async list(signal?: AbortSignal): Promise<SysBackgroundJobGroupRecord[]> {
    const response = await apiClient.get<ApiResponse<GroupDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: SysBackgroundJobGroupRecord): Promise<SysBackgroundJobGroupRecord> {
    const response = await apiClient.post<ApiResponse<GroupDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: SysBackgroundJobGroupRecord): Promise<SysBackgroundJobGroupRecord> {
    const response = await apiClient.put<ApiResponse<GroupDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async remove(record: SysBackgroundJobGroupRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
