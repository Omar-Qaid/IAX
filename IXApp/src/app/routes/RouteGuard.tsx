import React from 'react';
import { useAuth } from '@core/auth/useAuth';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppAccessDeniedState } from './AppAccessDeniedState';

export interface RouteGuardProps {
  permission?: string;
  children?: React.ReactNode;
}

export function RouteGuard({ permission, children }: RouteGuardProps): React.ReactElement {
  const { isAuthenticated, isLoading, hasPermission } = useAuth();
  const { t } = useAppTranslation();

  if (isLoading) {
    return <LoadingState message={t('messages.verifyingCredentials')} />;
  }

  if (!isAuthenticated) {
    return <AppAccessDeniedState message={t('messages.authenticationRequired')} />;
  }

  if (permission && !hasPermission(permission)) {
    return <AppAccessDeniedState message={t('messages.permissionRequired', { permission })} />;
  }

  return <>{children}</>;
}
