import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfActivityDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  activityTypeId: number;
  stepId: number;
  performerId: number;
  score: number;
  sysNotificationTemplateId: number | null;
  alertingBySystem: boolean;
  alertingByEmail: boolean;
  alertingBySms: boolean;
  alertingByWhatsApp: boolean;
  showPreviousSteps: boolean;
  showPreviousDocs: boolean;
  mandatoryDocs: boolean;
  autoPassEnabled: boolean;
  autoPassingHrs: number;
  extendedProperties: string | null;
}

export interface WfActivityRecord extends WfActivityDto {
  id: string;
}

const endpoint = '/v1/WfActivity';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The workflow-activity response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: WfActivityDto): WfActivityRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityRecord): WfActivityDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  description: record.description?.trim() || null,
  extendedProperties: record.extendedProperties?.trim() || null,
});

export const wfActivityApi = {
  async list(signal?: AbortSignal): Promise<WfActivityRecord[]> {
    const response = await apiClient.get<ApiResponse<WfActivityDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfActivityRecord): Promise<WfActivityRecord> {
    const response = await apiClient.post<ApiResponse<WfActivityDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfActivityRecord): Promise<WfActivityRecord> {
    const response = await apiClient.put<ApiResponse<WfActivityDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfActivityRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
