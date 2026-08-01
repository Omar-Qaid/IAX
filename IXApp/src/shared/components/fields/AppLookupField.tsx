import React from 'react';
import { Controller, useFormContext } from 'react-hook-form';
import { LookupField } from '../lookups/LookupField';
import type { LookupFieldProps } from '../lookups/types';

export const AppLookupField: React.FC<LookupFieldProps> = ({
  name,
  label,
  control: controlProp,
  required = false,
  disabled = false,
  readOnly = false,
  options = [],
  helperText,
  fullWidth = true,
  onChange: onChangeProp,
}) => {
  const formContext = useFormContext();
  const control = controlProp || formContext?.control;

  if (!control) {
    return (
      <LookupField
        name={name}
        label={label}
        options={options}
        required={required}
        disabled={disabled}
        readOnly={readOnly}
        helperText={helperText}
        fullWidth={fullWidth}
        onChange={onChangeProp}
      />
    );
  }

  return (
    <Controller
      name={name}
      control={control as any}
      render={({ field, fieldState: { error } }) => (
        <LookupField
          name={name}
          label={label}
          value={field.value}
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
        />
      )}
    />
  );
};
