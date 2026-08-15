import { createWorkflowMasterApi, type WorkflowMasterDto } from './workflowMasterApi';

export interface WfControlDto extends WorkflowMasterDto {
  controlType: string;
}

export const wfActivityTypeApi = createWorkflowMasterApi<WorkflowMasterDto>(
  '/v1/WfActivityType',
  'workflow activity type'
);
export const wfDataTypeApi = createWorkflowMasterApi<WorkflowMasterDto>(
  '/v1/WfDataType',
  'workflow data type'
);
export const wfControlApi = createWorkflowMasterApi<WfControlDto>(
  '/v1/WfControl',
  'workflow control'
);
export const wfPriorityApi = createWorkflowMasterApi<WorkflowMasterDto>(
  '/v1/WfPriority',
  'workflow priority'
);
export const wfProcessTypeApi = createWorkflowMasterApi<WorkflowMasterDto>(
  '/v1/WfProcessType',
  'workflow process type'
);
export const wfOperatorApi = createWorkflowMasterApi<WorkflowMasterDto>(
  '/v1/WfOperator',
  'workflow operator'
);
