import { createWorkflowEntityApi } from './createWorkflowEntityApi';

interface MappingRecordBase {
  id: string;
  recId: number;
  variableId: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface WfRequestMappingVariableRecord extends MappingRecordBase {
  requestControlId: number;
  sortOrder: number;
}

export interface WfActivityMappingVariableRecord extends MappingRecordBase {
  activityControlId: number;
  variableOrder: number;
}

export const wfRequestMappingVariableApi = createWorkflowEntityApi({
  endpoint: '/v1/WfRequestMappingVariable',
  resourceName: 'RequestVariableMapping',
  processField: 'VariableId',
  toRecord: (dto: Omit<WfRequestMappingVariableRecord, 'id'>) => ({ ...dto, id: String(dto.recId) }),
  toDto: ({ id: _id, ...record }: WfRequestMappingVariableRecord) => record,
});

export const wfActivityMappingVariableApi = createWorkflowEntityApi({
  endpoint: '/v1/WfActivityMappingVariable',
  resourceName: 'ActivityVariableMapping',
  processField: 'VariableId',
  toRecord: (dto: Omit<WfActivityMappingVariableRecord, 'id'>) => ({ ...dto, id: String(dto.recId) }),
  toDto: ({ id: _id, ...record }: WfActivityMappingVariableRecord) => record,
});
