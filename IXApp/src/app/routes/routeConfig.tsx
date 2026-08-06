import React, { lazy, Suspense } from 'react';
import { useRouteError, type RouteObject } from 'react-router-dom';
import { AppLayout } from '@app/layouts/AppLayout';
import { AuthLayout } from '@app/layouts/AuthLayout';
import { RouteGuard } from './RouteGuard';
import { ROUTE_PATHS } from './routePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { APP_PAGE_DEFINITIONS, getPageDefinition } from './pageRegistry';

const LoginPage = lazy(() => import('@modules/auth/pages/LoginPage').then((module) => ({ default: module.LoginPage })));

const RouteLoading = () => {
  const { t } = useAppTranslation();
  return <LoadingState message={t('messages.loadingPage')} />;
};

const NotFoundPage = () => {
  const { t } = useAppTranslation();
  return <ErrorState title={t('pages.notFound.title')} message={t('pages.notFound.message')} />;
};

const AccessDeniedPage = () => {
  const { t } = useAppTranslation();
  return <AccessDeniedState title={t('pages.accessDenied.title')} message={t('pages.accessDenied.message')} />;
};

const RouteErrorPage = () => {
  const { t } = useAppTranslation();
  const routeError = useRouteError();
  const message = routeError instanceof Error ? routeError.message : t('errors.generic');
  return <ErrorState title={t('errors.boundaryTitle', 'Application error')} message={message} />;
};

const load = (element: React.ReactNode) => <Suspense fallback={<RouteLoading />}>{element}</Suspense>;
const pageRoutes: RouteObject[] = APP_PAGE_DEFINITIONS.map(page => ({
  path: page.path,
  element: <RouteGuard permission={page.permission}>{load(React.createElement(page.component))}</RouteGuard>,
}));

const dashboard = getPageDefinition(ROUTE_PATHS.DASHBOARD);

export const appRoutes: RouteObject[] = [
  {
    path: ROUTE_PATHS.LOGIN,
    element: (
      <AuthLayout>
        {load(<LoginPage />)}
      </AuthLayout>
    ),
    errorElement: <RouteErrorPage />,
  },
  {
    path: ROUTE_PATHS.HOME,
    element: (
      <RouteGuard>
        <AppLayout />
      </RouteGuard>
    ),
    errorElement: <RouteErrorPage />,
    children: [
      ...(dashboard ? [{ index: true, element: <RouteGuard permission={dashboard.permission}>{load(React.createElement(dashboard.component))}</RouteGuard> }] : []),
      ...pageRoutes,
      { path: ROUTE_PATHS.ACCESS_DENIED, element: <AccessDeniedPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
];
