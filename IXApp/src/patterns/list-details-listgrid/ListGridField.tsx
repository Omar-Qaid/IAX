import React from 'react';
import { Box, MenuItem, TextField, Typography } from '@mui/material';
import { EditableViewField } from '@shared/components/fields/EditableViewField';
import { listGridControlSx, listGridLabelSx } from './fieldStyles';
export function ListGridField({
  label,
  value,
  onChange,
  editing = false,
  numeric = false,
  options,
  underlined = false,
  disabled = false,
  onEdit,
}: {
  label: string;
  value: string | number;
  onChange?: (value: string) => void;
  editing?: boolean;
  numeric?: boolean;
  options?: { value: string; label: string }[];
  underlined?: boolean;
  disabled?: boolean;
  onEdit?: () => void;
}): React.ReactElement {
  const id = React.useId();
  const editable = editing && !disabled;
  const displayValue = options
    ? (options.find((option) => option.value === String(value))?.label ?? value)
    : value;
  return (
    <Box sx={{ minWidth: 0, maxWidth: underlined ? 153 : 206 }}>
      <Typography component="label" htmlFor={id} sx={listGridLabelSx}>
        {label}
      </Typography>
      {!editable ? <EditableViewField
        value={displayValue}
        label={label}
        numeric={numeric}
        lookup={Boolean(options)}
        disabled={disabled}
        onEdit={onEdit}
      /> : <TextField
        id={id}
        fullWidth
        size="small"
        variant={underlined && !editable ? 'standard' : 'outlined'}
        value={value}
        select={Boolean(options)}
        type={numeric ? 'number' : 'text'}
        onChange={(event) => onChange?.(event.target.value)}
        slotProps={{ htmlInput: { 'aria-label': label } }}
        sx={listGridControlSx}
      >
        {options?.map((option) => (
          <MenuItem key={option.value} value={option.value}>
            {option.label}
          </MenuItem>
        ))}
      </TextField>}
    </Box>
  );
}
