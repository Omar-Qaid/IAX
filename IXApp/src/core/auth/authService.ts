import type { UserProfile, LoginResponse } from './types';
import { authStorage } from './authStorage';

// Enterprise default admin user for initial dev/mock mode
const MOCK_USER: UserProfile = {
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

export const authService = {
  async getCurrentUser(): Promise<UserProfile> {
    const cachedUser = authStorage.getUser<UserProfile>();
    if (cachedUser) {
      return cachedUser;
    }
    // Return mock user for dev
    authStorage.setUser(MOCK_USER);
    authStorage.setToken('mock-jwt-token-12345');
    return MOCK_USER;
  },

  async login(username: string): Promise<LoginResponse> {
    const user: UserProfile = {
      ...MOCK_USER,
      username,
      displayName: username.split('@')[0] || 'Enterprise User',
    };
    const token = 'mock-jwt-token-12345';
    authStorage.setToken(token);
    authStorage.setUser(user);
    return { user, token, expiresInSeconds: 86400 };
  },

  async logout(): Promise<void> {
    authStorage.clearAll();
  },
};
