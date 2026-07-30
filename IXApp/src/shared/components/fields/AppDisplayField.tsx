import React from 'react';
import { Box, Typography } from '@mui/material';

export interface AppDisplayFieldProps {
  label: string;
  value?: string | number | null;
  helperText?: string;
}

export const AppDisplayField: React.FC<AppDisplayFieldProps> = ({ label, value, helperText }) => {
  return (
    <Box sx={{ mb: 1 }}>
      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', fontWeight: 600 }}>
        {label}
      </Typography>
      <Typography variant="body2" color="text.primary" sx={{ fontWeight: 600 }}>
        {value !== undefined && value !== null && value !== '' ? value : '-'}
      </Typography>
      {helperText && (
        <Typography variant="caption" color="text.secondary">
          {helperText}
        </Typography>
      )}
    </Box>
  );
};
