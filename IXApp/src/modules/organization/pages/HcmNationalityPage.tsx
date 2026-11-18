import React from 'react';
import { HcmReferencePage } from '../components/HcmReferencePage';
import { hcmNationalityApi } from '../api/hcmReferenceApi';

export function HcmNationalityPage(): React.ReactElement {
  return (
    <HcmReferencePage
      titleKey="hcmNationalities.title"
      codeLabelKey="hcmNationalities.fields.code"
      recordTableName="HcmNationality"
      sourceKey="hcm-nationalities"
      storageKey="organization.hcm-nationalities.reference-view"
      api={hcmNationalityApi}
    />
  );
}
