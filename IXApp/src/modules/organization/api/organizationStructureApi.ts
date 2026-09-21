import { apiClient } from '@core/api/apiClient';

export interface OrganizationUnit {
  id: number;
  code: string;
  name: string;
  type: number;
}
export interface OrganizationPosition {
  id: number;
  code: string;
  name: string;
  organizationUnitId: number;
  roleId: number;
  validFrom: string;
  validTo: string | null;
}
export interface OrganizationHierarchy {
  id: number;
  code: string;
  name: string;
  purpose: string;
}
export interface OrganizationRole {
  id: number;
  code: string;
  name: string;
}
export interface OrganizationHierarchyNode { id: number; hierarchyId: number; organizationUnitId: number; parentNodeId: number | null; validFrom: string; validTo: string | null; }
export interface WorkerOrganizationAssignment { assignmentId: number; workerId: number; positionId: number | null; organizationUnitId: number; organizationRoleId: number; roleCode: string | null; isPrimary: boolean; validFrom: string; validTo: string | null; }
export interface ReportingHierarchy { id: number; code: string; name: string; purpose: string; }
export interface PositionReportingLine { id: number; reportingHierarchyId: number; subordinatePositionId: number; managerPositionId: number; validFrom: string; validTo: string | null; isPrimary: boolean; }
export interface NewOrganizationUnit {
  code: string;
  name: string;
  nameAR: string | null;
  type: number;
  validFrom: string;
  validTo: string | null;
}
const base = '/v1/organization-structure';
export const organizationStructureApi = {
  async units(asOf: string, signal?: AbortSignal) {
    return (await apiClient.get<OrganizationUnit[]>(`${base}/units`, { params: { asOf }, signal }))
      .data;
  },
  async positions(asOf: string, signal?: AbortSignal) {
    return (
      await apiClient.get<OrganizationPosition[]>(`${base}/positions`, { params: { asOf }, signal })
    ).data;
  },
  async hierarchies(signal?: AbortSignal) {
    return (await apiClient.get<OrganizationHierarchy[]>(`${base}/hierarchies`, { signal })).data;
  },
  async roles(signal?: AbortSignal) {
    return (await apiClient.get<OrganizationRole[]>(`${base}/roles`, { signal })).data;
  },
  async reportingHierarchies(signal?: AbortSignal) { return (await apiClient.get<ReportingHierarchy[]>(`${base}/reporting-hierarchies`, { signal })).data; },
  async createReportingHierarchy(payload: Omit<ReportingHierarchy, 'id'>) { return (await apiClient.post<number>(`${base}/reporting-hierarchies`, payload)).data; },
  async updateReportingHierarchy(id: number, payload: Omit<ReportingHierarchy, 'id'>) { return (await apiClient.put<number>(`${base}/reporting-hierarchies/${id}`, payload)).data; },
  async reportingLines(hierarchyId: number, asOf: string, signal?: AbortSignal) { return (await apiClient.get<PositionReportingLine[]>(`${base}/reporting-hierarchies/${hierarchyId}/lines`, { params: { asOf }, signal })).data; },
  async createReportingLine(payload: Omit<PositionReportingLine, 'id'>) { return (await apiClient.post<number>(`${base}/reporting-lines`, payload)).data; },
  async updateReportingLine(id: number, payload: Omit<PositionReportingLine, 'id' | 'reportingHierarchyId'>) { return (await apiClient.put<number>(`${base}/reporting-lines/${id}`, payload)).data; },
  async closeReportingLine(id: number, validTo: string) { return (await apiClient.put<number>(`${base}/reporting-lines/${id}/close`, { validTo })).data; },
  async createRole(role: Omit<OrganizationRole, 'id'>) { return (await apiClient.post<number>(`${base}/roles`, role)).data; },
  async updateRole(id: number, role: Omit<OrganizationRole, 'id'>) { return (await apiClient.put<number>(`${base}/roles/${id}`, role)).data; },
  async deactivateRole(id: number) { return (await apiClient.delete<number>(`${base}/roles/${id}`)).data; },
  async workerAssignments(workerId: number, asOf: string, signal?: AbortSignal) { return (await apiClient.get<WorkerOrganizationAssignment[]>(`${base}/workers/${workerId}/assignments`, { params: { asOf }, signal })).data; },
  async assignWorker(payload: { workerId: number; positionId: number; validFrom: string; validTo: string | null; isPrimary: boolean; }) { return (await apiClient.post<number>(`${base}/assignments`, payload)).data; },
  async updateWorkerAssignment(id: number, payload: { positionId: number; validFrom: string; validTo: string | null; isPrimary: boolean; }) {
    return (await apiClient.put<number>(`${base}/assignments/${id}`, payload)).data;
  },
  async closeAssignment(id: number, validTo: string) { return (await apiClient.put<number>(`${base}/assignments/${id}/close`, { validTo })).data; },
  async ancestors(hierarchyId: number, unitId: number, asOf: string, signal?: AbortSignal) {
    return (
      await apiClient.get<OrganizationUnit[]>(
        `${base}/hierarchies/${hierarchyId}/units/${unitId}/ancestors`,
        { params: { asOf }, signal }
      )
    ).data;
  },
  async create(unit: NewOrganizationUnit) {
    return (await apiClient.post<number>(`${base}/units`, unit)).data;
  },
  async update(id: number, unit: Pick<NewOrganizationUnit, 'code' | 'name' | 'type'>) {
    return (await apiClient.put<number>(`${base}/units/${id}`, unit)).data;
  },
  async nodes(hierarchyId: number, asOf: string, signal?: AbortSignal) {
    return (await apiClient.get<OrganizationHierarchyNode[]>(`${base}/hierarchies/${hierarchyId}/nodes`, { params: { asOf }, signal })).data;
  },
  async createNode(node: Omit<OrganizationHierarchyNode, 'id'>) {
    return (await apiClient.post<number>(`${base}/nodes`, node)).data;
  },
  async updateNode(id: number, node: Omit<OrganizationHierarchyNode, 'id' | 'hierarchyId'>) {
    return (await apiClient.put<number>(`${base}/nodes/${id}`, node)).data;
  },
  async closeNode(id: number, validTo: string) {
    return (await apiClient.put<number>(`${base}/nodes/${id}/close`, { validTo })).data;
  },  async createHierarchy(hierarchy: Omit<OrganizationHierarchy, 'id'>) {
    return (await apiClient.post<number>(`${base}/hierarchies`, hierarchy)).data;
  },
  async createHierarchyWithRootNode(hierarchy: Omit<OrganizationHierarchy, 'id'> & { organizationUnitId: number; validFrom: string; validTo: string | null }) {
    return (await apiClient.post<number>(`${base}/hierarchies/with-root-node`, hierarchy)).data;
  },
  async updateHierarchy(id: number, hierarchy: Omit<OrganizationHierarchy, 'id'>) {
    return (await apiClient.put<number>(`${base}/hierarchies/${id}`, hierarchy)).data;
  },
  async createPosition(position: {
    code: string;
    name: string;
    organizationUnitId: number;
    roleId: number;
    validFrom: string;
    validTo: string | null;
  }) {
    return (await apiClient.post<number>(`${base}/positions`, position)).data;
  },
  async updatePosition(
    id: number,
    position: {
      code: string;
      name: string;
      organizationUnitId: number;
      roleId: number;
      validFrom: string;
      validTo: string | null;
    }
  ) {
    return (await apiClient.put<number>(`${base}/positions/${id}`, position)).data;
  },
  async closePosition(id: number, validTo: string) {
    return (await apiClient.put<number>(`${base}/positions/${id}/close`, { validTo })).data;
  },
  async close(id: number, validTo: string) {
    return (await apiClient.put<number>(`${base}/units/${id}/close`, { validTo })).data;
  },
};
