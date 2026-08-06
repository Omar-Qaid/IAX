import type { UserProfile } from '@core/auth/types';
import type { PermissionCode } from './permissions';

export class PermissionService {
  private userPermissions: Set<string> = new Set();

  public setPermissions(permissions: string[]): void {
    this.userPermissions = new Set(permissions);
  }

  public hasPermission(permission?: PermissionCode | string): boolean {
    if (!permission) return true;
    return this.userPermissions.has(permission);
  }

  public hasAllPermissions(permissions: (PermissionCode | string)[]): boolean {
    return permissions.every((p) => this.hasPermission(p));
  }

  public hasAnyPermission(permissions: (PermissionCode | string)[]): boolean {
    return permissions.some((p) => this.hasPermission(p));
  }
}

export const permissionService = new PermissionService();

export const userHasPermission = (
  user: UserProfile | null,
  permission?: PermissionCode | string,
): boolean => {
  if (!permission) return true;
  if (!user) return false;
  return user.roles.includes('SystemAdmin')
    || user.permissions.includes('*')
    || user.permissions.includes(permission);
};
