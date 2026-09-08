import { createWorkflowEntityApi } from './createWorkflowEntityApi';

export interface WfActivityControlValidationRecord {
  id: string;
  recId: number;
  activityControlId: number;
  name?: string | null;
  nameAlias?: string | null;
  validationType: string;
  validationExpression: string | null;
  operator: string | null;
  value: string | null;
  maskInput: string | null;
  errorMessage: string;
  severity: string;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfActivityControlValidationRecord, 'id'>;
const endpoint = '/v1/WfActivityControlsValidation';
const toRecord = (dto: Dto): WfActivityControlValidationRecord => ({
  ...dto,
  id: String(dto.recId),
});
const toDto = ({ id: _id, ...record }: WfActivityControlValidationRecord): Dto => record;

export const wfActivityControlValidationApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'ActivityControlValidation',
  processField: 'ActivityControl.Activity.Step.ProcessId',
  toRecord,
  toDto,
});
