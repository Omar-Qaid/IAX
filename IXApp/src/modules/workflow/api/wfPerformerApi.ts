import { createWorkflowMasterApi, type WorkflowMasterDto } from './workflowMasterApi';

export interface WfPerformerDto extends WorkflowMasterDto {
  performerTypeId: number;
  relatedField: number | null;
  isApplicant: boolean;
  isEmployee: boolean;
  isManager1: boolean;
  isManager2: boolean;
  isManager3: boolean;
  isManager4: boolean;
  sqlTable: string | null;
  sqlField: string | null;
  sqlWhere: string | null;
  userIds: number[];
}

export const wfPerformerApi = createWorkflowMasterApi<WfPerformerDto>(
  '/v1/WfPerformer',
  'workflow performer'
);
