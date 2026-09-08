import { createWorkflowEntityApi } from './createWorkflowEntityApi';
import type { WorkflowMasterDto } from './workflowMasterApi';
import type { WfProcessDto } from './wfProcessApi';

export interface WfVariableDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  dataTypeId: number;
  processId: number;
  sortOrder: number;
  dataType?: WorkflowMasterDto | null;
  process?: WfProcessDto | null;
}

export interface WfVariableRecord extends WfVariableDto {
  id: string;
}

const endpoint = '/v1/WfVariable';

const toRecord = (dto: WfVariableDto): WfVariableRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({
  id: _id,
  dataType: _dataType,
  process: _process,
  ...record
}: WfVariableRecord): WfVariableDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
  dataType: null,
  process: null,
});

export const wfVariableApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'Variable',
  processField: 'ProcessId',
  toRecord,
  toDto,
});
