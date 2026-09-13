import React from 'react';
import { Box, MenuItem, TextField, Typography } from '@mui/material';
export function ListGridField({
  label,
  value,
  onChange,
  editing = false,
  numeric = false,
  options,
  underlined = false,
  disabled = false,
}: {
  label: string;
  value: string | number;
  onChange?: (value: string) => void;
  editing?: boolean;
  numeric?: boolean;
  options?: { value: string; label: string }[];
  underlined?: boolean;
  disabled?: boolean;
}): React.ReactElement {
  const id = React.useId();
  const editable = editing && !disabled;
  return (
    <Box sx={{ minWidth: 0, maxWidth: underlined ? 153 : 206 }}>
      <Typography component="label" htmlFor={id} sx={{ display: 'block', fontSize: 11, mb: '4px' }}>
        {label}
      </Typography>
      <TextField
        id={id}
        fullWidth
        size="small"
        variant={underlined && !editable ? 'standard' : 'outlined'}
        value={
          options && !editable
            ? (options.find((option) => option.value === String(value))?.label ?? value)
            : value
        }
        select={Boolean(options && editable)}
        type={numeric && editable ? 'number' : 'text'}
        onChange={(event) => onChange?.(event.target.value)}
        slotProps={{ input: { readOnly: !editable }, htmlInput: { 'aria-label': label } }}
        sx={{
          '& .MuiInputBase-root': { height: 28, fontSize: 12, borderRadius: '3px' },
          '& .MuiInputBase-input': { px: '6px', py: '4px' },
        }}
      >
        {options?.map((option) => (
          <MenuItem key={option.value} value={option.value}>
            {option.label}
          </MenuItem>
        ))}
      </TextField>
    </Box>
  );
}
