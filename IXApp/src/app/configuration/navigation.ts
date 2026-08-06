import { ROUTE_PATHS } from '@app/routes/routePaths';
import { PERMISSIONS, type PermissionCode } from '@core/permissions/permissions';

export interface ModuleNavLink {
  label: string;
  path?: string;
  icon?: string;
  expandable?: boolean;
  permission?: PermissionCode;
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
          { label: 'nav.allCustomers', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS, permission: PERMISSIONS.CUSTOMER_VIEW },
          { label: 'nav.customerGroups', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS, permission: PERMISSIONS.CUSTOMER_GROUP_VIEW },
        ],
      },
      {
        id: 'orders',
        title: 'nav.orders',
        links: [
          { label: 'nav.salesOrders', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS, permission: PERMISSIONS.SALES_ORDER_VIEW },
        ],
      },
      {
        id: 'setup',
        title: 'nav.setup',
        links: [
          { label: 'nav.customerParameters', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS, permission: PERMISSIONS.CUSTOMER_VIEW },
          { label: 'nav.customerPaymentMethods', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS, permission: PERMISSIONS.CUSTOMER_VIEW },
          { label: 'nav.customerPaymentTerms', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_TERMS, permission: PERMISSIONS.CUSTOMER_VIEW },
        ],
      },
    ],
  },
  'mod-GeneralLedger': {
    moduleId: 'mod-GeneralLedger',
    label: 'nav.generalLedger',
    icon: 'ledger',
    defaultPath: ROUTE_PATHS.FOUNDATION.CURRENCIES,
    matchPath: ROUTE_PATHS.FOUNDATION.ROOT,
    sections: [{ id: 'setup', title: 'nav.setup', links: [{ label: 'nav.currencies', path: ROUTE_PATHS.FOUNDATION.CURRENCIES, permission: PERMISSIONS.CURRENCY_VIEW }, { label: 'nav.exchangeRateTypes', path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATE_TYPES, permission: PERMISSIONS.CURRENCY_VIEW }, { label: 'nav.exchangeRates', path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATES, permission: PERMISSIONS.CURRENCY_VIEW }] }],
  },
  'mod-SystemAdministration': {
    moduleId: 'mod-SystemAdministration',
    label: 'nav.systemAdmin',
    icon: 'admin',
    defaultPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
    matchPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.ROOT,
    sections: [{ id: 'system', title: 'nav.system', links: [{ label: 'nav.settings', path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS }] }],
  },
  'mod-OrganizationAdministration': {
    moduleId: 'mod-OrganizationAdministration',
    label: 'nav.organizationAdministration',
    icon: 'corporate',
    defaultPath: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES,
    matchPath: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.ROOT,
    sections: [{ id: 'setup', title: 'nav.setup', links: [{ label: 'nav.legalEntities', path: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES, permission: PERMISSIONS.LEGAL_ENTITY_VIEW }] }],
  },
};
