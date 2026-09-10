import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface InboxNotification {
  recId: number;
  title: string;
  message: string;
  createdAt: string | null;
  isRead: boolean;
  isArchived: boolean;
  priority: 'Critical' | 'High' | 'Medium' | 'Low';
  category?: string;
}
const data = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'Notification request failed.', 500);
  return response.data;
};
const path = '/v1/SysNotification';
export const notificationInboxApi = {
  async list(page: number, tab: number, signal?: AbortSignal) {
    const response = await apiClient.get<ApiResponse<InboxNotification[]>>(path, {
      params: { pageNumber: page, pageSize: 20, isArchived: tab === 2, ...(tab === 1 ? { isRead: false } : {}) }, signal,
    });
    return { items: data(response.data), totalPages: response.data.pagination?.totalPages ?? 1 };
  },
  async unread(signal?: AbortSignal) {
    return data((await apiClient.get<ApiResponse<number>>(`${path}/unread-count`, { signal })).data);
  },
  async update(action: 'read' | 'archive' | 'delete' | 'read-all', id?: string) {
    if (action === 'delete') return data((await apiClient.delete<ApiResponse<boolean>>(`${path}/${id}`)).data);
    return data((await apiClient.put<ApiResponse<boolean>>(action === 'read-all' ? `${path}/read-all` : `${path}/${id}/${action}`, {})).data);
  },
};
