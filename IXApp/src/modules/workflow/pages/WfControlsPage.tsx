import React from 'react';
import { WorkflowSetupListPage } from '../components/WorkflowSetupListPage';
import { createEmptyWorkflowMaster } from '../api/workflowMasterApi';
import { wfControlApi, type WfControlDto } from '../api/workflowSetupApis';

export function WfControlsPage(): React.ReactElement {
  return (
    <WorkflowSetupListPage<WfControlDto>
      titleKey="pages.wfControls.title"
      resourceKey="controls"
      api={wfControlApi}
      createRecord={() => createEmptyWorkflowMaster<WfControlDto>({ controlType: '' })}
      numberSequenceKey="WfControl"
      requiredCoreFields={['name']}
      permissions={{
        create: 'Workflow.Controls.Create',
        edit: 'Workflow.Controls.Edit',
        delete: 'Workflow.Controls.Delete',
      }}
      extraFields={[
        {
          field: 'controlType',
          labelKey: 'workflowSetup.fields.controlType',
          width: 180,
          required: true,
        },
      ]}
    />
  );
}
