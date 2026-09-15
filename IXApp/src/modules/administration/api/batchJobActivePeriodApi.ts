import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
export interface BatchJobActivePeriodRecord { id: string; recId: number; periodId: string; name: string | null; fromTimeUtc: number; toTimeUtc: number; fromTimeLocal: number; toTimeLocal: number; timeZoneFollowed: number; isActive: boolean; }
type Dto = Omit<BatchJobActivePeriodRecord, 'id' | 'periodId'> & { id: string };
const endpoint = '/v1/BatchJobActivePeriod';
const data = <T>(r: ApiResponse<T>) => { if (!r.success || r.data == null) throw new ApiError(r.message || 'Active-period response has no data.', 500); return r.data; };
const fromDto = (x: Dto): BatchJobActivePeriodRecord => ({ ...x, id: String(x.recId), periodId: x.id });
const toDto = (x: BatchJobActivePeriodRecord): Dto => ({ recId: x.recId, id: x.periodId.trim(), name: x.name?.trim() || null, fromTimeUtc: x.fromTimeUtc, toTimeUtc: x.toTimeUtc, fromTimeLocal: x.fromTimeLocal, toTimeLocal: x.toTimeLocal, timeZoneFollowed: x.timeZoneFollowed, isActive: x.isActive });
export const batchJobActivePeriodApi = {
  async list(signal?: AbortSignal) { const r = await apiClient.get<ApiResponse<Dto[]>>(endpoint, { signal }); return data(r.data).map(fromDto); },
  async create(x: BatchJobActivePeriodRecord) { const r = await apiClient.post<ApiResponse<Dto>>(endpoint, toDto(x)); return fromDto(data(r.data)); },
  async update(x: BatchJobActivePeriodRecord) { const r = await apiClient.put<ApiResponse<Dto>>(`${endpoint}/${x.recId}`, toDto(x)); return fromDto(data(r.data)); },
  async remove(x: BatchJobActivePeriodRecord) { const r = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${x.recId}`); data(r.data); },
};
