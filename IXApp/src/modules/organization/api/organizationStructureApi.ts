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
export interface WorkerOrganizationAssignment { assignmentId: number; workerId: number; positionId: number | null; organizationUnitId: number; organizationRoleId: number; roleCode: string | null; isPrimary: boolean; validFrom: string; validTo: string | null; }
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
  async workerAssignments(workerId: number, asOf: string, signal?: AbortSignal) { return (await apiClient.get<WorkerOrganizationAssignment[]>(`${base}/workers/${workerId}/assignments`, { params: { asOf }, signal })).data; },
  async assignWorker(payload: { workerId: number; positionId: number; validFrom: string; validTo: string | null; isPrimary: boolean; }) { return (await apiClient.post<number>(`${base}/assignments`, payload)).data; },
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
  async createHierarchy(hierarchy: Omit<OrganizationHierarchy, 'id'>) {
    return (await apiClient.post<number>(`${base}/hierarchies`, hierarchy)).data;
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
