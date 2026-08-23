export const WORKFLOW_ROUTE_PATHS = {
  ROOT: '/workflow',
  MAIL: '/workflow/mail',
  REQUEST_SUBMISSION: '/workflow/request-submission',
  REQUEST_FROM: '/workflow/request-from/:categoryId/:processId',
  requestFrom: (categoryId: string | number, processId: string | number) =>
    `/workflow/request-from/${encodeURIComponent(String(categoryId))}/${encodeURIComponent(String(processId))}`,
  PROCESSES: '/workflow/processes',
  CATEGORIES: '/workflow/categories',
  ACTIVITY_TYPES: '/workflow/activity-types',
  DATA_TYPES: '/workflow/data-types',
  CONTROLS: '/workflow/controls',
  PRIORITIES: '/workflow/priorities',
  VARIABLES: '/workflow/variables',
  STEPS: '/workflow/steps',
  ACTIVITIES: '/workflow/activities',
} as const;
