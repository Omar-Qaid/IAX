import { environment } from '@core/configuration/environment';
import { apiAuthAdapter } from './apiAuthAdapter';
import { authStorage } from './authStorage';
import { mockAuthAdapter, MOCK_USER } from './mockAuthAdapter';
import type { AuthAdapter, LoginResponse, UserProfile } from './types';

const adapter: AuthAdapter = environment.enableMockApi ? mockAuthAdapter : apiAuthAdapter;

export const authService = {
  getInitialUser(): UserProfile | null {
    return environment.enableMockApi ? MOCK_USER : null;
  },
  hasSession(): boolean {
    return environment.enableMockApi || Boolean(authStorage.getToken());
  },
  getCurrentUser(): Promise<UserProfile> {
    return adapter.getCurrentUser();
  },
  login(username: string, password: string): Promise<LoginResponse> {
    return adapter.login(username, password);
  },
  refreshToken(): Promise<string> {
    return adapter.refreshToken();
  },
  logout(): Promise<void> {
    return adapter.logout();
  },
};
