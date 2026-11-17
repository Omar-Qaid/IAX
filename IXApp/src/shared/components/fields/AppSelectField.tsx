import React from 'react';
import { MenuItem, TextField } from '@mui/material';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import { Controller, type FieldValues } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import type { BaseFieldProps } from './types';
import type { SelectOption } from '@core/types/common';
import { lookupValueSx } from '../lookups/lookupValueStyle';
import { LookupNavigationLabel } from '../lookups/LookupNavigationLabel';

const selectValue = (selected: unknown, options: SelectOption[], placeholder: string) =>
  selected === '' ? (
    <span className="app-select-placeholder">{placeholder}</span>
  ) : (
    (options.find((option) => String(option.value) === String(selected))?.label ?? String(selected))
  );

export interface AppSelectFieldProps<
  TFieldValues extends FieldValues = FieldValues,
> extends BaseFieldProps<TFieldValues, string | number> {
  options: SelectOption[];
  masterRoute?: string;
  /** Render inside a data-grid cell: borderless, thin chevron (D365). Unbound mode only. */
  gridCell?: boolean;
}

const GridChevron = (props: React.ComponentProps<typeof KeyboardArrowDownIcon>) => (
  <KeyboardArrowDownIcon {...props} sx={{ fontSize: 18 }} />
);

export function AppSelectField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  control,
  options,
  masterRoute,
  required = false,
  disabled = false,
  readOnly = false,
  hidden = false,
  helperText,
  fullWidth = true,
  variant = 'outlined',
  value,
  onChange,
  gridCell = false,
}: AppSelectFieldProps<TFieldValues>): React.ReactElement | null {
  const { t } = useTranslation();
  const placeholder = t('lookups.selectPlaceholder', 'Select…');
  if (hidden) return null;

  if (!control || !name) {
    return (
      <TextField
        select
        name={name}
        label={
          label ? <LookupNavigationLabel label={label} masterRoute={masterRoute} /> : undefined
        }
        required={required}
        disabled={disabled || readOnly}
        helperText={helperText}
        fullWidth={fullWidth}
        size="small"
        variant={gridCell ? 'standard' : variant}
        value={value ?? ''}
        sx={[
          // Enum values in a grid read as plain text; only lookups are links (D365).
          gridCell ? {} : lookupValueSx(value),
          { '& .app-select-placeholder': { color: 'text.disabled' } },
        ]}
        slotProps={{
          ...(gridCell ? { input: { disableUnderline: true } } : {}),
          select: {
            displayEmpty: true,
            renderValue: (selected) => selectValue(selected, options, placeholder),
            ...(gridCell ? { IconComponent: GridChevron } : {}),
          },
        }}
        onChange={(e) => onChange?.(e.target.value)}
      >
        {options.map((opt) => (
          <MenuItem key={opt.value} value={opt.value} disabled={opt.disabled}>
            {opt.label}
          </MenuItem>
        ))}
      </TextField>
    );
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <TextField
          {...field}
          select
          label={
            label ? <LookupNavigationLabel label={label} masterRoute={masterRoute} /> : undefined
          }
          required={required}
          disabled={disabled || readOnly}
          error={!!error}
          helperText={error ? error.message : helperText}
          fullWidth={fullWidth}
          size="small"
          value={field.value ?? ''}
          sx={[
            lookupValueSx(field.value),
            { '& .app-select-placeholder': { color: 'text.disabled' } },
          ]}
          slotProps={{
            select: {
              displayEmpty: true,
              renderValue: (selected) => selectValue(selected, options, placeholder),
            },
          }}
        >
          {options.map((opt) => (
            <MenuItem key={opt.value} value={opt.value} disabled={opt.disabled}>
              {opt.label}
            </MenuItem>
          ))}
        </TextField>
      )}
    />
  );
}
