import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfActivityControlValidationRecord {
  id: string;
  recId: number;
  activityControlId: number;
  validationType: string;
  validationExpression: string | null;
  operator: string | null;
  value: string | null;
  maskInput: string | null;
  errorMessage: string;
  severity: string;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfActivityControlValidationRecord, 'id'>;
const endpoint = '/v1/WfActivityControlsValidation';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The activity-control validation response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: Dto): WfActivityControlValidationRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityControlValidationRecord): Dto => record;

export const wfActivityControlValidationApi = {
  async list(signal?: AbortSignal): Promise<WfActivityControlValidationRecord[]> {
    const response = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfActivityControlValidationRecord): Promise<WfActivityControlValidationRecord> {
    const response = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfActivityControlValidationRecord): Promise<WfActivityControlValidationRecord> {
    const response = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfActivityControlValidationRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
