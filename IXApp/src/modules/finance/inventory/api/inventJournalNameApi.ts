import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventJournalNameRecord {
  id: string;
  recId: number;
  journalNameId: string;
  description: string;
  journalType: number;
  voucherNumberSequenceTable: number;
  voucherNumberSequenceCode: string;
  detailSummary: number;
  deletePostedLines: boolean;
  reservation: number;
}

export interface InventJournalNameLookups {
  numberSequences: Array<{ recId: number; numberSequence: string; description: string }>;
}

const endpoint = '/v1/InventJournalName';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The journal name response did not contain data.', 500);
  return response.data;
};
const payload = (x: InventJournalNameRecord) => ({
  journalNameId: x.journalNameId.trim().toUpperCase(),
  description: x.description.trim(),
  journalType: x.journalType,
  voucherNumberSequenceTable: x.voucherNumberSequenceTable,
  detailSummary: x.detailSummary,
  deletePostedLines: x.deletePostedLines,
  reservation: x.reservation,
});

export const inventJournalNameApi = {
  async list(signal?: AbortSignal): Promise<InventJournalNameRecord[]> {
    return requireData((await apiClient.get<ApiResponse<InventJournalNameRecord[]>>(endpoint, { signal })).data);
  },
  async lookups(signal?: AbortSignal): Promise<InventJournalNameLookups> {
    return requireData((await apiClient.get<ApiResponse<InventJournalNameLookups>>(`${endpoint}/lookups`, { signal })).data);
  },
  async create(x: InventJournalNameRecord): Promise<InventJournalNameRecord> {
    return requireData((await apiClient.post<ApiResponse<InventJournalNameRecord>>(endpoint, payload(x))).data);
  },
  async update(x: InventJournalNameRecord): Promise<InventJournalNameRecord> {
    return requireData((await apiClient.put<ApiResponse<InventJournalNameRecord>>(`${endpoint}/${x.recId}`, payload(x))).data);
  },
  async delete(x: InventJournalNameRecord): Promise<void> {
    if (requireData((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${x.recId}`)).data) !== true)
      throw new ApiError('The journal name could not be deleted.', 400);
  },
};
