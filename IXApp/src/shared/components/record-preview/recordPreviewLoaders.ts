import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { RecordPreviewLoader } from './types';

/**
 * Generic loader for any entity served by the shared BaseController: asks its
 * `/paged` endpoint for the single record whose key field equals the value.
 * No entity-specific backend endpoint is needed.
 */
export function pagedRecordLoader<TRecord>(
  endpoint: string,
  keyField: string
): RecordPreviewLoader<TRecord> {
  return async (key, signal) => {
    const params = new URLSearchParams({
      PageNumber: '1',
      PageSize: '1',
      'Filters[0].Field': keyField,
      'Filters[0].Operator': 'equals',
      'Filters[0].Value': key,
    });
    const { data } = await apiClient.get<ApiResponse<TRecord[]>>(`${endpoint}/paged`, {
      params,
      signal,
    });
    return Array.isArray(data?.data) && data.data.length > 0 ? data.data[0] : null;
  };
}

/**
 * Loader for entities whose full list is already fetched elsewhere (and cached):
 * finds the record by key in that list instead of issuing a new request shape.
 */
export function listRecordLoader<TRecord>(
  loadList: (signal?: AbortSignal) => Promise<TRecord[]>,
  getKey: (record: TRecord) => string
): RecordPreviewLoader<TRecord> {
  return async (key, signal) =>
    (await loadList(signal)).find((record) => getKey(record) === key) ?? null;
}
