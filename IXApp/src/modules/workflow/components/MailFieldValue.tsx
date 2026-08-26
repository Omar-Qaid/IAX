import React from 'react';
import { Box } from '@mui/material';
import type { MailRequestFieldDto } from '../api/wfRequestApi';
import { normalizeDynamicControlType } from './DynamicControlRenderer';
import { SignatureControl } from './DynamicSpecialControls';
import { useAppTranslation } from '@core/localization/useAppTranslation';

export function MailFieldValue({ field }: { field: MailRequestFieldDto }): React.ReactElement {
  const { isRtl } = useAppTranslation();
  const label = isRtl ? field.labelAr || field.label : field.label || field.labelAr;
  const value = isRtl ? field.valueAr || field.value || field.valueEn : field.valueEn || field.value || field.valueAr;
  if (normalizeDynamicControlType(field.controlType) === 'signature') {
    return (
      <SignatureControl
        control={{ label, hideLabel: true, controlType: 'signature', readOnly: true }}
        value={value}
        onChange={() => undefined}
        preview
      />
    );
  }
  return <Box dir="auto" sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>{value || '—'}</Box>;
}
