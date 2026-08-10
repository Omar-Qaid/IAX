import React from 'react';
import { useAuth } from '@core/auth/useAuth';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppAccessDeniedState } from './AppAccessDeniedState';
import { Navigate, useLocation } from 'react-router-dom';
import { ROUTE_PATHS } from './routePaths';

export interface RouteGuardProps {
  permission?: string;
  children?: React.ReactNode;
}

export function RouteGuard({ permission, children }: RouteGuardProps): React.ReactElement {
  const { isAuthenticated, isLoading, hasPermission } = useAuth();
  const { t } = useAppTranslation();
  const location = useLocation();

  if (isLoading) {
    return <LoadingState message={t('messages.verifyingCredentials')} />;
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to={ROUTE_PATHS.LOGIN}
        replace
        state={{ returnTo: location.pathname + location.search }}
      />
    );
  }

  if (permission && !hasPermission(permission)) {
    return <AppAccessDeniedState message={t('messages.permissionRequired', { permission })} />;
  }

  return <>{children}</>;
}
