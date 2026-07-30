export const DEFAULT_PAGE_SIZE = 25;
export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];
export const DEFAULT_CURRENCY = 'USD';
export const DEFAULT_COMPANY = 'USMF';
export const AVAILABLE_COMPANIES = [
  { code: 'USMF', name: 'Contoso Entertainment USA' },
  { code: 'DEMO', name: 'Demo Company Entity' },
  { code: 'DAT', name: 'Default Data Entity' },
];

export const STORAGE_KEYS = {
  AUTH_TOKEN: 'ixapp_auth_token',
  USER_PREFERENCES: 'ixapp_user_prefs',
  CURRENT_COMPANY: 'ixapp_current_company',
  LANGUAGE: 'ixapp_language',
} as const;
