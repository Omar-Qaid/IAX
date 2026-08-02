import React, { Suspense } from 'react';
import { useRoutes, BrowserRouter } from 'react-router-dom';
import { appRoutes } from './routeConfig';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { useAppTranslation } from '@core/localization/useAppTranslation';

const AppRoutesContent: React.FC = () => {
  const element = useRoutes(appRoutes);
  const { t } = useAppTranslation();
  return <Suspense fallback={<LoadingState message={t('messages.loadingModule')} />}>{element}</Suspense>;
};

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <AppRoutesContent />
    </BrowserRouter>
  );
};
