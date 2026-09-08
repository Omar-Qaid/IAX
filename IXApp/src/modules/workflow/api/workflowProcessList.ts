import { ApiError } from '@core/api/apiError';
import { fetchAllPages } from '@core/api/fetchAllPages';

/** Workflow owns the relationship path and process-id validation. */
export function listWorkflowProcessRows<T>(
  endpoint: string,
  processField: string,
  processId: number,
  signal?: AbortSignal
): Promise<T[]> {
  if (!Number.isSafeInteger(processId) || processId <= 0) {
    return Promise.reject(new ApiError('A valid workflow process is required.', 400));
  }
  return fetchAllPages<T>(endpoint, {
    filters: [{ field: processField, operator: 'equals', value: processId }],
    signal,
  });
}
