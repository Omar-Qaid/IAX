import React from 'react';
import { MenuItem, TextField, type TextFieldProps } from '@mui/material';
export interface AppEnumOption { value: string | number; label: string }
export interface AppEnumFieldProps extends Omit<TextFieldProps, 'select' | 'onChange'> { options: AppEnumOption[]; onChange?: (value: string | number) => void }
export const AppEnumField: React.FC<AppEnumFieldProps> = ({ options, onChange, ...props }) => <TextField {...props} select size="small" onChange={(event) => onChange?.(event.target.value)}>{options.map((option) => <MenuItem key={option.value} value={option.value}>{option.label}</MenuItem>)}</TextField>;

