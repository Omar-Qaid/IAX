import React from 'react';
import { WorkflowSetupListPage } from '../components/WorkflowSetupListPage';
import { createEmptyWorkflowMaster } from '../api/workflowMasterApi';
import { wfActivityTypeApi } from '../api/workflowSetupApis';

export function WfActivityTypesPage(): React.ReactElement {
  return (
    <WorkflowSetupListPage
      titleKey="pages.wfActivityTypes.title"
      resourceKey="activity-types"
      api={wfActivityTypeApi}
      createRecord={() => createEmptyWorkflowMaster({})}
      generatedCode
      requiredCoreFields={['name']}
      permissions={{
        create: 'Workflow.ActivityTypes.Create',
        edit: 'Workflow.ActivityTypes.Edit',
        delete: 'Workflow.ActivityTypes.Delete',
      }}
    />
  );
}
