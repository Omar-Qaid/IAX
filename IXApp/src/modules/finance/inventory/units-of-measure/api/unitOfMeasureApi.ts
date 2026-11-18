import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface UnitConversionRecord { id: string; recId: number; fromUnitOfMeasure: number; fromSymbol: string; toUnitOfMeasure: number; toSymbol: string; product: number; productNumber: string; factor: number; numerator: number; denominator: number; innerOffset: number; outerOffset: number; rounding: number; }
export interface UnitOfMeasureRecord { id: string; recId: number; symbol: string; unitOfMeasureClass: number; systemOfUnits: number; decimalPrecision: number; conversions: UnitConversionRecord[]; }
export interface UnitConversionInput { fromUnitOfMeasure: number; toUnitOfMeasure: number; product: number; factor: number; numerator: number; denominator: number; innerOffset: number; outerOffset: number; rounding: number; }
const endpoint = '/v1/UnitOfMeasure';
const requireData = <T>(response: ApiResponse<T>): T => { if (!response.success || response.data == null) throw new ApiError(response.message || 'The unit response did not contain data.', 500); return response.data; };
const payload = (x: UnitOfMeasureRecord) => ({ symbol: x.symbol.trim(), unitOfMeasureClass: x.unitOfMeasureClass, systemOfUnits: x.systemOfUnits, decimalPrecision: x.decimalPrecision });
export const unitOfMeasureApi = {
  async list(signal?: AbortSignal): Promise<UnitOfMeasureRecord[]> { const response = await apiClient.get<ApiResponse<UnitOfMeasureRecord[]>>(endpoint, { signal }); return requireData(response.data); },
  async create(x: UnitOfMeasureRecord): Promise<UnitOfMeasureRecord> { const response = await apiClient.post<ApiResponse<UnitOfMeasureRecord>>(endpoint, payload(x)); return requireData(response.data); },
  async update(x: UnitOfMeasureRecord): Promise<UnitOfMeasureRecord> { const response = await apiClient.put<ApiResponse<UnitOfMeasureRecord>>(`${endpoint}/${x.recId}`, payload(x)); return requireData(response.data); },
  async delete(x: UnitOfMeasureRecord): Promise<void> { const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${x.recId}`); if (requireData(response.data) !== true) throw new ApiError('The unit could not be deleted.', 400); },
  async createConversion(x: UnitConversionInput): Promise<UnitConversionRecord> { const response = await apiClient.post<ApiResponse<UnitConversionRecord>>(`${endpoint}/conversions`, x); return requireData(response.data); },
  async updateConversion(recId: number, x: UnitConversionInput): Promise<UnitConversionRecord> { const response = await apiClient.put<ApiResponse<UnitConversionRecord>>(`${endpoint}/conversions/${recId}`, x); return requireData(response.data); },
  async deleteConversion(recId: number): Promise<void> { const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/conversions/${recId}`); if (requireData(response.data) !== true) throw new ApiError('The unit conversion could not be deleted.', 400); },
};
