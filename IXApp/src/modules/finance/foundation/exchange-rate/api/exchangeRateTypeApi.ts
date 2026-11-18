import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface ExchangeRateTypeDto {
  recId: number;
  name: string;
  description: string;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface ExchangeRateTypeRecord {
  id: string;
  recId: number;
  type: string;
  name: string;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

const endpoint = '/v1/ExchangeRateType';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The exchange-rate-type response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: ExchangeRateTypeDto): ExchangeRateTypeRecord => ({
  id: String(dto.recId), recId: dto.recId, type: dto.name, name: dto.description,
  isActive: dto.isActive, rowVersion: dto.rowVersion, recVersion: dto.recVersion, dataAreaId: dto.dataAreaId,
});

const toDto = (record: ExchangeRateTypeRecord): ExchangeRateTypeDto => ({
  recId: record.recId, name: record.type.trim(), description: record.name.trim(),
  isActive: record.isActive, rowVersion: record.rowVersion, recVersion: record.recVersion, dataAreaId: record.dataAreaId,
});

export const exchangeRateTypeApi = {
  async list(signal?: AbortSignal): Promise<ExchangeRateTypeRecord[]> {
    const response = await apiClient.get<ApiResponse<ExchangeRateTypeDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: ExchangeRateTypeRecord): Promise<ExchangeRateTypeRecord> {
    const response = await apiClient.post<ApiResponse<ExchangeRateTypeDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: ExchangeRateTypeRecord): Promise<ExchangeRateTypeRecord> {
    const response = await apiClient.put<ApiResponse<ExchangeRateTypeDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: ExchangeRateTypeRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
