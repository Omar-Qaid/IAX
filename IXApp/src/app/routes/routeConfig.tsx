import React from 'react';
import type { RouteObject } from 'react-router-dom';
import { AppLayout } from '@app/layouts/AppLayout';
import { AuthLayout } from '@app/layouts/AuthLayout';
import { RouteGuard } from './RouteGuard';
import { ROUTE_PATHS } from './routePaths';

const makePlaceholder = (title: string) => () => (
  <div style={{ padding: 24 }}>
    <h2>{title} Page</h2>
    <p>This module page will be implemented in a future phase.</p>
  </div>
);

const LoginPage = makePlaceholder('Login');
const DashboardPage = makePlaceholder('Dashboard');
const CustomersPage = makePlaceholder('Customers');
const CustomerGroupsPage = makePlaceholder('Customer Groups');
const SalesOrdersPage = makePlaceholder('Sales Orders');
const SalesOrderPage = makePlaceholder('Sales Order Details');
const CurrenciesPage = makePlaceholder('Currencies');
const ApplicationSettingsPage = makePlaceholder('Application Settings');

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
