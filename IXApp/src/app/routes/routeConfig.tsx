import { lazy } from 'react';
import type { RouteObject } from 'react-router-dom';
import { AppLayout } from '@app/layouts/AppLayout';
import { AuthLayout } from '@app/layouts/AuthLayout';
import { RouteGuard } from './RouteGuard';
import { ROUTE_PATHS } from './routePaths';

const LoginPage = lazy(() => import('@modules/auth/pages/LoginPage'));
const DashboardPage = lazy(() =>
  import('@modules/dashboard/pages/DashboardPage').then((m) => ({ default: m.DashboardPage }))
);
const CustomersPage = lazy(() =>
  import('@modules/accounts-receivable/customers/pages/CustomersPage').then((m) => ({
    default: m.CustomersPage,
  }))
);
const CustomerGroupsPage = lazy(() =>
  import('@modules/accounts-receivable/customer-groups/pages/CustomerGroupsPage').then((m) => ({
    default: m.CustomerGroupsPage,
  }))
);
const SalesOrdersPage = lazy(() =>
  import('@modules/accounts-receivable/sales-orders/pages/SalesOrdersPage').then((m) => ({
    default: m.SalesOrdersPage,
  }))
);
const SalesOrderPage = lazy(() =>
  import('@modules/accounts-receivable/sales-orders/pages/SalesOrderPage').then((m) => ({
    default: m.SalesOrderPage,
  }))
);
const CurrenciesPage = lazy(() =>
  import('@modules/foundation/currencies/pages/CurrenciesPage').then((m) => ({
    default: m.CurrenciesPage,
  }))
);
const ApplicationSettingsPage = lazy(() =>
  import('@modules/system-administration/settings/pages/ApplicationSettingsPage').then((m) => ({
    default: m.ApplicationSettingsPage,
  }))
);

export const appRoutes: RouteObject[] = [
  {
    path: ROUTE_PATHS.LOGIN,
    element: (
      <AuthLayout>
        <LoginPage />
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
        element: <DashboardPage />,
      },
      {
        path: ROUTE_PATHS.DASHBOARD,
        element: <DashboardPage />,
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
        element: <CustomersPage />,
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS,
        element: <CustomerGroupsPage />,
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS,
        element: <SalesOrdersPage />,
      },
      {
        path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDER_DETAILS,
        element: <SalesOrderPage />,
      },
      {
        path: ROUTE_PATHS.FOUNDATION.CURRENCIES,
        element: <CurrenciesPage />,
      },
      {
        path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
        element: <ApplicationSettingsPage />,
      },
    ],
  },
];
