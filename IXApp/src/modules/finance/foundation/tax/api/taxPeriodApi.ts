import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxPeriodIntervalDto {
  recId: number;
  taxPeriod: string;
  fromDate: string;
  toDate: string;
  closed: number;
}

export interface TaxPeriodInterval extends TaxPeriodIntervalDto { id: string }

export interface TaxPeriodDto {
  recId: number;
  taxPeriod: string;
  name: string;
  taxAuthority: string;
  paymentCode: string;
  qtyUnit: number;
  periodUnit: number;
  notGenerateOffsetTaxTrans: number;
  reportAdjustment: number;
  useBatch: number;
  activePeriodForBatchJobs: string;
  intervals: TaxPeriodIntervalDto[];
}

export interface TaxPeriodRecord extends Omit<TaxPeriodDto, 'intervals'> {
  id: string;
  intervals: TaxPeriodInterval[];
}

const endpoint = '/v1/TaxPeriodHead';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The sales tax settlement period response did not contain data.', 500);
  }
  return response.data;
};
const toInterval = (row: TaxPeriodIntervalDto): TaxPeriodInterval => ({ ...row, id: row.recId ? String(row.recId) : `new-${crypto.randomUUID()}` });
const toRecord = (dto: TaxPeriodDto): TaxPeriodRecord => ({ ...dto, id: String(dto.recId), intervals: (dto.intervals ?? []).map(toInterval) });
const toDto = ({ id: _id, intervals, ...record }: TaxPeriodRecord): TaxPeriodDto => ({
  ...record,
  taxPeriod: record.taxPeriod.trim(), name: record.name.trim(), taxAuthority: record.taxAuthority.trim().toUpperCase(), paymentCode: record.paymentCode.trim(),
  intervals: intervals.map(({ id: _intervalId, ...interval }) => ({ ...interval, taxPeriod: record.taxPeriod.trim() })),
});

export const taxPeriodApi = {
  async list(signal?: AbortSignal): Promise<TaxPeriodRecord[]> {
    const response = await apiClient.get<ApiResponse<TaxPeriodDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: TaxPeriodRecord): Promise<TaxPeriodRecord> {
    const response = await apiClient.post<ApiResponse<TaxPeriodDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: TaxPeriodRecord): Promise<TaxPeriodRecord> {
    const response = await apiClient.put<ApiResponse<TaxPeriodDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: TaxPeriodRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
