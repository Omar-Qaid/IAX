export type Permission = string;
export interface PermissionContextValue { permissions: ReadonlySet<Permission>; hasPermission: (permission?: Permission) => boolean }
