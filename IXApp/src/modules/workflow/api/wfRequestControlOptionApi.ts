import { createWorkflowEntityApi } from './createWorkflowEntityApi';

export interface WfRequestControlOptionRecord {
  id: string;
  recId: number;
  requestControlId: number;
  value: string;
  name: string;
  nameAlias?: string | null;
  score: number;
  sortOrder: number;
  extendedProperties: string | null;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

type Dto = Omit<WfRequestControlOptionRecord, 'id'>;
const endpoint = '/v1/WfRequestControlsOption';

const toRecord = (dto: Dto): WfRequestControlOptionRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfRequestControlOptionRecord): Dto => record;

export const wfRequestControlOptionApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'RequestControlOption',
  processField: 'RequestControl.ProcessId',
  toRecord,
  toDto,
});
