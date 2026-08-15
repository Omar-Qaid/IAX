import React from 'react';
import { createEmptyWorkflowMaster } from '../api/workflowMasterApi';
import { wfCategoryApi, type WfCategoryDto } from '../api/wfCategoryApi';
import { WorkflowSetupListPage } from '../components/WorkflowSetupListPage';

export function WFCategoryPage(): React.ReactElement {
  return (
    <WorkflowSetupListPage<WfCategoryDto>
      titleKey="pages.wfCategories.title"
      resourceKey="workflow-categories"
      api={wfCategoryApi}
      createRecord={() => createEmptyWorkflowMaster<WfCategoryDto>({ sysField: false })}
      numberSequenceKey="WfCategory"
      requiredCoreFields={['name']}
      permissions={{
        create: 'Workflow.Categories.Create',
        edit: 'Workflow.Categories.Edit',
        delete: 'Workflow.Categories.Delete',
      }}
      extraFields={[
        {
          field: 'sysField',
          labelKey: 'wfCategory.fields.systemCategory',
          width: 130,
          editable: false,
          type: 'boolean',
        },
      ]}
    />
  );
}
