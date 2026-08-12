import { describe, expect, it } from 'vitest';
import { getRouteBreadcrumbs } from '@app/routes/routeMetadata';
import { ROUTE_PATHS } from '@app/routes/routePaths';

describe('route metadata', () => {
  it.each([
    [ROUTE_PATHS.WORKFLOW.VARIABLES, 'nav.wfVariables'],
    [ROUTE_PATHS.WORKFLOW.STEPS, 'nav.wfSteps'],
  ])('adds Workflow Process to process-scoped %s breadcrumbs', (path, childLabel) => {
    expect(getRouteBreadcrumbs(path, '?processId=592').slice(1)).toEqual([
      { labelKey: 'nav.workflow', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      { labelKey: 'nav.workflowProcesses', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      { labelKey: childLabel },
    ]);
  });

  it('keeps direct Variables navigation at the normal module depth', () => {
    expect(getRouteBreadcrumbs(ROUTE_PATHS.WORKFLOW.VARIABLES).slice(1)).toHaveLength(2);
  });

  it('adds Workflow steps to step-scoped Activities breadcrumbs', () => {
    expect(
      getRouteBreadcrumbs(ROUTE_PATHS.WORKFLOW.ACTIVITIES, '?stepId=10').slice(1)
    ).toEqual([
      { labelKey: 'nav.workflow', path: ROUTE_PATHS.WORKFLOW.PROCESSES },
      { labelKey: 'nav.wfSteps', path: ROUTE_PATHS.WORKFLOW.STEPS },
      { labelKey: 'nav.wfActivities' },
    ]);
  });
});
