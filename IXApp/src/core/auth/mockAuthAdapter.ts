import type { AuthAdapter, LoginResponse, UserProfile } from './types';
import { authStorage } from './authStorage';
import { PERMISSIONS } from '@core/permissions/permissions';

export const MOCK_USER: UserProfile = {
  id: 'usr-001',
  username: 'admin@ixapp.com',
  email: 'admin@ixapp.com',
  displayName: 'Enterprise Administrator',
  roles: ['SystemAdmin', 'Accountant', 'SalesManager'],
  permissions: [
    PERMISSIONS.DASHBOARD_VIEW,
    PERMISSIONS.CUSTOMER_VIEW,
    PERMISSIONS.CUSTOMER_CREATE,
    PERMISSIONS.CUSTOMER_UPDATE,
    PERMISSIONS.CUSTOMER_DELETE,
    PERMISSIONS.CUSTOMER_GROUP_VIEW,
    PERMISSIONS.SALES_ORDER_VIEW,
    PERMISSIONS.SALES_ORDER_CREATE,
    PERMISSIONS.SALES_ORDER_UPDATE,
    PERMISSIONS.SALES_ORDER_CONFIRM,
    PERMISSIONS.SALES_ORDER_POST,
    PERMISSIONS.CURRENCY_VIEW,
    PERMISSIONS.CURRENCY_MANAGE,
    PERMISSIONS.MARKUP_TABLE_VIEW,
    PERMISSIONS.MARKUP_TABLE_CREATE,
    PERMISSIONS.MARKUP_TABLE_EDIT,
    PERMISSIONS.MARKUP_TABLE_DELETE,
    PERMISSIONS.TAX_AUTHORITY_VIEW,
    PERMISSIONS.TAX_AUTHORITY_CREATE,
    PERMISSIONS.TAX_AUTHORITY_EDIT,
    PERMISSIONS.TAX_AUTHORITY_DELETE,
    PERMISSIONS.TAX_PERIOD_VIEW,
    PERMISSIONS.TAX_PERIOD_CREATE,
    PERMISSIONS.TAX_PERIOD_EDIT,
    PERMISSIONS.TAX_PERIOD_DELETE,
    PERMISSIONS.TAX_CODE_VIEW,
    PERMISSIONS.TAX_CODE_CREATE,
    PERMISSIONS.TAX_CODE_EDIT,
    PERMISSIONS.TAX_CODE_DELETE,
    PERMISSIONS.TAX_GROUP_VIEW,
    PERMISSIONS.TAX_GROUP_CREATE,
    PERMISSIONS.TAX_GROUP_EDIT,
    PERMISSIONS.TAX_GROUP_DELETE,
    PERMISSIONS.TAX_ITEM_GROUP_VIEW,
    PERMISSIONS.TAX_ITEM_GROUP_CREATE,
    PERMISSIONS.TAX_ITEM_GROUP_EDIT,
    PERMISSIONS.TAX_ITEM_GROUP_DELETE,
    PERMISSIONS.SETTINGS_VIEW,
    PERMISSIONS.SETTINGS_UPDATE,
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
