import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfRequestControlValidationRecord {
  id: string;
  recId: number;
  requestControlId: number;
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
type Dto = Omit<WfRequestControlValidationRecord, 'id'>;
const endpoint = '/v1/WfRequestControlsValidation';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The request-control validation response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: Dto): WfRequestControlValidationRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfRequestControlValidationRecord): Dto => record;

export const wfRequestControlValidationApi = {
  async list(signal?: AbortSignal): Promise<WfRequestControlValidationRecord[]> {
    const response = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: WfRequestControlValidationRecord): Promise<WfRequestControlValidationRecord> {
    const response = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfRequestControlValidationRecord): Promise<WfRequestControlValidationRecord> {
    const response = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: WfRequestControlValidationRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
