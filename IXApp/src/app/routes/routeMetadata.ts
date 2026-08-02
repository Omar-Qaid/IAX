import { matchPath } from 'react-router-dom';
import { ROUTE_PATHS } from './routePaths';

export interface BreadcrumbDefinition {
  labelKey: string;
  path?: string;
}

interface RouteMetadata {
  path: string;
  breadcrumbs: BreadcrumbDefinition[];
}

const home: BreadcrumbDefinition = { labelKey: 'nav.home', path: ROUTE_PATHS.DASHBOARD };

export const ROUTE_METADATA: RouteMetadata[] = [
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.customers' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.customerGroups' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.customerParameters' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.customerPaymentMethods' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_TERMS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.customerPaymentTerms' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.salesOrders' }] },
  { path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDER_DETAILS, breadcrumbs: [home, { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS }, { labelKey: 'nav.salesOrders', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS }, { labelKey: 'pages.salesOrder.breadcrumb' }] },
  { path: ROUTE_PATHS.FOUNDATION.CURRENCIES, breadcrumbs: [home, { labelKey: 'nav.generalLedger', path: ROUTE_PATHS.FOUNDATION.CURRENCIES }, { labelKey: 'nav.currencies' }] },
  { path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATE_TYPES, breadcrumbs: [home, { labelKey: 'nav.generalLedger', path: ROUTE_PATHS.FOUNDATION.CURRENCIES }, { labelKey: 'nav.exchangeRateTypes' }] },
  { path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATES, breadcrumbs: [home, { labelKey: 'nav.generalLedger', path: ROUTE_PATHS.FOUNDATION.CURRENCIES }, { labelKey: 'nav.exchangeRates' }] },
  { path: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES, breadcrumbs: [home, { labelKey: 'nav.organizationAdministration', path: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES }, { labelKey: 'nav.legalEntities' }] },
  { path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS, breadcrumbs: [home, { labelKey: 'nav.systemAdmin', path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS }, { labelKey: 'nav.settings' }] },
];

export const getRouteBreadcrumbs = (pathname: string): BreadcrumbDefinition[] =>
  ROUTE_METADATA.find((route) => matchPath({ path: route.path, end: true }, pathname))?.breadcrumbs ?? [home];
