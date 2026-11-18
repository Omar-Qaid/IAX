import React from 'react';
import { HcmReferencePage } from '../components/HcmReferencePage';
import { hcmOccupationApi } from '../api/hcmReferenceApi';

export function HcmOccupationPage(): React.ReactElement {
  return (
    <HcmReferencePage
      titleKey="hcmOccupations.title"
      codeLabelKey="hcmOccupations.fields.code"
      recordTableName="HcmOccupation"
      sourceKey="hcm-occupations"
      storageKey="organization.hcm-occupations.reference-view"
      api={hcmOccupationApi}
    />
  );
}
