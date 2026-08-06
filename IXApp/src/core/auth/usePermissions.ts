import { useAuth } from './useAuth';

const toPermissionCode = (resource: string, action: string): string => {
  const normalizedResource = resource.endsWith('ies')
    ? `${resource.slice(0, -3)}y`
    : resource.endsWith('s') ? resource.slice(0, -1) : resource;
  return `${normalizedResource.charAt(0).toLowerCase()}${normalizedResource.slice(1)}.${action.toLowerCase()}`;
};

/** Compatibility adapter for older module/resource/action consumers. */
export const usePermissions = () => {
  const { user, hasPermission: hasPermissionCode } = useAuth();
  return {
    canView: (_module: string, resource: string) => hasPermissionCode(toPermissionCode(resource, 'view')),
    hasPermission: (_module?: string, resource?: string, action = 'view') =>
      !resource || hasPermissionCode(toPermissionCode(resource, action)),
    isAdmin: user?.roles.includes('SystemAdmin') ?? false,
  };
};
