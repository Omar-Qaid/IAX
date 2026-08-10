import type { AxiosInstance, InternalAxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import { ApiError, type ApiValidationProblem } from './apiError';
import { authStorage } from '@core/auth/authStorage';
import { ensureFreshAccessToken } from '@core/auth/tokenRefresh';
import { authEvents } from '@core/auth/authEvents';
import { STORAGE_KEYS } from '@core/constants/storageKeys';

export function setupInterceptors(axiosInstance: AxiosInstance): void {
  // Request Interceptor
  axiosInstance.interceptors.request.use(
    async (config: InternalAxiosRequestConfig) => {
      const token = await ensureFreshAccessToken();
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }

      // Add correlation / trace header
      const traceId = `ixapp-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
      if (config.headers) {
        config.headers['X-Correlation-ID'] = traceId;
        try {
          const company = globalThis.localStorage?.getItem(STORAGE_KEYS.COMPANY);
          if (company) config.headers['X-Company'] = company;
        } catch {
          // Company selection is optional when browser storage is unavailable.
        }
      }

      return config;
    },
    (error: AxiosError) => Promise.reject(error)
  );

  // Response Interceptor
  axiosInstance.interceptors.response.use(
    (response: AxiosResponse) => response,
    (error: AxiosError) => {
      if (!error.response) {
        return Promise.reject(
          new ApiError(
            error.message || 'Network error occurred. Please check server connectivity.',
            0
          )
        );
      }

      const { status, data } = error.response;
      const problem = data as ApiValidationProblem;

      if (status === 401) {
        authStorage.clearAll();
        authEvents.emit('session-expired');
      } else if (status === 403) {
        authEvents.emit('access-denied');
      }

      if (problem && (problem.title || problem.detail || problem.errors)) {
        return Promise.reject(ApiError.fromProblem(problem, status));
      }

      return Promise.reject(
        new ApiError(
          (data as { message?: string } | undefined)?.message ||
            error.message ||
            `HTTP Request failed with status ${status}`,
          status
        )
      );
    }
  );
}
