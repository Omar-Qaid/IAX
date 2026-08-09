import type { Control, FieldPath, FieldValues } from 'react-hook-form';
import type { SxProps, Theme } from '@mui/material/styles';

export interface BaseFieldProps<TFieldValues extends FieldValues = FieldValues, TValue = unknown> {
  name?: FieldPath<TFieldValues>;
  label?: string;
  control?: Control<TFieldValues>;
  required?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  hidden?: boolean;
  helperText?: string;
  fullWidth?: boolean;
  placeholder?: string;
  value?: TValue;
  onChange?: (value: TValue) => void;
  variant?: 'outlined' | 'standard' | 'filled';
  sx?: SxProps<Theme>;
  inputRef?: React.Ref<HTMLInputElement>;
}
