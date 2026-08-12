import React from 'react';
import { Controller, useFormContext, type FieldValues } from 'react-hook-form';
import { LookupField } from '../lookups/LookupField';
import type { LookupFieldProps } from '../lookups/types';

export function AppLookupField<TFieldValues extends FieldValues = FieldValues>({
  name,
  label,
  value,
  control: controlProp,
  required = false,
  disabled = false,
  readOnly = false,
  options = [],
  helperText,
  fullWidth = true,
  placeholder,
  displayMode = 'dialog',
  onChange: onChangeProp,
}: LookupFieldProps<TFieldValues>): React.ReactElement {
  const formContext = useFormContext<TFieldValues>();
  const control = controlProp || formContext?.control;

  if (!control) {
    return (
      <LookupField
        name={name}
        label={label}
        value={value}
        options={options}
        required={required}
        disabled={disabled}
        readOnly={readOnly}
        helperText={helperText}
        fullWidth={fullWidth}
        placeholder={placeholder}
        displayMode={displayMode}
        onChange={onChangeProp}
      />
    );
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field, fieldState: { error } }) => (
        <LookupField
          name={name}
          label={label}
          value={field.value as string | number | (string | number)[] | undefined}
          onChange={(val, option) => {
            field.onChange(val);
            onChangeProp?.(val, option);
          }}
          options={options}
          required={required}
          disabled={disabled}
          readOnly={readOnly}
          error={!!error}
          helperText={error ? error.message : helperText}
          fullWidth={fullWidth}
          placeholder={placeholder}
          displayMode={displayMode}
        />
      )}
    />
  );
}
