import React from 'react';
import { Box } from '@mui/material';
import type { MailRequestFieldDto } from '../api/wfRequestApi';
import { normalizeDynamicControlType } from './DynamicControlRenderer';
import { SignatureControl } from './DynamicSpecialControls';

export function MailFieldValue({ field }: { field: MailRequestFieldDto }): React.ReactElement {
  if (normalizeDynamicControlType(field.controlType) === 'signature') {
    return (
      <SignatureControl
        control={{ label: field.labelAr || field.label, hideLabel: true, controlType: 'signature', readOnly: true }}
        value={field.value}
        onChange={() => undefined}
        preview
      />
    );
  }
  return <Box sx={{ whiteSpace: 'pre-wrap', overflowWrap: 'anywhere' }}>{field.value || '—'}</Box>;
}
