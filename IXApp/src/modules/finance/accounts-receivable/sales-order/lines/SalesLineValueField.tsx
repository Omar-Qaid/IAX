import React, { useState } from 'react';
import { TextField, type TextFieldProps } from '@mui/material';

/** Preserve partially typed decimals and an empty input until the user commits. */
export function SalesLineValueField(props: TextFieldProps) {
  const { value, type, onChange, onFocus, slotProps } = props;
  const [text, setText] = useState(String(value ?? ''));
  const [previousValue, setPreviousValue] = useState(value);
  if (!Object.is(previousValue, value)) {
    setPreviousValue(value);
    if (!Object.is(Number(text), value)) setText(String(value ?? ''));
  }
  if (type !== 'number') return <TextField {...props} />;
  return (
    <TextField
      {...props}
      type="text"
      value={text}
      onChange={(event) => {
        setText(event.target.value);
        onChange?.(event);
      }}
      onFocus={(event) => {
        event.target.select();
        onFocus?.(event);
      }}
      slotProps={{
        ...slotProps,
        htmlInput: {
          ...(typeof slotProps?.htmlInput === 'object' ? slotProps.htmlInput : {}),
          inputMode: 'decimal',
          role: 'spinbutton',
          'aria-valuenow': typeof value === 'number' && Number.isFinite(value) ? value : undefined,
        },
      }}
    />
  );
}
