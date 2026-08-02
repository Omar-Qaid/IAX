import { ROUTE_PATHS } from '@app/routes/routePaths';

export interface ModuleNavLink {
  label: string;
  path?: string;
  icon?: string;
  expandable?: boolean;
  permission?: { module: string; resource: string };
}

export interface ModuleNavSection {
  id: string;
  title: string;
  links: ModuleNavLink[];
  bordered?: boolean;
}

export interface ModuleNavConfig {
  moduleId: string;
  label: string;
  icon: string;
  defaultPath: string;
  matchPath: string;
  sections: ModuleNavSection[];
}

export const MODULE_NAV_CONFIGS: Record<string, ModuleNavConfig> = {
  'mod-AccountsReceivable': {
    moduleId: 'mod-AccountsReceivable',
    label: 'nav.accountsReceivable',
    icon: 'receipt',
    defaultPath: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
    matchPath: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.ROOT,
    sections: [
      {
        id: 'customers',
        title: 'nav.customers',
        links: [
          { label: 'nav.allCustomers', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS, permission: { module: 'AccountsReceivable', resource: 'Customers' } },
          { label: 'nav.customerGroups', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS, permission: { module: 'AccountsReceivable', resource: 'CustomerGroups' } },
          { label: 'nav.customerParameters', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS, permission: { module: 'AccountsReceivable', resource: 'Customers' } },
          { label: 'nav.customerPaymentMethods', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS, permission: { module: 'AccountsReceivable', resource: 'Customers' } },
        ],
      },
      {
        id: 'orders',
        title: 'nav.orders',
        links: [
          { label: 'nav.salesOrders', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS, permission: { module: 'AccountsReceivable', resource: 'SalesOrders' } },
        ],
      },
    ],
  },
  'mod-GeneralLedger': {
    moduleId: 'mod-GeneralLedger',
    label: 'nav.foundation',
    icon: 'ledger',
    defaultPath: ROUTE_PATHS.FOUNDATION.CURRENCIES,
    matchPath: ROUTE_PATHS.FOUNDATION.ROOT,
    sections: [{ id: 'currencies', title: 'nav.setup', links: [{ label: 'nav.currencies', path: ROUTE_PATHS.FOUNDATION.CURRENCIES, permission: { module: 'GeneralLedger', resource: 'Currencies' } }] }],
  },
  'mod-SystemAdministration': {
    moduleId: 'mod-SystemAdministration',
    label: 'nav.systemAdmin',
    icon: 'admin',
    defaultPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
    matchPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.ROOT,
    sections: [{ id: 'system', title: 'nav.system', links: [{ label: 'nav.settings', path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS }] }],
  },
};
