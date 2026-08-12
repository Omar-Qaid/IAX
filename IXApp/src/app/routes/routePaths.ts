import { WORKFLOW_ROUTE_PATHS } from '@modules/workflow/routes/workflowRoutePaths';

export const ROUTE_PATHS = {
  ROOT: '/',
  HOME: '/',
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  PROCESS_BUILDER: '/process-builder/:builderId',
  PROCESS_BUILDER_NEW: '/process-builder/new',
  processBuilder: (builderId: string | number) =>
    `/process-builder/${encodeURIComponent(String(builderId))}`,

  ACCOUNTS_RECEIVABLE: {
    ROOT: '/accounts-receivable',
    CUSTOMERS: '/accounts-receivable/customers',
    CUSTOMER_DETAILS: '/accounts-receivable/customers/:customerId',
    CUSTOMER_GROUPS: '/accounts-receivable/customer-groups',
    CUSTOMER_PARAMETERS: '/accounts-receivable/customer-parameters',
    CUSTOMER_PAYMENT_METHODS: '/accounts-receivable/customer-payment-methods',
    CUSTOMER_PAYMENT_TERMS: '/accounts-receivable/customer-payment-terms',
    SALES_ORDERS: '/accounts-receivable/sales-orders',
    SALES_ORDER_DETAILS: '/accounts-receivable/sales-orders/:salesOrderId',
    salesOrder: (salesOrderId: string) =>
      `/accounts-receivable/sales-orders/${encodeURIComponent(salesOrderId)}`,
  },

  FOUNDATION: {
    ROOT: '/foundation',
    CURRENCIES: '/foundation/currencies',
    EXCHANGE_RATE_TYPES: '/foundation/exchange-rate-types',
    EXCHANGE_RATES: '/foundation/exchange-rates',
  },

  WORKFLOW: WORKFLOW_ROUTE_PATHS,

  ORGANIZATION_ADMINISTRATION: {
    ROOT: '/organization-administration',
    LEGAL_ENTITIES: '/organization-administration/legal-entities',
  },

  SYSTEM_ADMINISTRATION: {
    ROOT: '/system-administration',
    SETTINGS: '/system-administration/settings',
  },

  ACCESS_DENIED: '/access-denied',
  NOT_FOUND: '/not-found',
} as const;
