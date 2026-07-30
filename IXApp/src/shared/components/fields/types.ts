import type { Control, FieldPath, FieldValues } from 'react-hook-form';

export interface BaseFieldProps<TFieldValues extends FieldValues = FieldValues> {
  name?: FieldPath<TFieldValues> | string;
  label?: string;
  control?: Control<TFieldValues>;
  required?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  hidden?: boolean;
  helperText?: string;
  fullWidth?: boolean;
  placeholder?: string;
  value?: unknown;
  onChange?: (value: any) => void;
  variant?: 'outlined' | 'standard' | 'filled';
  sx?: any;
  inputRef?: React.Ref<any>;
}
