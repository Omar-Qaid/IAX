import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type {
  DocumentCopyLine,
  DocumentCopyOptions,
  DocumentCopySource,
} from '@patterns/document/DocumentCopyDrawer';

export type SalesOrderCopyMode = 'fromAll' | 'fromJournal';

const segment = (mode: SalesOrderCopyMode) => (mode === 'fromAll' ? 'from-all' : 'from-journal');

function data<T>(response: { data: ApiResponse<T> }, message: string): T {
  if (!response.data.success || response.data.data == null)
    throw new ApiError(response.data.message || message, 500);
  return response.data.data;
}

export const salesOrderCopyApi = {
  async documents(orderId: string, mode: SalesOrderCopyMode, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<DocumentCopySource[]>>(
      `/v1/SalesTable/${orderId}/copy/${segment(mode)}/documents`,
      { signal }
    );
    return data(response, 'Source documents could not be loaded.');
  },
  async lines(orderId: string, mode: SalesOrderCopyMode, sourceId: number, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<DocumentCopyLine[]>>(
      `/v1/SalesTable/${orderId}/copy/${segment(mode)}/documents/${sourceId}/lines`,
      { signal }
    );
    return data(response, 'Source lines could not be loaded.');
  },
  async copy(orderId: string, mode: SalesOrderCopyMode, lineIds: number[], options: DocumentCopyOptions) {
    const response = await apiClient.post<ApiResponse<{ copiedLineCount: number }>>(
      `/v1/SalesTable/${orderId}/copy`,
      { mode, lineIds, ...options }
    );
    return data(response, 'The selected lines could not be copied.');
  },
};
