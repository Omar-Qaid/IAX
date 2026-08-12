import React from 'react';
import { WorkflowSetupListPage } from '../components/WorkflowSetupListPage';
import { createEmptyWorkflowMaster } from '../api/workflowMasterApi';
import { wfDataTypeApi } from '../api/workflowSetupApis';

export function WfDataTypesPage(): React.ReactElement {
  return (
    <WorkflowSetupListPage
      titleKey="pages.wfDataTypes.title"
      resourceKey="data-types"
      api={wfDataTypeApi}
      createRecord={() => createEmptyWorkflowMaster({})}
      generatedCode={false}
    />
  );
}
