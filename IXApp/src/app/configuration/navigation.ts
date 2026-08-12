import { ROUTE_PATHS } from '@app/routes/routePaths';
import { getPageDefinition, getRoutePermission } from '@app/routes/pageRegistry';
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
          {
            label: 'nav.allCustomers',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS,
            permission: PERMISSIONS.CUSTOMER_VIEW,
          },
          {
            label: 'nav.customerGroups',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_GROUPS,
            permission: PERMISSIONS.CUSTOMER_GROUP_VIEW,
          },
        ],
      },
      {
        id: 'orders',
        title: 'nav.orders',
        links: [
          {
            label: 'nav.salesOrders',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS,
            permission: PERMISSIONS.SALES_ORDER_VIEW,
          },
        ],
      },
      {
        id: 'setup',
        title: 'nav.setup',
        links: [
          {
            label: 'nav.customerParameters',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PARAMETERS,
            permission: PERMISSIONS.CUSTOMER_VIEW,
          },
          {
            label: 'nav.customerPaymentMethods',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_METHODS,
            permission: PERMISSIONS.CUSTOMER_VIEW,
          },
          {
            label: 'nav.customerPaymentTerms',
            path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMER_PAYMENT_TERMS,
            permission: PERMISSIONS.CUSTOMER_VIEW,
          },
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
    sections: [
      {
        id: 'setup',
        title: 'nav.setup',
        links: [
          {
            label: 'nav.currencies',
            path: ROUTE_PATHS.FOUNDATION.CURRENCIES,
            permission: PERMISSIONS.CURRENCY_VIEW,
          },
          {
            label: 'nav.exchangeRateTypes',
            path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATE_TYPES,
            permission: PERMISSIONS.CURRENCY_VIEW,
          },
          {
            label: 'nav.exchangeRates',
            path: ROUTE_PATHS.FOUNDATION.EXCHANGE_RATES,
            permission: PERMISSIONS.CURRENCY_VIEW,
          },
        ],
      },
    ],
  },
  'mod-Workflow': {
    moduleId: 'mod-Workflow',
    label: 'nav.workflow',
    icon: 'workflow',
    defaultPath: ROUTE_PATHS.WORKFLOW.PROCESSES,
    matchPath: ROUTE_PATHS.WORKFLOW.ROOT,
    sections: [
      {
        id: 'setup',
        title: 'nav.setup',
        links: [
          {
            label: 'nav.wfActivityTypes',
            path: ROUTE_PATHS.WORKFLOW.ACTIVITY_TYPES,
            permission: PERMISSIONS.WF_ACTIVITY_TYPE_VIEW,
          },
          { label: 'nav.wfDataTypes', path: ROUTE_PATHS.WORKFLOW.DATA_TYPES },
          {
            label: 'nav.wfControls',
            path: ROUTE_PATHS.WORKFLOW.CONTROLS,
            permission: PERMISSIONS.WF_CONTROL_VIEW,
          },
          {
            label: 'nav.wfPriorities',
            path: ROUTE_PATHS.WORKFLOW.PRIORITIES,
            permission: PERMISSIONS.WF_PRIORITY_VIEW,
          },
          {
            label: 'nav.wfVariables',
            path: ROUTE_PATHS.WORKFLOW.VARIABLES,
            permission: PERMISSIONS.WF_VARIABLE_VIEW,
          },
          {
            label: 'nav.wfSteps',
            path: ROUTE_PATHS.WORKFLOW.STEPS,
            permission: PERMISSIONS.WF_STEP_VIEW,
          },
          {
            label: 'nav.wfActivities',
            path: ROUTE_PATHS.WORKFLOW.ACTIVITIES,
            permission: PERMISSIONS.WF_ACTIVITY_VIEW,
          },
          {
            label: 'nav.workflowCategories',
            path: ROUTE_PATHS.WORKFLOW.CATEGORIES,
            permission: PERMISSIONS.WF_CATEGORY_VIEW,
          },
          {
            label: 'nav.workflowProcesses',
            path: ROUTE_PATHS.WORKFLOW.PROCESSES,
            permission: PERMISSIONS.WF_PROCESS_VIEW,
          },
        ],
      },
    ],
  },
  'mod-SystemAdministration': {
    moduleId: 'mod-SystemAdministration',
    label: 'nav.systemAdmin',
    icon: 'admin',
    defaultPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS,
    matchPath: ROUTE_PATHS.SYSTEM_ADMINISTRATION.ROOT,
    sections: [
      {
        id: 'system',
        title: 'nav.system',
        links: [{ label: 'nav.settings', path: ROUTE_PATHS.SYSTEM_ADMINISTRATION.SETTINGS }],
      },
    ],
  },
  'mod-OrganizationAdministration': {
    moduleId: 'mod-OrganizationAdministration',
    label: 'nav.organizationAdministration',
    icon: 'corporate',
    defaultPath: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES,
    matchPath: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.ROOT,
    sections: [
      {
        id: 'setup',
        title: 'nav.setup',
        links: [
          {
            label: 'nav.legalEntities',
            path: ROUTE_PATHS.ORGANIZATION_ADMINISTRATION.LEGAL_ENTITIES,
            permission: PERMISSIONS.LEGAL_ENTITY_VIEW,
          },
        ],
      },
    ],
  },
};

export const getModuleNavLinkPermission = (link: ModuleNavLink): PermissionCode | undefined =>
  link.permission ?? getRoutePermission(link.path);

export const isRegisteredModuleNavLink = (link: ModuleNavLink): boolean =>
  !link.path || getPageDefinition(link.path) !== undefined;

export const filterModuleNavigation = (
  configs: Record<string, ModuleNavConfig>
): Record<string, ModuleNavConfig> =>
  Object.fromEntries(
    Object.entries(configs).flatMap(([key, config]) => {
      if (!getPageDefinition(config.defaultPath)) return [];

      const sections = config.sections
        .map((section) => ({
          ...section,
          links: section.links.filter(isRegisteredModuleNavLink),
        }))
        .filter((section) => section.links.length > 0);

      return sections.length > 0 ? [[key, { ...config, sections }]] : [];
    })
  );

export const AVAILABLE_MODULE_NAV_CONFIGS = filterModuleNavigation(MODULE_NAV_CONFIGS);
