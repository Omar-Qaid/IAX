import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface HcmWorkerDto {
  id: number;
  personnelNumber: string;
  person: number;
  name?: string | null;
  departmentId: number;
  departmentName?: string | null;
  occupationId: number;
  occupationName?: string | null;
  genderId: number;
  nationalityId: number;
  hireDate: string | null;
  birthDate: string | null;
  showroomId: number | null;
  showroomName?: string | null;
  userId?: string | null;
  isActive: boolean;
}

export interface HcmWorkerRecord extends Omit<HcmWorkerDto, 'id'> {
  id: string;
  recordId: number;
}

export interface HcmLookupOption {
  id: number;
  code?: string | null;
  name?: string | null;
}

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The worker response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: HcmWorkerDto): HcmWorkerRecord => ({
  ...dto,
  id: String(dto.id),
  recordId: dto.id,
});
const toDto = ({ id: _id, recordId, ...record }: HcmWorkerRecord): HcmWorkerDto => ({
  ...record,
  id: recordId,
});

export const hcmWorkerApi = {
  async list(signal?: AbortSignal): Promise<HcmWorkerRecord[]> {
    const response = await apiClient.get<ApiResponse<HcmWorkerDto[]>>('/v1/HcmWorker', { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: HcmWorkerRecord): Promise<HcmWorkerRecord> {
    const response = await apiClient.post<ApiResponse<HcmWorkerDto>>(
      '/v1/HcmWorker',
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async update(record: HcmWorkerRecord): Promise<HcmWorkerRecord> {
    const response = await apiClient.put<ApiResponse<HcmWorkerDto>>(
      `/v1/HcmWorker/${record.recordId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: HcmWorkerRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(
      `/v1/HcmWorker/${record.recordId}`
    );
    requireData(response.data);
  },
  async lookup(endpoint: string, signal?: AbortSignal): Promise<HcmLookupOption[]> {
    const response = await apiClient.get<ApiResponse<HcmLookupOption[]>>(`/v1/${endpoint}`, {
      signal,
    });
    return requireData(response.data);
  },
};
