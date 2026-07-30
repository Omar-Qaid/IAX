export const ROUTE_PATHS = {
  ROOT: '/',
  HOME: '/',
  LOGIN: '/login',
  DASHBOARD: '/dashboard',

  ACCOUNTS_RECEIVABLE: {
    ROOT: '/accounts-receivable',
    CUSTOMERS: '/accounts-receivable/customers',
    CUSTOMER_DETAILS: '/accounts-receivable/customers/:customerId',
    CUSTOMER_GROUPS: '/accounts-receivable/customer-groups',
    SALES_ORDERS: '/accounts-receivable/sales-orders',
    SALES_ORDER_DETAILS: '/accounts-receivable/sales-orders/:salesOrderId',
  },

  FOUNDATION: {
    ROOT: '/foundation',
    CURRENCIES: '/foundation/currencies',
  },

  SYSTEM_ADMINISTRATION: {
    ROOT: '/system-administration',
    SETTINGS: '/system-administration/settings',
  },

  ACCESS_DENIED: '/access-denied',
  NOT_FOUND: '/not-found',
} as const;
