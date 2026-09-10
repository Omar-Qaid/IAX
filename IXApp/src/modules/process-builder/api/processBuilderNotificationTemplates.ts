import { apiClient } from '@core/api/apiClient';
import { ApiError } from '@core/api/apiError';
import type { ApiResponse } from '@core/api/apiResponse';

export interface BuilderNotificationTemplate {
  recId: number;
  code: string;
  name: string;
  isActive: boolean;
}

export async function loadBuilderNotificationTemplates(signal?: AbortSignal): Promise<BuilderNotificationTemplate[]> {
  const response = await apiClient.get<ApiResponse<BuilderNotificationTemplate[]>>('/v1/SysNotificationTemplate', { signal });
  if (!response.data.success || !Array.isArray(response.data.data))
    throw new ApiError(response.data.message || 'Unable to load notification templates.', 500);
  return response.data.data;
}
