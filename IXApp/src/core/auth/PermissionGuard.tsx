import React from 'react';
import { useAuth } from './useAuth';

export interface PermissionGuardProps {
  permission?: string;
  role?: string;
  fallback?: React.ReactNode;
  children: React.ReactNode;
}

export const PermissionGuard: React.FC<PermissionGuardProps> = ({
  permission,
  role,
  fallback,
  children,
}) => {
  const { hasPermission, hasRole } = useAuth();

  if (permission && !hasPermission(permission)) {
    return fallback ? <>{fallback}</> : null;
  }

  if (role && !hasRole(role)) {
    return fallback ? <>{fallback}</> : null;
  }

  return <>{children}</>;
};
