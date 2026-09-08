import { createWorkflowEntityApi } from './createWorkflowEntityApi';

export interface WfRequestControlValidationRecord {
  id: string;
  recId: number;
  requestControlId: number;
  validationType: string;
  validationExpression: string | null;
  operator: string | null;
  value: string | null;
  maskInput: string | null;
  errorMessage: string;
  errorMessageAlias?: string | null;
  severity: string;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}
type Dto = Omit<WfRequestControlValidationRecord, 'id'>;
const endpoint = '/v1/WfRequestControlsValidation';
const toRecord = (dto: Dto): WfRequestControlValidationRecord => ({
  ...dto,
  id: String(dto.recId),
});
const toDto = ({ id: _id, ...record }: WfRequestControlValidationRecord): Dto => record;

export const wfRequestControlValidationApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'RequestControlValidation',
  processField: 'RequestControl.ProcessId',
  toRecord,
  toDto,
});
