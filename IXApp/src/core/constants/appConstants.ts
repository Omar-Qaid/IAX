export const DEFAULT_PAGE_SIZE = 25;
export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];
export const DEFAULT_CURRENCY = 'USD';
export const DEFAULT_COMPANY = 'USMF';
export const AVAILABLE_COMPANIES = [
  { code: 'USMF', name: 'Contoso Entertainment USA' },
  { code: 'DEMO', name: 'Demo Company Entity' },
  { code: 'DAT', name: 'Default Data Entity' },
];

// Compatibility aliases. New code should import STORAGE_KEYS from storageKeys.ts.
export { STORAGE_KEYS } from './storageKeys';
