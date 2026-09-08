import { apiClient } from './apiClient';
import { ApiError } from './apiError';
import type { ApiResponse } from './apiResponse';
import { requireApiData } from './apiResponseData';

export interface EntityQueryFilter {
  field: string;
  operator: string;
  value: string | number | boolean;
}

export interface PagedEntityQuery {
  filters?: readonly EntityQueryFilter[];
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
  signal?: AbortSignal;
}

/** Uses the shared /paged contract and the server's maximum page size. */
export async function fetchAllPages<T>(
  endpoint: string,
  { filters = [], sortField = 'RecId', sortOrder = 'asc', signal }: PagedEntityQuery = {}
): Promise<T[]> {
  const rows: T[] = [];
  for (let page = 1; ; page += 1) {
    signal?.throwIfAborted();
    const params = new URLSearchParams({
      PageNumber: String(page),
      PageSize: '100',
      SortField: sortField,
      SortOrder: sortOrder,
    });
    filters.forEach((filter, index) => {
      params.set(`Filters[${index}].Field`, filter.field);
      params.set(`Filters[${index}].Operator`, filter.operator);
      params.set(`Filters[${index}].Value`, String(filter.value));
    });
    const { data } = await apiClient.get<ApiResponse<T[]>>(`${endpoint}/paged`, { params, signal });
    const pageRows = requireApiData(data, 'paged records');
    if (!Array.isArray(pageRows)) throw new ApiError('Expected a list of records.', 500);
    const pagination = data.pagination;
    if (
      pagination &&
      (!Number.isSafeInteger(pagination.totalPages) ||
        pagination.totalPages < 0 ||
        (pagination.pageNumber !== undefined && pagination.pageNumber !== page))
    ) {
      throw new ApiError('Invalid pagination metadata.', 500);
    }
    if (pagination && pageRows.length === 0 && page < pagination.totalPages) {
      throw new ApiError('The paged response ended before all records were returned.', 500);
    }
    rows.push(...pageRows);
    if (
      pageRows.length === 0 ||
      (data.pagination ? page >= data.pagination.totalPages : pageRows.length < 100)
    )
      return rows;
  }
}
