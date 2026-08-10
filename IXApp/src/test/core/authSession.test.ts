import axios, { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { setupInterceptors } from '@core/api/interceptors';
import { authEvents, type AuthEvent } from '@core/auth/authEvents';
import { authService } from '@core/auth/authService';
import { authStorage } from '@core/auth/authStorage';
import { getTokenExpiration } from '@core/auth/jwtUtils';
import { ensureFreshAccessToken } from '@core/auth/tokenRefresh';

const jwt = (expiresAt: number): string => {
  const payload = btoa(JSON.stringify({ exp: Math.floor(expiresAt / 1000) }))
    .replace(/=/g, '')
    .replace(/\+/g, '-')
    .replace(/\//g, '_');
  return `header.${payload}.signature`;
};

afterEach(() => {
  authStorage.clearAll();
  localStorage.clear();
  sessionStorage.clear();
  vi.restoreAllMocks();
});

describe('authentication session foundation', () => {
  it('stores access tokens in session storage and removes legacy local storage values', () => {
    localStorage.setItem('ixapp_auth_token', 'legacy');
    localStorage.setItem('ixapp_auth_user', '{"id":"legacy"}');

    authStorage.setToken('current');

    expect(authStorage.getToken()).toBe('current');
    expect(sessionStorage.getItem('ixapp_auth_token')).toBe('current');
    expect(localStorage.getItem('ixapp_auth_token')).toBeNull();
    expect(localStorage.getItem('ixapp_auth_user')).toBeNull();
  });

  it('decodes JWT expiration and coordinates one proactive refresh', async () => {
    const currentToken = jwt(Date.now() + 30_000);
    const refreshedToken = jwt(Date.now() + 600_000);
    authStorage.setToken(currentToken);
    const refresh = vi.spyOn(authService, 'refreshToken').mockImplementation(async () => {
      authStorage.setToken(refreshedToken);
      return refreshedToken;
    });

    const [first, second] = await Promise.all([ensureFreshAccessToken(), ensureFreshAccessToken()]);

    expect(getTokenExpiration(currentToken)).not.toBeNull();
    expect(first).toBe(refreshedToken);
    expect(second).toBe(refreshedToken);
    expect(refresh).toHaveBeenCalledTimes(1);
  });

  it('clears an expired session and publishes the session-expired event', async () => {
    authStorage.setToken(jwt(Date.now() - 1_000));
    const events: AuthEvent[] = [];
    const unsubscribe = authEvents.subscribe((event) => events.push(event));

    await expect(ensureFreshAccessToken()).rejects.toMatchObject({ status: 401 });

    unsubscribe();
    expect(authStorage.getToken()).toBeNull();
    expect(events).toContain('session-expired');
  });

  it('adds bearer and company headers to authenticated requests', async () => {
    const instance = axios.create();
    setupInterceptors(instance);
    authStorage.setToken('opaque-development-token');
    localStorage.setItem('ixapp_current_company', 'USMF');
    let captured: InternalAxiosRequestConfig | undefined;
    instance.defaults.adapter = async (config) => {
      captured = config;
      return { data: {}, status: 200, statusText: 'OK', headers: {}, config };
    };

    await instance.get('/resource');

    expect(captured?.headers.Authorization).toBe('Bearer opaque-development-token');
    expect(captured?.headers['X-Company']).toBe('USMF');
    expect(captured?.headers['X-Correlation-ID']).toMatch(/^ixapp-/);
  });

  it('publishes access-denied without clearing a valid session on 403', async () => {
    const instance = axios.create();
    setupInterceptors(instance);
    authStorage.setToken('valid-token');
    const events: AuthEvent[] = [];
    const unsubscribe = authEvents.subscribe((event) => events.push(event));
    instance.defaults.adapter = async (config) => {
      const response: AxiosResponse = {
        data: { title: 'Company access denied', status: 403 },
        status: 403,
        statusText: 'Forbidden',
        headers: {},
        config,
      };
      throw new AxiosError('Forbidden', 'ERR_BAD_REQUEST', config, undefined, response);
    };

    await expect(instance.get('/resource')).rejects.toMatchObject({ status: 403 });

    unsubscribe();
    expect(authStorage.getToken()).toBe('valid-token');
    expect(events).toContain('access-denied');
  });
});
