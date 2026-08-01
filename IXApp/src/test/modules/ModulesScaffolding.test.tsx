import { describe, it, expect } from 'vitest';
import { AUTH_MODULE_ID, DASHBOARD_MODULE_ID, ACCOUNTS_RECEIVABLE_MODULE_ID } from '@modules/index';

describe('Enterprise Core Module Scaffolding', () => {
  it('exports core module identifiers correctly', () => {
    expect(AUTH_MODULE_ID).toBe('auth');
    expect(DASHBOARD_MODULE_ID).toBe('dashboard');
    expect(ACCOUNTS_RECEIVABLE_MODULE_ID).toBe('accounts-receivable');
  });
});
