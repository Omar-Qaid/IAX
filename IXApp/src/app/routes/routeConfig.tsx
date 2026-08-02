import React, { lazy, Suspense } from 'react';
import type { RouteObject } from 'react-router-dom';
import { AppLayout } from '@app/layouts/AppLayout';
import { AuthLayout } from '@app/layouts/AuthLayout';
import { RouteGuard } from './RouteGuard';
import { ROUTE_PATHS } from './routePaths';
import { LoadingState } from '@shared/components/feedback/LoadingState';
import { ErrorState } from '@shared/components/feedback/ErrorState';
import { AccessDeniedState } from '@shared/components/feedback/AccessDeniedState';
import { useAppTranslation } from '@core/localization/useAppTranslation';

const LoginPage = lazy(() => import('@modules/auth/pages/LoginPage').then((module) => ({ default: module.LoginPage })));
const DashboardPage = lazy(() => import('@modules/dashboard/pages/DashboardPage').then((module) => ({ default: module.DashboardPage })));
const CustomerListPage = lazy(() => import('@modules/accounts-receivable/pages/CustomerListPage').then((module) => ({ default: module.CustomerListPage })));
const CustomerGroupListPage = lazy(() => import('@modules/accounts-receivable/pages/CustomerGroupListPage').then((module) => ({ default: module.CustomerGroupListPage })));
const CustParametersPage = lazy(() => import('@modules/accounts-receivable/pages/CustParametersPage').then((module) => ({ default: module.CustParametersPage })));
const CustPaymMode = lazy(() => import('@modules/accounts-receivable/pages/CustPaymModePage').then((module) => ({ default: module.CustPaymMode })));
const CustPaymTerm = lazy(() => import('@modules/accounts-receivable/pages/CustPaymTermPage').then((module) => ({ default: module.CustPaymTerm })));
const SalesOrdersPage = lazy(() => import('@modules/accounts-receivable/pages/SalesOrdersPage').then((module) => ({ default: module.SalesOrdersPage })));
const SalesOrderPage = lazy(() => import('@modules/accounts-receivable/pages/SalesOrderPage').then((module) => ({ default: module.SalesOrderPage })));
const CurrenciesPage = lazy(() => import('@modules/foundation/pages/CurrenciesPage').then((module) => ({ default: module.CurrenciesPage })));
const ApplicationSettingsPage = lazy(() => import('@modules/system-administration/pages/ApplicationSettingsPage').then((module) => ({ default: module.ApplicationSettingsPage })));

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

const load = (element: React.ReactNode) => <Suspense fallback={<RouteLoading />}>{element}</Suspense>;

export const appRoutes: RouteObject[] = [
  {
    path: ROUTE_PATHS.LOGIN,
    element: (
      <AuthLayout>
        {load(<LoginPage />)}
      </AuthLayout>
    ),
  },
  {
    path: ROUTE_PATHS.HOME,
    element: (
      <RouteGuard>
        <AppLayout />
      </RouteGuard>
    ),
    children: [
      {
        index: true,
        element: load(<DashboardPage />),
      },
      {
        path: ROUTE_PATHS.DASHBOARD,
        element: load(<DashboardPage />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
        element: load(<CustomerListPage />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS,
        element: load(<CustomerGroupListPage />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS,
        element: load(<CustParametersPage />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS,
        element: load(<CustPaymMode />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_TERMS,
        element: load(<CustPaymTerm />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS,
        element: load(<SalesOrdersPage />),
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDER_DETAILS,
        element: load(<SalesOrderPage />),
      },
      {
        path: ROUTE_PATHS.FOUNDATION.CURRENCIES,
        element: load(<CurrenciesPage />),
      },
      {
        path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
        element: load(<ApplicationSettingsPage />),
      },
      { path: ROUTE_PATHS.ACCESS_DENIED, element: <AccessDeniedPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
];
