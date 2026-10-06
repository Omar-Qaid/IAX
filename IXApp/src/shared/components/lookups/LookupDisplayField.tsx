import React from 'react';
import { Box, Typography } from '@mui/material';
import { d365 } from '@shared/constants/enterpriseUiTokens';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { LookupNavigationLabel } from './LookupNavigationLabel';

interface LookupDisplayFieldProps {
  label: string;
  value: string | number | null | undefined;
  masterRoute?: string;
}

export function LookupDisplayField({ label, value, masterRoute }: LookupDisplayFieldProps): React.ReactElement {
  return (
    <Box sx={{ minWidth: 0 }}>
      {label && (
        <Typography sx={{ display: 'block', mb: '6px', fontSize: d365.labelFontSize, lineHeight: 1.2, color: d365.text }}>
          <LookupNavigationLabel label={label} masterRoute={masterRoute} />
        </Typography>
      )}
      <EditableViewField label={label} value={value} lookup />
    </Box>
  );
}
