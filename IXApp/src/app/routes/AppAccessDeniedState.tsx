import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import {
  AccessDeniedState,
  type AccessDeniedStateProps,
} from '@shared/components/feedback/AccessDeniedState';
import { ROUTE_PATHS } from './routePaths';

export type AppAccessDeniedStateProps = Pick<AccessDeniedStateProps, 'title' | 'message'>;

export function AppAccessDeniedState(props: AppAccessDeniedStateProps): React.ReactElement {
  const navigate = useNavigate();
  const { t } = useAppTranslation();

  return (
    <AccessDeniedState
      {...props}
      actionLabel={t('actions.backToDashboard')}
      onAction={() => navigate(ROUTE_PATHS.DASHBOARD)}
    />
  );
}
