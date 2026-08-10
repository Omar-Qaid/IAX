import type { ApiResponse } from '@core/api/apiResponse';
import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { GlobalSettings, SettingsRepository, UserSettings } from '../types/settingsTypes';

const ENDPOINTS = {
  global: '/v1/SysSettings/global',
  user: '/v1/SysSettings/user',
} as const;

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The settings response did not contain data.', 500);
  }
  return response.data;
};

export const settingsApiRepository: SettingsRepository = {
  async getGlobal(signal): Promise<GlobalSettings> {
    const response = await apiClient.get<ApiResponse<GlobalSettings>>(ENDPOINTS.global, { signal });
    return requireData(response.data);
  },
  async updateGlobal(settings): Promise<GlobalSettings> {
    const response = await apiClient.put<ApiResponse<GlobalSettings>>(ENDPOINTS.global, settings);
    return requireData(response.data);
  },
  async getUser(signal): Promise<UserSettings> {
    const response = await apiClient.get<ApiResponse<UserSettings>>(ENDPOINTS.user, { signal });
    return requireData(response.data);
  },
  async updateUser(settings): Promise<UserSettings> {
    const response = await apiClient.put<ApiResponse<UserSettings>>(ENDPOINTS.user, settings);
    return requireData(response.data);
  },
};
