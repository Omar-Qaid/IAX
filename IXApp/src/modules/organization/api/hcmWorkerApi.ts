import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface HcmWorkerDto {
  recId: number;
  personnelNumber: string;
  person: number;
  name?: string | null;
  nameAlias?: string | null;
  occupationId: number;
  occupationName?: string | null;
  managerWorkerId?: number | null;
  departmentId?: number | null;
  showroomId?: number | null;
  genderId: number;
  genderName?: string | null;
  nationalityId: number;
  nationalityName?: string | null;
  hireDate: string | null;
  birthDate: string | null;
  userId?: string | null;
  isActive: boolean;
}

export interface HcmWorkerRecord extends Omit<HcmWorkerDto, 'recId'> {
  id: string;
  recordId: number;
}

export interface HcmLookupOption {
  id: number;
  code?: string | null;
  name?: string | null;
  nameAlias?: string | null;
}

interface HcmWorkerLookupDto {
  recId: number;
  personnelNumber: string;
  name?: string | null;
  nameAlias?: string | null;
}

interface HcmWorkerLookupPageDto {
  data: HcmWorkerLookupDto[];
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

export interface HcmWorkerLookupPage {
  data: Array<{ id: number; code: string; name: string; nameAlias?: string | null }>;
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

export interface HcmWorkerOrganizationAssignmentV1Record {
  recId: number;
  hcmManagerWorkerId: number;
  managerPersonnelNumber: string;
  managerName?: string | null;
  managerNameAlias?: string | null;
  departmentId?: number | null;
  departmentName?: string | null;
  departmentNameAlias?: string | null;
  occupationId?: number | null;
  occupationName?: string | null;
  occupationNameAlias?: string | null;
  validFrom: string;
  validTo?: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

export interface HcmWorkerShowroomAssignmentRecord {
  recId: number;
  hcmShowroomId: number;
  showroomName: string;
  showroomNameAlias?: string | null;
  validFrom: string;
  validTo?: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

export interface HcmWorkerAssignmentChainNode {
  type: 'manager' | 'worker' | 'showroom';
  recId: number;
  code: string;
  name?: string | null;
  nameAlias?: string | null;
  title?: string | null;
  titleAlias?: string | null;
}

export interface SaveHcmWorkerOrganizationAssignmentV1 {
  hcmManagerWorkerId: number;
  departmentId: number | null;
  occupationId: number | null;
  validFrom: string;
  validTo: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

export interface SaveHcmWorkerShowroomAssignment {
  hcmShowroomId: number;
  validFrom: string;
  validTo: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

interface HcmLookupDto {
  recId: number;
  code?: string | null;
  name?: string | null;
  nameAlias?: string | null;
}

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The worker response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = ({ recId, ...dto }: HcmWorkerDto): HcmWorkerRecord => {
  if (!Number.isSafeInteger(recId) || recId <= 0) {
    throw new ApiError('The worker response contained an invalid record identifier.', 500);
  }

  return {
    ...dto,
    id: String(recId),
    recordId: recId,
  };
};
const toDto = ({ id: _id, recordId, ...record }: HcmWorkerRecord): HcmWorkerDto => ({
  ...record,
  recId: recordId,
});

export const hcmWorkerApi = {
  async list(signal?: AbortSignal): Promise<HcmWorkerRecord[]> {
    const response = await apiClient.get<ApiResponse<HcmWorkerDto[]>>('/v1/HcmWorker', { signal });
    return requireData(response.data).map(toRecord);
  },
  async managerLookup(params: {
    pageNumber: number;
    pageSize: number;
    search: string;
    selectedId?: number | null;
    signal?: AbortSignal;
  }): Promise<HcmWorkerLookupPage> {
    const response = await apiClient.get<ApiResponse<HcmWorkerLookupPageDto>>(
      '/v1/HcmWorker/lookup',
      { params: {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        search: params.search || undefined,
        selectedId: params.selectedId || undefined,
      }, signal: params.signal }
    );
    const page = requireData(response.data);
    return {
      ...page,
      data: page.data.map((worker) => ({
        id: worker.recId,
        code: worker.personnelNumber,
        name: worker.name ?? worker.personnelNumber,
        nameAlias: worker.nameAlias,
      })),
    };
  },
  async create(record: HcmWorkerRecord): Promise<HcmWorkerRecord> {
    const response = await apiClient.post<ApiResponse<HcmWorkerDto>>(
      '/v1/HcmWorker',
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async update(record: HcmWorkerRecord): Promise<HcmWorkerRecord> {
    const response = await apiClient.put<ApiResponse<HcmWorkerDto>>(
      `/v1/HcmWorker/${record.recordId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: HcmWorkerRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(
      `/v1/HcmWorker/${record.recordId}`
    );
    requireData(response.data);
  },
  async lookup(endpoint: string, signal?: AbortSignal): Promise<HcmLookupOption[]> {
    const response = await apiClient.get<ApiResponse<HcmLookupDto[]>>(`/v1/${endpoint}`, {
      signal,
    });
    return requireData(response.data).map(({ recId, code, name, nameAlias }) => ({
      id: recId,
      code,
      name,
      nameAlias,
    }));
  },
  async organizationAssignmentsV1(
    workerId: number,
    signal?: AbortSignal
  ): Promise<HcmWorkerOrganizationAssignmentV1Record[]> {
    const response = await apiClient.get<
      ApiResponse<HcmWorkerOrganizationAssignmentV1Record[]>
    >(`/v1/HcmWorker/${workerId}/organization-assignments-v1`, { signal });
    return requireData(response.data);
  },
  async assignmentChain(
    workerId: number,
    signal?: AbortSignal
  ): Promise<HcmWorkerAssignmentChainNode[]> {
    const response = await apiClient.get<ApiResponse<HcmWorkerAssignmentChainNode[]>>(
      `/v1/HcmWorker/${workerId}/assignment-chain`,
      { signal }
    );
    return requireData(response.data);
  },
  async showroomAssignments(
    workerId: number,
    signal?: AbortSignal
  ): Promise<HcmWorkerShowroomAssignmentRecord[]> {
    const response = await apiClient.get<ApiResponse<HcmWorkerShowroomAssignmentRecord[]>>(
      `/v1/HcmWorker/${workerId}/showroom-assignments`,
      { signal }
    );
    return requireData(response.data);
  },
  async saveOrganizationAssignmentV1(
    workerId: number,
    assignmentId: number | null,
    request: SaveHcmWorkerOrganizationAssignmentV1
  ): Promise<void> {
    const path = `/v1/HcmWorker/${workerId}/organization-assignments-v1`;
    const response = assignmentId
      ? await apiClient.put<ApiResponse<boolean>>(`${path}/${assignmentId}`, request)
      : await apiClient.post<ApiResponse<boolean>>(path, request);
    requireData(response.data);
  },
  async saveShowroomAssignment(
    workerId: number,
    assignmentId: number | null,
    request: SaveHcmWorkerShowroomAssignment
  ): Promise<void> {
    const path = `/v1/HcmWorker/${workerId}/showroom-assignments`;
    const response = assignmentId
      ? await apiClient.put<ApiResponse<boolean>>(`${path}/${assignmentId}`, request)
      : await apiClient.post<ApiResponse<boolean>>(path, request);
    requireData(response.data);
  },
};
