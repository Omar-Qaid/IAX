import { useAuth } from '@core/auth/useAuth';
import { permissionService } from './permissionService';
import type { PermissionCode } from './permissions';

export function usePermission(permission?: PermissionCode | string): { hasPermission: boolean } {
  const { user } = useAuth();
  if (!permission) return { hasPermission: true };
  if (user?.permissions) {
    permissionService.setPermissions(user.permissions);
  }
  return { hasPermission: permissionService.hasPermission(permission) };
}
