import { PERMISSIONS, type PermissionCode } from './permissions';

export class PermissionService {
  private userPermissions: Set<string> = new Set(Object.values(PERMISSIONS));

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
