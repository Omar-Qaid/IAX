import { createWorkflowEntityApi } from './createWorkflowEntityApi';

export interface WfActivityControlOptionRecord {
  id: string;
  recId: number;
  activityControlId: number;
  value: string;
  name: string;
  nameAlias?: string | null;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfActivityControlOptionRecord, 'id'>;
const endpoint = '/v1/WfActivityControlsOption';
const toRecord = (dto: Dto): WfActivityControlOptionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityControlOptionRecord): Dto => record;

export const wfActivityControlOptionApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'ActivityControlOption',
  processField: 'ActivityControl.Activity.Step.ProcessId',
  toRecord,
  toDto,
});
