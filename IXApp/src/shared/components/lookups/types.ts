import type React from 'react';
import type { QueryKey } from '@tanstack/react-query';
import type { Control, FieldError, FieldValues } from 'react-hook-form';

export interface LookupOption {
  id: string | number;
  code: string;
  name: string;
  description?: string;
  [key: string]: any;
}

export interface GridLookupColumn<T extends Record<string, any> = Record<string, any>> {
  field: keyof T | string;
  header: string;
  headerAr?: string;
  headerEn?: string;
  width?: number | string;
  flex?: number;
  align?: 'left' | 'center' | 'right';
  render?: (row: T) => React.ReactNode;
  hidden?: boolean;
  showInRtl?: boolean;
  showInLtr?: boolean;
}

export interface GridLookupAction {
  label: string;
  icon?: React.ReactNode;
  onClick: () => void;
  disabled?: boolean;
}

export interface LookupPage<T> {
  data: T[];
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

export type FetchPageFn<T> = (params: {
  pageNumber: number;
  pageSize: number;
  search: string;
  signal?: AbortSignal;
}) => Promise<LookupPage<T>>;

export interface GridLookupProps<T extends Record<string, any>> {
  value: T[keyof T] | null | undefined;
  displayText?: string;
  onChange: (value: T[keyof T] | null, row: T | null) => void;

  columns: GridLookupColumn<T>[];
  fetchPage: FetchPageFn<T>;
  queryKey: QueryKey;

  valueField?: keyof T;
  labelField?: keyof T;

  label?: string;
  placeholder?: string;
  error?: string;
  disabled?: boolean;
  required?: boolean;
  fullWidth?: boolean;
  size?: 'small' | 'medium';

  pageSize?: number;
  rowHeight?: number;
  popupWidth?: number | string;
  popupMaxHeight?: number;
  searchDebounceMs?: number;
  showClearButton?: boolean;
  actions?: GridLookupAction[];
}

export interface LookupGridFieldProps<T extends Record<string, any>, TFieldValues extends FieldValues = FieldValues> {
  name: string;
  label?: string;
  control?: Control<TFieldValues>;
  error?: FieldError | { message?: string } | string;

  columns: GridLookupColumn<T>[];
  queryKey: QueryKey;

  fetchPage: (params: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) => Promise<{
    data: T[];
    pageNumber: number;
    totalPages: number;
    totalRecords: number;
  }>;

  fetchById?: (id: string | number) => Promise<T | null>;

  valueField?: keyof T;
  labelField?: keyof T;
  labelFieldAr?: keyof T;

  placeholder?: string;
  disabled?: boolean;
  required?: boolean;
  fullWidth?: boolean;
  size?: 'small' | 'medium';
  pageSize?: number;
  actions?: GridLookupAction[];
  permissionModule?: string;
  permissionResource?: string;
}

export interface LookupGridFieldBaseProps<T extends Record<string, any>>
  extends Omit<LookupGridFieldProps<T, FieldValues>, 'control' | 'name' | 'error'> {
  value: any;
  onChange: (v: any, row?: T | null) => void;
  errorMessage?: string;
}

export type FormLookupGridFieldProps<T extends Record<string, any>, TFieldValues extends FieldValues = FieldValues> = LookupGridFieldProps<T, TFieldValues>;
export type FormLookupGridFieldBaseProps<T extends Record<string, any>> = LookupGridFieldBaseProps<T>;

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
