export const usePermissions = () => {
  return {
    canView: (_module: string, _resource: string) => true,
    hasPermission: (_module?: string, _resource?: string, _action?: string) => true,
    isAdmin: true,
  };
};
