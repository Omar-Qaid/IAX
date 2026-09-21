import { WORKFLOW_ROUTE_PATHS } from '@modules/workflow/routes/workflowRoutePaths';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '@modules/finance/accounts-receivable/routes/accountsReceivableRoutePaths';
import { ADMINISTRATION_ROUTE_PATHS } from '@modules/administration/routes/administrationRoutePaths';

export const ROUTE_PATHS = {
  ROOT: '/',
  HOME: '/',
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  DOCU_VIEW: '/documents/docu-view',
  PROCESS_BUILDER: WORKFLOW_ROUTE_PATHS.PROCESS_BUILDER,
  PROCESS_BUILDER_NEW: WORKFLOW_ROUTE_PATHS.PROCESS_BUILDER_NEW,
  processBuilder: WORKFLOW_ROUTE_PATHS.processBuilder,

  ACCOUNTS_RECEIVABLE: ACCOUNTS_RECEIVABLE_ROUTE_PATHS,

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
    ORGANIZATION_UNITS: '/organization-administration/organization-units',
    ORGANIZATIONS: '/organization-administration/organizations',
    ORGANIZATION_ROLES: '/organization-administration/organization-roles',
    ORGANIZATION_HIERARCHIES: '/organization-administration/organization-hierarchies',
    REPORTING_HIERARCHIES: '/organization-administration/reporting-hierarchies',
    HCM_POSITIONS: '/organization-administration/positions',
    HCM_WORKERS: '/organization-administration/workers',
  },

  SYSTEM_ADMINISTRATION: ADMINISTRATION_ROUTE_PATHS,

  ACCESS_DENIED: '/access-denied',
  NOT_FOUND: '/not-found',
} as const;
