import type { AuthAdapter, LoginResponse, UserProfile } from './types';
import { authStorage } from './authStorage';

export const MOCK_USER: UserProfile = {
  id: 'usr-001',
  username: 'admin@ixapp.com',
  email: 'admin@ixapp.com',
  displayName: 'Enterprise Administrator',
  roles: ['SystemAdmin', 'Accountant', 'SalesManager'],
  permissions: [
    'dashboard.view',
    'customer.view',
    'customer.create',
    'customer.update',
    'customer.delete',
    'customerGroup.view',
    'salesOrder.view',
    'salesOrder.create',
    'salesOrder.update',
    'salesOrder.confirm',
    'salesOrder.post',
    'currency.view',
    'currency.manage',
    'settings.view',
    'settings.update',
  ],
  defaultCompany: 'USMF',
};

const MOCK_TOKEN = 'mock-development-token';
let currentMockUser = MOCK_USER;

export const mockAuthAdapter: AuthAdapter = {
  async login(username: string): Promise<LoginResponse> {
    currentMockUser = {
      ...MOCK_USER,
      username,
      displayName: username.split('@')[0] || 'Enterprise User',
    };
    authStorage.setToken(MOCK_TOKEN);
    return { user: currentMockUser, token: MOCK_TOKEN, expiresInSeconds: 86400 };
  },
  async getCurrentUser(): Promise<UserProfile> {
    if (!authStorage.getToken()) authStorage.setToken(MOCK_TOKEN);
    return currentMockUser;
  },
  async refreshToken(): Promise<string> {
    authStorage.setToken(MOCK_TOKEN);
    return MOCK_TOKEN;
  },
  async logout(): Promise<void> {
    authStorage.clearAll();
    currentMockUser = MOCK_USER;
  },
};
