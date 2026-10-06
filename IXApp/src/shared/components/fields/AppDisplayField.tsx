import React from 'react';
import { Box, Typography } from '@mui/material';
import { EditableViewField } from './EditableViewField';
import { d365 } from '@shared/constants/enterpriseUiTokens';

export interface AppDisplayFieldProps {
  label: string;
  value?: string | number | null;
  helperText?: string;
  lookup?: boolean;
}

export const AppDisplayField: React.FC<AppDisplayFieldProps> = ({ label, value, helperText, lookup = false }) => {
  return (
    <Box sx={{ minWidth: 0 }}>
      {label && <Typography sx={{ display: 'block', mb: '6px', fontSize: d365.labelFontSize, lineHeight: 1.2, color: d365.text }}>
        {label}
      </Typography>}
      <EditableViewField label={label} value={value} lookup={lookup} />
      {helperText && (
        <Typography variant="caption" color="text.secondary">
          {helperText}
        </Typography>
      )}
    </Box>
  );
};
