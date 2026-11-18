import React from 'react';
import { HcmReferencePage } from '../components/HcmReferencePage';
import { hcmDepartmentApi } from '../api/hcmReferenceApi';

export function HcmDepartmentPage(): React.ReactElement {
  return (
    <HcmReferencePage
      titleKey="hcmDepartments.title"
      codeLabelKey="hcmDepartments.fields.code"
      recordTableName="HcmDepartment"
      sourceKey="hcm-departments"
      storageKey="organization.hcm-departments.reference-view"
      api={hcmDepartmentApi}
    />
  );
}
