import axios, { type AxiosError } from 'axios';
import { apiConfig } from '@core/api/apiConfig';
import type { ApiResponse } from '@core/api/apiResponse';
import { ApiError } from '@core/api/apiError';
import { authStorage } from './authStorage';
import { getTokenLifetimeSeconds } from './jwtUtils';
import type { AuthAdapter, LoginResponse, UserProfile } from './types';

interface AccessTokenDto {
  accessToken: string;
}

interface ApiUserDto {
  id: string;
  userName: string;
  email?: string | null;
  employeeName?: string | null;
  photoUrl?: string | null;
  roles: string[];
  permissions: string[];
}

const AUTH_ENDPOINTS = {
  login: '/v1/Auth/login',
  logout: '/v1/Auth/logout',
  me: '/v1/Auth/me',
  refresh: '/v1/Auth/refresh-token',
} as const;

const authHttpClient = axios.create({
  baseURL: apiConfig.baseUrl,
  timeout: apiConfig.timeoutMs,
  headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
});

const authorization = (): { Authorization: string } => {
  const token = authStorage.getToken();
  if (!token) throw new ApiError('No authenticated session is available.', 401);
  return { Authorization: `Bearer ${token}` };
};

const mapApiError = (error: unknown): ApiError => {
  const axiosError = error as AxiosError<ApiResponse<unknown>>;
  const status = axiosError.response?.status ?? 0;
  const response = axiosError.response?.data;
  const validationMessage = response?.errors?.filter(Boolean).join(' ');
  return new ApiError(
    validationMessage ||
      response?.message ||
      (status === 0
        ? `Unable to reach IXApi at ${apiConfig.baseUrl}. Confirm the API is running and its development certificate is trusted.`
        : axiosError.message) ||
      'Authentication request failed.',
    status
  );
};

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || 'The server returned an empty authentication response.',
      500
    );
  }
  return response.data;
};

const mapUser = (user: ApiUserDto): UserProfile => ({
  id: user.id,
  username: user.userName,
  email: user.email ?? user.userName,
  displayName: user.employeeName?.trim() || user.userName,
  roles: user.roles ?? [],
  permissions: user.permissions ?? [],
  ...(user.photoUrl ? { avatarUrl: user.photoUrl } : {}),
});

export const apiAuthAdapter: AuthAdapter = {
  async login(username: string, password: string): Promise<LoginResponse> {
    try {
      const response = await authHttpClient.post<ApiResponse<AccessTokenDto>>(
        AUTH_ENDPOINTS.login,
        {
          username,
          password,
        }
      );
      const { accessToken } = requireData(response.data);
      authStorage.setToken(accessToken);
      const user = await this.getCurrentUser();
      return { user, token: accessToken, expiresInSeconds: getTokenLifetimeSeconds(accessToken) };
    } catch (error) {
      authStorage.clearAll();
      if (error instanceof ApiError) throw error;
      throw mapApiError(error);
    }
  },

  async getCurrentUser(): Promise<UserProfile> {
    try {
      const response = await authHttpClient.get<ApiResponse<ApiUserDto>>(AUTH_ENDPOINTS.me, {
        headers: authorization(),
      });
      return mapUser(requireData(response.data));
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw mapApiError(error);
    }
  },

  async refreshToken(): Promise<string> {
    try {
      const response = await authHttpClient.post<ApiResponse<AccessTokenDto>>(
        AUTH_ENDPOINTS.refresh,
        undefined,
        { headers: authorization() }
      );
      const { accessToken } = requireData(response.data);
      authStorage.setToken(accessToken);
      return accessToken;
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw mapApiError(error);
    }
  },

  async logout(): Promise<void> {
    try {
      if (authStorage.getToken()) {
        await authHttpClient.post(AUTH_ENDPOINTS.logout, undefined, { headers: authorization() });
      }
    } catch {
      // Local logout must complete even when token revocation is unavailable.
    } finally {
      authStorage.clearAll();
    }
  },
};
