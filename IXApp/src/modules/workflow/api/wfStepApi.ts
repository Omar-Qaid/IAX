import { createWorkflowEntityApi } from './createWorkflowEntityApi';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfStepDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  processId: number;
  sortOrder: number;
  score: number;
  mustCompleteAll: boolean;
  isSystemDefined: boolean;
}

export interface WfStepRecord extends WfStepDto {
  id: string;
}

const endpoint = '/v1/WfStep';

const toRecord = (dto: WfStepDto): WfStepRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfStepRecord): WfStepDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
});

export const wfStepApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'Step',
  processField: 'ProcessId',
  toRecord,
  toDto,
});
