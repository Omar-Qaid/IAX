import { useAuth } from '@core/auth/useAuth';

export function usePermission(permission?: string): { hasPermission: boolean } {
  const { hasPermission } = useAuth();
  if (!permission) return { hasPermission: true };
  return { hasPermission: hasPermission(permission) };
}
