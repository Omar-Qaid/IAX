import React, { useState } from 'react';
import { TextField, InputAdornment, IconButton } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { LookupDialog } from './LookupDialog';
import type { LookupFieldProps, LookupOption } from './types';

export const LookupField: React.FC<LookupFieldProps> = ({
  label,
  value,
  onChange,
  options = [],
  disabled = false,
  readOnly = false,
  required = false,
  error = false,
  helperText,
  placeholder = 'Select...',
  fullWidth = true,
}) => {
  const [dialogOpen, setDialogOpen] = useState(false);

  const selectedOption = options.find((opt) => opt.id === value);
  const displayValue = selectedOption ? `${selectedOption.code} - ${selectedOption.name}` : '';

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange?.(null, undefined);
  };

  const handleSelectOption = (option: LookupOption) => {
    onChange?.(option.id, option);
  };

  return (
    <>
      <TextField
        label={label}
        value={displayValue}
        required={required}
        disabled={disabled}
        error={error}
        helperText={helperText}
        placeholder={placeholder}
        fullWidth={fullWidth}
        size="small"
        onClick={() => !disabled && !readOnly && setDialogOpen(true)}
        slotProps={{
          input: {
            readOnly: true,
            endAdornment: (
              <InputAdornment position="end">
                {value && !disabled && !readOnly ? (
                  <IconButton size="small" onClick={handleClear}>
                    <ClearIcon fontSize="small" />
                  </IconButton>
                ) : null}
                <IconButton size="small" disabled={disabled || readOnly} onClick={() => setDialogOpen(true)}>
                  <SearchIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ),
          },
        }}
        sx={{ cursor: disabled || readOnly ? 'default' : 'pointer' }}
      />

      <LookupDialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title={`Select ${label}`}
        options={options}
        selectedId={value}
        onSelect={handleSelectOption}
      />
    </>
  );
};
