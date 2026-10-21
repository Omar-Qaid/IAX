import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InventJournalLine {
  recId: number; lineNum: number; transDate: string; itemId: string; inventDimId: string; toInventDimId: string;
  inventSiteId: string; inventLocationId: string; inventBatchId: string; inventSerialId: string;
  wmsLocationId: string; toWmsLocationId: string; toInventSiteId: string; toInventLocationId: string;
  qty: number; inventOnHand: number; counted: number; unit: string; costPrice: number; costAmount: number; ledgerDimension: number;
}
export interface InventJournal {
  recId: number; journalId: string; journalNameId: string; description: string; journalType: number;
  numOfLines: number; posted: boolean; postedDateTime?: string | null; inventSiteId: string; inventLocationId: string;
  lines: InventJournalLine[];
}
export interface InventJournalLookups {
  journalNames: Array<{ code: string; description: string }>;
  items: Array<{ code: string; description: string }>;
  sites: Array<{ code: string; description: string }>;
  warehouses: Array<{ code: string; description: string }>;
  units: Array<{ code: string; description: string }>;
}
export type InventJournalLineInput = Omit<InventJournalLine, 'recId' | 'lineNum' | 'inventDimId' | 'toInventDimId' | 'inventOnHand' | 'costAmount'>;

const endpoint = '/v1/InventJournal';
const data = <T,>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'Inventory journal request failed.', 500);
  return response.data;
};
export const inventJournalApi = {
  async list(signal?: AbortSignal): Promise<InventJournal[]> { return data((await apiClient.get<ApiResponse<InventJournal[]>>(endpoint, { signal })).data); },
  async lookups(signal?: AbortSignal): Promise<InventJournalLookups> { return data((await apiClient.get<ApiResponse<InventJournalLookups>>(`${endpoint}/lookups`, { signal })).data); },
  async ledgerDimensions(search: string, signal?: AbortSignal): Promise<Array<{ code: string; description: string }>> {
    return data((await apiClient.get<ApiResponse<Array<{ code: string; description: string }>>>(`${endpoint}/ledger-dimensions`, { params: { search }, signal })).data);
  },
  async get(id: string, signal?: AbortSignal): Promise<InventJournal> { return data((await apiClient.get<ApiResponse<InventJournal>>(`${endpoint}/${encodeURIComponent(id)}`, { signal })).data); },
  async create(input: { journalNameId: string; description: string; inventSiteId: string; inventLocationId: string }): Promise<InventJournal> { return data((await apiClient.post<ApiResponse<InventJournal>>(endpoint, input)).data); },
  async addLine(id: string, input: InventJournalLineInput): Promise<InventJournal> { return data((await apiClient.post<ApiResponse<InventJournal>>(`${endpoint}/${encodeURIComponent(id)}/lines`, input)).data); },
  async updateLine(id: string, lineId: number, input: InventJournalLineInput): Promise<InventJournal> { return data((await apiClient.put<ApiResponse<InventJournal>>(`${endpoint}/${encodeURIComponent(id)}/lines/${lineId}`, input)).data); },
  async deleteLine(id: string, lineId: number): Promise<void> { data((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(id)}/lines/${lineId}`)).data); },
  async delete(id: string): Promise<void> { data((await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${encodeURIComponent(id)}`)).data); },
  async post(id: string): Promise<{ journalId: string; posted: boolean; inventoryTransactionsCreated: number; ledgerEntriesCreated: number; vouchers: string[] }> { return data((await apiClient.post<ApiResponse<{ journalId: string; posted: boolean; inventoryTransactionsCreated: number; ledgerEntriesCreated: number; vouchers: string[] }>>(`${endpoint}/${encodeURIComponent(id)}/post`, {})).data); },
};
