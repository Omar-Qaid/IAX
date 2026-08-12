import React, { useState } from 'react';
import { TextField, InputAdornment, IconButton, MenuItem } from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { LookupDialog } from './LookupDialog';
import type { LookupFieldProps, LookupOption } from './types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { FieldValues } from 'react-hook-form';

export function LookupField<TFieldValues extends FieldValues = FieldValues>({
  label,
  value,
  onChange,
  options = [],
  disabled = false,
  readOnly = false,
  required = false,
  error = false,
  helperText,
  placeholder,
  fullWidth = true,
  displayMode = 'dialog',
}: LookupFieldProps<TFieldValues>): React.ReactElement {
  const { t } = useAppTranslation();
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

  if (displayMode === 'select') {
    return (
      <TextField
        select
        label={label}
        value={value ?? ''}
        required={required}
        disabled={disabled || readOnly}
        error={error}
        helperText={helperText}
        fullWidth={fullWidth}
        size="small"
        onChange={(event) => {
          const option = options.find((item) => String(item.id) === event.target.value);
          onChange?.(option?.id ?? null, option);
        }}
      >
        {!required && (
          <MenuItem value="">
            <em>{placeholder ?? t('lookups.selectPlaceholder')}</em>
          </MenuItem>
        )}
        {options.map((option) => (
          <MenuItem key={option.id} value={option.id}>
            {`${option.code} - ${option.name}`}
          </MenuItem>
        ))}
      </TextField>
    );
  }

  return (
    <>
      <TextField
        label={label}
        value={displayValue}
        required={required}
        disabled={disabled}
        error={error}
        helperText={helperText}
        placeholder={placeholder ?? t('lookups.selectPlaceholder')}
        fullWidth={fullWidth}
        size="small"
        onClick={() => !disabled && !readOnly && setDialogOpen(true)}
        slotProps={{
          input: {
            readOnly: true,
            endAdornment: (
              <InputAdornment position="end">
                {value && !disabled && !readOnly ? (
                  <IconButton size="small" aria-label={t('actions.clear')} onClick={handleClear}>
                    <ClearIcon fontSize="small" />
                  </IconButton>
                ) : null}
                <IconButton
                  size="small"
                  aria-label={t('actions.search')}
                  disabled={disabled || readOnly}
                  onClick={() => setDialogOpen(true)}
                >
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
        title={t('lookups.selectField', { field: label })}
        options={options}
        selectedId={value}
        onSelect={handleSelectOption}
      />
    </>
  );
}
