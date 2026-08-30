import { lazy, type ComponentType, type LazyExoticComponent } from 'react';
import { matchPath } from 'react-router-dom';
import { PERMISSIONS, type PermissionCode } from '@core/permissions/permissions';
import { ROUTE_PATHS } from './routePaths';

export interface AppPageDefinition {
  id: string;
  path: string;
  permission?: PermissionCode;
  component: LazyExoticComponent<ComponentType>;
}

const lazyPage = <TModule extends object>(
  importer: () => Promise<TModule>,
  select: (module: TModule) => ComponentType
) => lazy(async () => ({ default: select(await importer()) }));

export const APP_PAGE_DEFINITIONS: readonly AppPageDefinition[] = [
  {
    id: 'dashboard',
    path: ROUTE_PATHS.DASHBOARD,
    permission: PERMISSIONS.DASHBOARD_VIEW,
    component: lazyPage(
      () => import('@modules/dashboard/pages/DashboardPage'),
      (module) => module.DashboardPage
    ),
  },
  {
    id: 'customers',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
    permission: PERMISSIONS.CUSTOMER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/CustomerListPage'),
      (module) => module.CustomerListPage
    ),
  },
  {
    id: 'customer-groups',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS,
    permission: PERMISSIONS.CUSTOMER_GROUP_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/CustomerGroupListPage'),
      (module) => module.CustomerGroupListPage
    ),
  },
  {
    id: 'customer-parameters',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS,
    permission: PERMISSIONS.CUSTOMER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/CustParametersPage'),
      (module) => module.CustParametersPage
    ),
  },
  {
    id: 'customer-payment-methods',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS,
    permission: PERMISSIONS.CUSTOMER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/CustPaymModePage'),
      (module) => module.CustPaymMode
    ),
  },
  {
    id: 'customer-payment-terms',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_TERMS,
    permission: PERMISSIONS.CUSTOMER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/CustPaymTermPage'),
      (module) => module.CustPaymTerm
    ),
  },
  {
    id: 'sales-orders',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS,
    permission: PERMISSIONS.SALES_ORDER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/SalesOrdersPage'),
      (module) => module.SalesOrdersPage
    ),
  },
  {
    id: 'sales-order-details',
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDER_DETAILS,
    permission: PERMISSIONS.SALES_ORDER_VIEW,
    component: lazyPage(
      () => import('@modules/finance/accounts-receivable/pages/SalesOrderPage'),
      (module) => module.SalesOrderPage
    ),
  },
  {
    id: 'currencies',
    path: ROUTE_PATHS.FOUNDATION.CURRENCIES,
    permission: PERMISSIONS.CURRENCY_VIEW,
    component: lazyPage(
      () => import('@modules/finance/foundation/pages/CurrencyPage'),
      (module) => module.CurrencyPage
    ),
  },
  {
    id: 'exchange-rate-types',
    path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATE_TYPES,
    permission: PERMISSIONS.CURRENCY_VIEW,
    component: lazyPage(
      () => import('@modules/finance/foundation/pages/ExchangeRateTypePage'),
      (module) => module.ExchangeRateTypePage
    ),
  },
  {
    id: 'exchange-rates',
    path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATES,
    permission: PERMISSIONS.CURRENCY_VIEW,
    component: lazyPage(
      () => import('@modules/finance/foundation/pages/ExchangeRatePage'),
      (module) => module.ExchangeRatePage
    ),
  },
  {
    id: 'workflow-mail',
    path: ROUTE_PATHS.WORKFLOW.MAIL,
    component: lazyPage(
      () => import('@modules/workflow/pages/MailPage'),
      (module) => module.MailPage
    ),
  },
  {
    id: 'request-submission',
    path: ROUTE_PATHS.WORKFLOW.REQUEST_SUBMISSION,
    component: lazyPage(
      () => import('@modules/workflow/pages/RequestSubmissionPage'),
      (module) => module.RequestSubmissionPage
    ),
  },
  {
    id: 'request-from',
    path: ROUTE_PATHS.WORKFLOW.REQUEST_FROM,
    component: lazyPage(
      () => import('@modules/workflow/pages/RequestFromPage'),
      (module) => module.RequestFromPage
    ),
  },
  {
    id: 'workflow-processes',
    path: ROUTE_PATHS.WORKFLOW.PROCESSES,
    permission: PERMISSIONS.WF_PROCESS_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WFProcessPage'),
      (module) => module.WFProcessPage
    ),
  },
  {
    id: 'workflow-print-templates',
    path: ROUTE_PATHS.WORKFLOW.PRINT_TEMPLATES,
    permission: PERMISSIONS.WF_PRINT_TEMPLATE_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/print-templates/pages/PrintTemplatesPage'),
      (module) => module.PrintTemplatesPage
    ),
  },
  {
    id: 'workflow-generic-report',
    path: ROUTE_PATHS.WORKFLOW.GENERIC_REPORT,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfGenericReportPage'),
      (module) => module.WfGenericReportPage
    ),
  },
  {
    id: 'process-builder',
    path: ROUTE_PATHS.PROCESS_BUILDER,
    component: lazyPage(
      () => import('@modules/process-builder/pages/ProcessBuilderPage'),
      (module) => module.ProcessBuilderPage
    ),
  },
  {
    id: 'process-builder-new',
    path: ROUTE_PATHS.PROCESS_BUILDER_NEW,
    component: lazyPage(
      () => import('@modules/process-builder/pages/ProcessBuilderPage'),
      (module) => module.ProcessBuilderPage
    ),
  },
  {
    id: 'workflow-categories',
    path: ROUTE_PATHS.WORKFLOW.CATEGORIES,
    permission: PERMISSIONS.WF_CATEGORY_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WFCategoryPage'),
      (module) => module.WFCategoryPage
    ),
  },
  {
    id: 'workflow-activity-types',
    path: ROUTE_PATHS.WORKFLOW.ACTIVITY_TYPES,
    permission: PERMISSIONS.WF_ACTIVITY_TYPE_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfActivityTypesPage'),
      (module) => module.WfActivityTypesPage
    ),
  },
  {
    id: 'workflow-data-types',
    path: ROUTE_PATHS.WORKFLOW.DATA_TYPES,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfDataTypesPage'),
      (module) => module.WfDataTypesPage
    ),
  },
  {
    id: 'workflow-controls',
    path: ROUTE_PATHS.WORKFLOW.CONTROLS,
    permission: PERMISSIONS.WF_CONTROL_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfControlsPage'),
      (module) => module.WfControlsPage
    ),
  },
  {
    id: 'workflow-priorities',
    path: ROUTE_PATHS.WORKFLOW.PRIORITIES,
    permission: PERMISSIONS.WF_PRIORITY_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfPrioritiesPage'),
      (module) => module.WfPrioritiesPage
    ),
  },
  {
    id: 'workflow-variables',
    path: ROUTE_PATHS.WORKFLOW.VARIABLES,
    permission: PERMISSIONS.WF_VARIABLE_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WFVariablesPage'),
      (module) => module.WFVariablesPage
    ),
  },
  {
    id: 'workflow-steps',
    path: ROUTE_PATHS.WORKFLOW.STEPS,
    permission: PERMISSIONS.WF_STEP_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WFStepsPage'),
      (module) => module.WFStepsPage
    ),
  },
  {
    id: 'workflow-activities',
    path: ROUTE_PATHS.WORKFLOW.ACTIVITIES,
    permission: PERMISSIONS.WF_ACTIVITY_VIEW,
    component: lazyPage(
      () => import('@modules/workflow/pages/WfActivitiesPage'),
      (module) => module.WfActivitiesPage
    ),
  },
  {
    id: 'legal-entities',
    path: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES,
    permission: PERMISSIONS.LEGAL_ENTITY_VIEW,
    component: lazyPage(
      () => import('@modules/organization/pages/LegalEntityPage'),
      (module) => module.LegalEntityPage
    ),
  },
  {
    id: 'application-settings',
    path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
    permission: PERMISSIONS.SETTINGS_VIEW,
    component: lazyPage(
      () => import('@modules/administration/pages/ApplicationSettingsPage'),
      (module) => module.ApplicationSettingsPage
    ),
  },
  {
    id: 'system-number-sequences',
    path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.NUMBER_SEQUENCES,
    permission: PERMISSIONS.NUMBER_SEQUENCE_VIEW,
    component: lazyPage(
      () => import('@modules/administration/pages/SysNumberSequencePage'),
      (module) => module.SysNumberSequencePage
    ),
  },
] as const;

export const getPageDefinition = (path: string): AppPageDefinition | undefined =>
  APP_PAGE_DEFINITIONS.find((page) => page.path === path);

export const findPageDefinitionForPath = (pathname: string): AppPageDefinition | undefined =>
  APP_PAGE_DEFINITIONS.find((page) => matchPath({ path: page.path, end: true }, pathname));

export const getRoutePermission = (path?: string): PermissionCode | undefined =>
  path ? getPageDefinition(path)?.permission : undefined;
