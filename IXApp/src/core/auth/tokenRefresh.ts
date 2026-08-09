import { ApiError } from '@core/api/apiError';
import { authEvents } from './authEvents';
import { authService } from './authService';
import { authStorage } from './authStorage';
import { getTokenExpiration } from './jwtUtils';

const REFRESH_WINDOW_MS = 60_000;
let refreshPromise: Promise<string> | null = null;

export const ensureFreshAccessToken = async (): Promise<string | null> => {
  const token = authStorage.getToken();
  if (!token) return null;

  const expiresAt = getTokenExpiration(token);
  if (!expiresAt) return token;
  if (expiresAt - Date.now() > REFRESH_WINDOW_MS) return token;

  if (expiresAt <= Date.now()) {
    authStorage.clearAll();
    authEvents.emit('session-expired');
    throw new ApiError('Your session has expired.', 401);
  }

  refreshPromise ??= authService.refreshToken().finally(() => {
    refreshPromise = null;
  });

  try {
    return await refreshPromise;
  } catch (error) {
    authStorage.clearAll();
    authEvents.emit('session-expired');
    throw error;
  }
};
