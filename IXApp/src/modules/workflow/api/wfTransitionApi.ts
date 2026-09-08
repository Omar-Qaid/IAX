import { createWorkflowEntityApi } from './createWorkflowEntityApi';

export interface WfTransitionRecord {
  id: string;
  recId: number;
  processId: number;
  activityId: number | null;
  requestControlId: number | null;
  variableId: number;
  operatorId: number;
  value: string;
  stepId: number;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfTransitionRecord, 'id'>;
const endpoint = '/v1/WfTransition';
const toRecord = (dto: Dto): WfTransitionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfTransitionRecord): Dto => record;
export const wfTransitionApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'Transition',
  processField: 'ProcessId',
  toRecord,
  toDto,
});
