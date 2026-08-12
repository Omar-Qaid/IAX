import type React from 'react';
import type { QueryKey } from '@tanstack/react-query';
import type { Control, FieldError, FieldPath, FieldValues } from 'react-hook-form';

export interface LookupOption {
  id: string | number;
  code: string;
  name: string;
  description?: string;
  [key: string]: unknown;
}

export interface GridLookupColumn<T extends object = Record<string, unknown>> {
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

export type LookupValue<T extends object> = Extract<T[keyof T], string | number>;

export type FetchPageFn<T> = (params: {
  pageNumber: number;
  pageSize: number;
  search: string;
  signal?: AbortSignal;
}) => Promise<LookupPage<T>>;

export interface GridLookupProps<T extends object> {
  value: LookupValue<T> | null | undefined;
  displayText?: string;
  onChange: (value: LookupValue<T> | null, row: T | null) => void;

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

export interface LookupGridFieldProps<
  T extends object,
  TFieldValues extends FieldValues = FieldValues,
> {
  name: FieldPath<TFieldValues>;
  label?: string;
  control?: Control<TFieldValues>;
  value?: LookupValue<T> | null;
  onChange?: (value: LookupValue<T> | null, row?: T | null) => void;
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

  fetchById?: (id: LookupValue<T>) => Promise<T | null>;

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

export interface LookupGridFieldBaseProps<T extends object> extends Omit<
  LookupGridFieldProps<T, FieldValues>,
  'control' | 'name' | 'error'
> {
  value: LookupValue<T> | null | undefined;
  onChange: (value: LookupValue<T> | null, row?: T | null) => void;
  errorMessage?: string;
}

export type FormLookupGridFieldProps<
  T extends object,
  TFieldValues extends FieldValues = FieldValues,
> = LookupGridFieldProps<T, TFieldValues>;
export type FormLookupGridFieldBaseProps<T extends object> = LookupGridFieldBaseProps<T>;

export interface LookupFieldProps<TFieldValues extends FieldValues = FieldValues> {
  name: FieldPath<TFieldValues>;
  label: string;
  value?: string | number | (string | number)[];
  onChange?: (
    value: string | number | (string | number)[] | null,
    option?: LookupOption | LookupOption[]
  ) => void;
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
  control?: Control<TFieldValues>;
  displayMode?: 'dialog' | 'select';
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
