import { describe, expect, it } from 'vitest';
import { userHasPermission } from '@core/permissions/permissionService';
import type { UserProfile } from '@core/auth/types';

const user = (permissions: string[], roles: string[] = []): UserProfile => ({
  id: '1', username: 'user', email: 'user@example.com', displayName: 'User', roles, permissions,
});

describe('userHasPermission', () => {
  it('denies anonymous users and permissions not assigned to a user', () => {
    expect(userHasPermission(null, 'customer.view')).toBe(false);
    expect(userHasPermission(user([]), 'customer.view')).toBe(false);
  });

  it('supports explicit, wildcard and system administrator access', () => {
    expect(userHasPermission(user(['customer.view']), 'customer.view')).toBe(true);
    expect(userHasPermission(user(['*']), 'currency.view')).toBe(true);
    expect(userHasPermission(user([], ['SystemAdmin']), 'settings.view')).toBe(true);
  });
});
