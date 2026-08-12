import React from 'react';
import { WorkflowSetupListPage } from '../components/WorkflowSetupListPage';
import { createEmptyWorkflowMaster } from '../api/workflowMasterApi';
import { wfPriorityApi } from '../api/workflowSetupApis';

export function WfPrioritiesPage(): React.ReactElement {
  return (
    <WorkflowSetupListPage
      titleKey="pages.wfPriorities.title"
      resourceKey="priorities"
      api={wfPriorityApi}
      createRecord={() => createEmptyWorkflowMaster({})}
      generatedCode
      requiredCoreFields={['name', 'nameAR']}
      permissions={{
        create: 'Workflow.Priorities.Create',
        edit: 'Workflow.Priorities.Edit',
        delete: 'Workflow.Priorities.Delete',
      }}
    />
  );
}
