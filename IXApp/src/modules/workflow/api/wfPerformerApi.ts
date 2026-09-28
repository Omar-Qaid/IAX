import { createWorkflowMasterApi, type WorkflowBaseDto } from './workflowMasterApi';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { requireApiData } from '@core/api/apiResponseData';

export interface WfPerformerDto extends WorkflowBaseDto {
  nameAlias?: string | null;
  performerTypeId: number;
  relatedField: number | null;
  isApplicant: boolean;
  isEmployee: boolean;
  isManager1: boolean;
  isManager2: boolean;
  isManager3: boolean;
  isManager4: boolean;
  sqlTable: string | null;
  sqlField: string | null;
  sqlWhere: string | null;
  userIds?: number[];
  userOptions?: WfPerformerUserOption[];
}

export interface WfPerformerUserOption {
  id: number;
  code: string;
  name: string;
  nameAlias?: string | null;
}

export interface WfPerformerUserRecord {
  id: string;
  recId: number;
  performerId: number;
  userID: number;
  relatedField: number;
  extendedProperties?: string | null;
}

interface WfPerformerWorkerLookupDto {
  recId: number;
  personnelNumber: string;
  name?: string | null;
  nameAlias?: string | null;
}

interface WfPerformerWorkerLookupPageDto {
  data: WfPerformerWorkerLookupDto[];
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

export interface WfPerformerWorkerLookupPage {
  data: Array<{ id: number; code: string; name: string; nameAlias?: string | null }>;
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

export type WfPerformerSqlSchema = Record<string, string[]>;

export const wfPerformerApi = createWorkflowMasterApi<WfPerformerDto>(
  '/v1/WfPerformer',
  'workflow performer'
);

export const wfPerformerTypeApi = createWorkflowMasterApi<WorkflowBaseDto>(
  '/v1/WfPerformerType',
  'workflow performer type'
);

export async function loadWfPerformerUsers(
  performerId: number,
  signal?: AbortSignal
): Promise<WfPerformerUserRecord[]> {
  const response = await apiClient.get<ApiResponse<Omit<WfPerformerUserRecord, 'id'>[]>>(
    `/v1/WfPerformer/${performerId}/users`,
    { signal }
  );
  return requireApiData(response.data, 'workflow performer users').map((row) => ({
    ...row,
    id: String(row.recId),
  }));
}

export async function loadWfPerformerWorkerLookup(params: {
  pageNumber: number;
  pageSize: number;
  search: string;
  selectedId?: number;
  signal?: AbortSignal;
}): Promise<WfPerformerWorkerLookupPage> {
  const response = await apiClient.get<ApiResponse<WfPerformerWorkerLookupPageDto>>(
    '/v1/HcmWorker/lookup',
    {
      params: {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        search: params.search || undefined,
        selectedId: params.selectedId || undefined,
      },
      signal: params.signal,
    }
  );
  const page = requireApiData(response.data, 'performer worker lookup');
  return {
    ...page,
    data: page.data.map((worker) => ({
      id: worker.recId,
      code: worker.personnelNumber,
      name: worker.name ?? worker.personnelNumber,
      nameAlias: worker.nameAlias,
    })),
  };
}

export async function loadWfPerformerSqlSchema(
  signal?: AbortSignal
): Promise<WfPerformerSqlSchema> {
  const response = await apiClient.get<ApiResponse<WfPerformerSqlSchema>>(
    '/v1/WfPerformer/sql-schema',
    { signal }
  );
  return requireApiData(response.data, 'performer SQL schema');
}
