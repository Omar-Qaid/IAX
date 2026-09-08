import { createWorkflowEntityApi } from './createWorkflowEntityApi';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfActivityControlDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  activityId: number;
  processId: number;
  controlId: number;
  mandatory: boolean;
  uniqueKey: boolean;
  score: number;
  usedAsCriteria: boolean;
  usedInSearch: boolean;
  sortOrder: number;
  validationRules: string | null;
  extendedProperties: string | null;
}

export interface WfActivityControlRecord extends WfActivityControlDto {
  id: string;
}
const endpoint = '/v1/WfActivityControl';
const toRecord = (dto: WfActivityControlDto): WfActivityControlRecord => ({
  ...dto,
  id: String(dto.recId),
});
const toDto = ({ id: _id, ...record }: WfActivityControlRecord): WfActivityControlDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
});

export const wfActivityControlApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'ActivityControl',
  processField: 'Activity.Step.ProcessId',
  toRecord,
  toDto,
});
