import { WORKFLOW_ROUTE_PATHS } from '@modules/workflow/routes/workflowRoutePaths';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '@modules/finance/accounts-receivable/routes/accountsReceivableRoutePaths';
import { ADMINISTRATION_ROUTE_PATHS } from '@modules/administration/routes/administrationRoutePaths';
import { INVENTORY_ROUTE_PATHS } from '@modules/finance/inventory/routes/inventoryRoutePaths';
import { FOUNDATION_ROUTE_PATHS } from '@modules/finance/foundation/routes/foundationRoutePaths';
import { ORGANIZATION_ROUTE_PATHS } from '@modules/organization/routes/organizationRoutePaths';

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

  FOUNDATION: FOUNDATION_ROUTE_PATHS,

  INVENTORY: INVENTORY_ROUTE_PATHS,

  WORKFLOW: WORKFLOW_ROUTE_PATHS,

  ORGANIZATION_ADMINISTRATION: ORGANIZATION_ROUTE_PATHS,

  SYSTEM_ADMINISTRATION: ADMINISTRATION_ROUTE_PATHS,

  ACCESS_DENIED: '/access-denied',
  NOT_FOUND: '/not-found',
} as const;
