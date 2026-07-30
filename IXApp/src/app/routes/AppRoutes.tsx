import React, { Suspense } from 'react';
import { useRoutes, BrowserRouter } from 'react-router-dom';
import { appRoutes } from './routeConfig';
import { LoadingState } from '@shared/components/feedback/LoadingState';

const AppRoutesContent: React.FC = () => {
  const element = useRoutes(appRoutes);
  return <Suspense fallback={<LoadingState message="Loading enterprise module..." />}>{element}</Suspense>;
};

export const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <AppRoutesContent />
    </BrowserRouter>
  );
};
