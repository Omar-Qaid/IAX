import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface SysNumberSequenceDto {
  recId: number;
  numberSequence: string;
  txt: string;
  latestCleanDateTime: string | null;
  latestCleanDateTimeTzId: number | null;
  lowest: number | null;
  highest: number | null;
  nextRec: number | null;
  blocked: number | null;
  format: string;
  continuous: number | null;
  cyclic: number | null;
  annotatedFormat: string;
  cleanAtAccess: number | null;
  inUse: number | null;
  noIncrement: number | null;
  numberSequenceScope: number | null;
  cleanInterval: number | null;
  allowChangeUp: number | null;
  allowChangeDown: number | null;
  manual: number | null;
  fetchAheadQty: number | null;
  fetchAhead: number | null;
  modifiedTransactionId: number | null;
  partition: number | null;
  isActive: boolean;
  rowVersion: string | null;
  isDeleted: boolean;
  recVersion: number;
  dataAreaId: string;
}

export interface SysNumberSequenceRecord extends SysNumberSequenceDto {
  id: string;
}

const endpoint = '/v1/SysNumberSequence';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || 'The number-sequence response did not contain data.',
      500
    );
  }
  return response.data;
};

const toRecord = (dto: SysNumberSequenceDto): SysNumberSequenceRecord => ({
  ...dto,
  id: String(dto.recId),
});

const toDto = ({ id: _id, ...record }: SysNumberSequenceRecord): SysNumberSequenceDto => ({
  ...record,
  numberSequence: record.numberSequence.trim(),
  txt: record.txt.trim(),
  format: record.format.trim(),
  annotatedFormat: record.annotatedFormat.trim(),
});

export const sysNumberSequenceApi = {
  async list(signal?: AbortSignal): Promise<SysNumberSequenceRecord[]> {
    const response = await apiClient.get<ApiResponse<SysNumberSequenceDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: SysNumberSequenceRecord): Promise<SysNumberSequenceRecord> {
    const response = await apiClient.post<ApiResponse<SysNumberSequenceDto>>(
      endpoint,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async update(record: SysNumberSequenceRecord): Promise<SysNumberSequenceRecord> {
    const response = await apiClient.put<ApiResponse<SysNumberSequenceDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: SysNumberSequenceRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
