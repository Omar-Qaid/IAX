import React from 'react';
import { useAuth } from './useAuth';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';

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
    return fallback ? <>{fallback}</> : <AccessDeniedState />;
  }

  if (role && !hasRole(role)) {
    return fallback ? <>{fallback}</> : <AccessDeniedState />;
  }

  return <>{children}</>;
};
