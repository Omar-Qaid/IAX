import { createWorkflowEntityApi } from './createWorkflowEntityApi';
import type { WorkflowMasterDto } from './workflowMasterApi';

export interface WfActivityDto extends Omit<WorkflowMasterDto, 'recId'> {
  recId: number;
  activityTypeId: number;
  stepId: number;
  performerId: number;
  score: number;
  sysNotificationTemplateId: number | null;
  isSystemNotificationEnabled: boolean;
  isEmailNotificationEnabled: boolean;
  isSmsNotificationEnabled: boolean;
  isWhatsAppNotificationEnabled: boolean;
  mandatoryDocuments: boolean;
  canViewPreviousSteps: boolean;
  canViewPreviousDocuments: boolean;
  isAutoPassEnabled: boolean;
  autoPassAfterHours: number;
  extendedProperties: string | null;
}

export interface WfActivityRecord extends WfActivityDto {
  id: string;
}

const endpoint = '/v1/WfActivity';

const toRecord = (dto: WfActivityDto): WfActivityRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfActivityRecord): WfActivityDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  nameAlias: record.nameAlias?.trim() || null,
  description: record.description?.trim() || null,
  extendedProperties: record.extendedProperties?.trim() || null,
});

export const wfActivityApi = createWorkflowEntityApi({
  endpoint,
  resourceName: 'Activity',
  processField: 'Step.ProcessId',
  toRecord,
  toDto,
});
