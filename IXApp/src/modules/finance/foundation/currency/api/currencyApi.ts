import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface CurrencyDto {
  recId: number;
  currencyCode: string;
  currencyCodeIso: string;
  txt: string;
  symbol: string;
  isEuro: number;
  roundOffSales: number;
  roundOffTypeSales: number;
  roundOffPurch: number;
  roundOffTypePurch: number;
  roundOffPrice: number;
  roundOffTypePrice: number;
  roundingPrecision: number;
  ltmRoundOffLineAmount: number;
  ltmRoundOffTypeLineAmount: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface CurrencyRecord extends CurrencyDto {
  id: string;
}

const endpoint = '/v1/Currency';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The currency response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: CurrencyDto): CurrencyRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: CurrencyRecord): CurrencyDto => ({
  ...record,
  currencyCode: record.currencyCode.trim().toUpperCase(),
  currencyCodeIso: record.currencyCodeIso.trim().toUpperCase(),
  txt: record.txt.trim(),
  symbol: record.symbol.trim(),
});

export const currencyApi = {
  async list(signal?: AbortSignal): Promise<CurrencyRecord[]> {
    const response = await apiClient.get<ApiResponse<CurrencyDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(record: CurrencyRecord): Promise<CurrencyRecord> {
    const response = await apiClient.post<ApiResponse<CurrencyDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: CurrencyRecord): Promise<CurrencyRecord> {
    const response = await apiClient.put<ApiResponse<CurrencyDto>>(`${endpoint}/${record.recId}`, toDto(record));
    return toRecord(requireData(response.data));
  },
  async delete(record: CurrencyRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
