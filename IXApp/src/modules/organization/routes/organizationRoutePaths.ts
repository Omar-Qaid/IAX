export const ORGANIZATION_ROUTE_PATHS = {
  ROOT: '/organization-administration',
  LEGAL_ENTITIES: '/organization-administration/legal-entities',
  ORGANIZATION_UNITS: '/organization-administration/organization-units',
  ORGANIZATIONS: '/organization-administration/organizations',
  ORGANIZATION_ROLES: '/organization-administration/organization-roles',
  ORGANIZATION_HIERARCHIES: '/organization-administration/organization-hierarchies',
  ORGANIZATION_HIERARCHY_SETUP: '/organization-administration/organization-hierarchies/setup',
  ORGANIZATION_HIERARCHY_NODES: '/organization-administration/organization-hierarchies/:hierarchyId/nodes',
  organizationHierarchyNodes: (hierarchyId: string | number) =>
    `/organization-administration/organization-hierarchies/${hierarchyId}/nodes`,
  REPORTING_HIERARCHIES: '/organization-administration/reporting-hierarchies',
  HCM_POSITIONS: '/organization-administration/positions',
  HCM_WORKERS: '/organization-administration/workers',
  HCM_SHOWROOMS: '/organization-administration/showrooms',
  HCM_NATIONALITIES: '/organization-administration/nationalities',
  HCM_OCCUPATIONS: '/organization-administration/occupations',
  HCM_DEPARTMENTS: '/organization-administration/departments',
} as const;
