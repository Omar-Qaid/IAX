import { matchPath } from 'react-router-dom';
import { ROUTE_PATHS } from './routePaths';
import { AVAILABLE_MODULE_NAV_CONFIGS } from '@app/configuration/navigation';

export interface BreadcrumbDefinition {
  labelKey: string;
  path?: string;
}

interface RouteMetadata {
  path: string;
  breadcrumbs: BreadcrumbDefinition[];
}

const home: BreadcrumbDefinition = { labelKey: 'nav.home', path: ROUTE_PATHS.DASHBOARD };

const navigationMetadata: RouteMetadata[] = Object.values(AVAILABLE_MODULE_NAV_CONFIGS).flatMap(
  (module) =>
    module.sections.flatMap((section) =>
      section.links.flatMap((link) =>
        link.path
          ? [
              {
                path: link.path,
                breadcrumbs: [
                  home,
                  { labelKey: module.label, path: module.defaultPath },
                  { labelKey: link.label },
                ],
              },
            ]
          : []
      )
    )
);

export const ROUTE_METADATA: RouteMetadata[] = [
  {
    path: ROUTE_PATHS.PROCESS_BUILDER,
    breadcrumbs: [
      home,
      { labelKey: 'pages.wfProcessBuilder.title' },
    ],
  },
  {
    path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDER_DETAILS,
    breadcrumbs: [
      home,
      { labelKey: 'nav.accountsReceivable', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.CUSTOMERS },
      { labelKey: 'nav.salesOrders', path: ROUTE_PATHS.ACCOUNTS_RECEIVABLE.SALES_ORDERS },
      { labelKey: 'pages.salesOrder.breadcrumb' },
    ],
  },
  ...navigationMetadata,
];

export const getRouteBreadcrumbs = (
  pathname: string,
  search = ''
): BreadcrumbDefinition[] => {
  const processScopedChild =
    new URLSearchParams(search).has('processId') &&
    (pathname === ROUTE_PATHS.WORKFLOW.VARIABLES || pathname === ROUTE_PATHS.WORKFLOW.STEPS);

  if (processScopedChild) {
    return [
      home,
      { labelKey: 'nav.workflow', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      { labelKey: 'nav.workflowProcesses', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      {
        labelKey:
          pathname === ROUTE_PATHS.WORKFLOW.VARIABLES ? 'nav.wfVariables' : 'nav.wfSteps',
      },
    ];
  }

  if (
    pathname === ROUTE_PATHS.WORKFLOW.ACTIVITIES &&
    new URLSearchParams(search).has('stepId')
  ) {
    return [
      home,
      { labelKey: 'nav.workflow', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      { labelKey: 'nav.wfSteps', path: ROUTE_PATHS.WORKFLOW.STEPS },
      { labelKey: 'nav.wfActivities' },
    ];
  }

  return (
    ROUTE_METADATA.find((route) => matchPath({ path: route.path, end: true }, pathname))
      ?.breadcrumbs ?? [home]
  );
};
