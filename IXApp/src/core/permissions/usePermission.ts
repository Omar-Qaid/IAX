import { useAuth } from '@core/auth/useAuth';
import type { PermissionCode } from './permissions';

export function usePermission(permission?: PermissionCode | string): { hasPermission: boolean } {
  const { hasPermission } = useAuth();
  if (!permission) return { hasPermission: true };
  return { hasPermission: hasPermission(permission) };
}
