import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface HcmShowroomDto {
  recId: number;
  personnelNumber: string;
  party: number;
  name?: string | null;
  nameAlias?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface HcmShowroomRecord extends Omit<HcmShowroomDto, 'recId'> {
  id: string;
  recordId: number;
}

export interface HcmShowroomWorkerAssignment {
  recId: number;
  hcmWorkerId: number;
  personnelNumber: string;
  workerName?: string | null;
  workerNameAlias?: string | null;
  validFrom: string;
  validTo?: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

export interface SaveHcmShowroomWorkerAssignment {
  hcmWorkerId: number;
  validFrom: string;
  validTo: string | null;
  isActive: boolean;
}

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The showroom response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = ({ recId, ...dto }: HcmShowroomDto): HcmShowroomRecord => ({
  ...dto,
  id: String(recId),
  recordId: recId,
});

const toDto = ({ id: _id, recordId, ...record }: HcmShowroomRecord): HcmShowroomDto => ({
  ...record,
  recId: recordId,
});

export const hcmShowroomApi = {
  async list(signal?: AbortSignal): Promise<HcmShowroomRecord[]> {
    const response = await apiClient.get<ApiResponse<HcmShowroomDto[]>>('/v1/HcmShowroom', {
      signal,
    });
    return requireData(response.data).map(toRecord);
  },
  async create(record: HcmShowroomRecord): Promise<HcmShowroomRecord> {
    const response = await apiClient.post<ApiResponse<HcmShowroomDto>>(
      '/v1/HcmShowroom',
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async update(record: HcmShowroomRecord): Promise<HcmShowroomRecord> {
    const response = await apiClient.put<ApiResponse<HcmShowroomDto>>(
      `/v1/HcmShowroom/${record.recordId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: HcmShowroomRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(
      `/v1/HcmShowroom/${record.recordId}`
    );
    requireData(response.data);
  },
  async workerAssignments(
    showroomId: number,
    signal?: AbortSignal
  ): Promise<HcmShowroomWorkerAssignment[]> {
    const response = await apiClient.get<ApiResponse<HcmShowroomWorkerAssignment[]>>(
      `/v1/HcmShowroom/${showroomId}/worker-assignments`,
      { signal }
    );
    return requireData(response.data);
  },
  async saveWorkerAssignment(
    showroomId: number,
    assignmentId: number | null,
    request: SaveHcmShowroomWorkerAssignment
  ): Promise<void> {
    const path = `/v1/HcmShowroom/${showroomId}/worker-assignments`;
    const response = assignmentId
      ? await apiClient.put<ApiResponse<boolean>>(`${path}/${assignmentId}`, request)
      : await apiClient.post<ApiResponse<boolean>>(path, request);
    requireData(response.data);
  },
};
