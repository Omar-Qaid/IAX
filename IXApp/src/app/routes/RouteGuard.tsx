import React from 'react';
import { useAuth } from '@core/auth/useAuth';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { LoadingState } from '@shared/components/feedback/LoadingState';

export interface RouteGuardProps {
  permission?: string;
  children?: React.ReactNode;
}

export function RouteGuard({ permission, children }: RouteGuardProps): React.ReactElement {
  const { isAuthenticated, isLoading, hasPermission } = useAuth();

  if (isLoading) {
    return <LoadingState message="Verifying security credentials..." />;
  }

  if (!isAuthenticated) {
    return <AccessDeniedState message="Authentication required to view this module." />;
  }

  if (permission && !hasPermission(permission)) {
    return <AccessDeniedState message={`Permission required: ${permission}`} />;
  }

  return <>{children}</>;
}
