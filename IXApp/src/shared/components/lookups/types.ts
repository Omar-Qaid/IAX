import type React from 'react';

export interface LookupOption {
  id: string | number;
  code: string;
  name: string;
  description?: string;
  [key: string]: any;
}

export interface LookupFieldProps {
  name: string;
  label: string;
  value?: string | number | (string | number)[];
  onChange?: (value: string | number | (string | number)[] | null, option?: LookupOption | LookupOption[]) => void;
  options?: LookupOption[];
  onFetchOptions?: (search: string) => Promise<LookupOption[]>;
  multiple?: boolean;
  disabled?: boolean;
  readOnly?: boolean;
  required?: boolean;
  error?: boolean;
  helperText?: string;
  placeholder?: string;
  fullWidth?: boolean;
  control?: any;
}

export interface LookupDialogProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  options: LookupOption[];
  selectedId?: string | number | (string | number)[];
  onSelect: (option: LookupOption) => void;
  loading?: boolean;
  multiple?: boolean;
}
