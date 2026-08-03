import React, { useCallback, useEffect, useState, useMemo, useRef, useLayoutEffect } from 'react';
import { Controller, useFormContext } from 'react-hook-form';
import type { FieldValues, Path } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Box, FormHelperText } from '@mui/material';
import LockIcon from '@mui/icons-material/LockOutlined';

import { LookupGrid } from './LookupGrid';
import { filterLocalizedColumns } from '@shared/utilities/localizeColumns';
import { usePermissions } from '@core/auth/usePermissions';
import type {
  LookupGridFieldProps,
  LookupGridFieldBaseProps,
  LookupPage,
} from './types';

function LookupGridFieldInner<T extends Record<string, any>>({
  value,
  onChange,
  errorMessage,
  columns,
  queryKey,
  fetchPage,
  fetchById,
  valueField = 'id' as keyof T,
  labelField = 'name' as keyof T,
  labelFieldAr = 'nameAR' as keyof T,
  label,
  placeholder,
  disabled,
  required,
  fullWidth = true,
  size = 'small',
  pageSize,
  actions,
  permissionModule,
  permissionResource,
}: LookupGridFieldBaseProps<T>) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language === 'ar';
  const displayField = (isRtl ? labelFieldAr : labelField) as keyof T;

  const { hasPermission, isAdmin } = usePermissions();
  const hasAccess =
    permissionModule && permissionResource
      ? isAdmin || hasPermission(permissionModule, permissionResource, 'View')
      : true;

  const localizedColumns = useMemo(
    () => filterLocalizedColumns(columns as any, isRtl) as typeof columns,
    [columns, isRtl]
  );
  const [selectedRow, setSelectedRow] = useState<T | null>(null);

  const adaptedFetch = useCallback(
    async (params: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }): Promise<LookupPage<T>> => {
      const r = await fetchPage(params);
      return {
        data: r.data,
        pageNumber: r.pageNumber,
        totalPages: r.totalPages,
        totalRecords: r.totalRecords,
      };
    },
    [fetchPage]
  );

  const fetchByIdRef = useRef(fetchById);
  useLayoutEffect(() => {
    fetchByIdRef.current = fetchById;
  });

  useEffect(() => {
    let cancelled = false;
    if (value == null || value === 0 || value === '') {
      setSelectedRow(null);
      return;
    }
    setSelectedRow((prev) => (prev && prev[valueField] === value ? prev : prev));
    const fn = fetchByIdRef.current;
    if (fn) {
      fn(value)
        .then((row) => {
          if (!cancelled) setSelectedRow(row);
        })
        .catch(() => {});
    }
    return () => {
      cancelled = true;
    };
  }, [value, valueField]);

  const hasValue = value != null && value !== 0 && value !== '';
  const displayText = selectedRow
    ? String(selectedRow[displayField] ?? selectedRow[labelField] ?? '')
    : hasValue
    ? String(value)
    : '';

  return (
    <Box sx={{ width: fullWidth ? '100%' : undefined }}>
      <LookupGrid<T>
        value={hasValue ? value : null}
        displayText={displayText}
        onChange={(val, row) => {
          onChange(val, row);
          setSelectedRow(row);
        }}
        columns={localizedColumns}
        fetchPage={adaptedFetch}
        queryKey={queryKey}
        valueField={valueField}
        labelField={displayField}
        label={label}
        placeholder={placeholder}
        error={errorMessage}
        disabled={disabled || !hasAccess}
        required={required}
        fullWidth={fullWidth}
        size={size}
        pageSize={pageSize}
        actions={actions}
      />
      {!hasAccess && permissionModule && permissionResource && (
        <FormHelperText sx={{ mx: 0, display: 'flex', alignItems: 'center', gap: 0.5, color: 'warning.main' }}>
          <LockIcon sx={{ fontSize: '0.75rem' }} />
          {t('errors.no_permission') || 'No permission to view source data'}
        </FormHelperText>
      )}
    </Box>
  );
}

function LookupGridFieldWrapper<T extends Record<string, any>, TFieldValues extends FieldValues = FieldValues>(
  props: LookupGridFieldProps<T, TFieldValues>
) {
  const { name, control: controlProp, error, ...rest } = props;
  const formContext = useFormContext<TFieldValues>();
  const control = controlProp || formContext?.control;

  const errorMessage = typeof error === 'string' ? error : error?.message;

  if (!control) {
    return (
      <LookupGridFieldInner<T>
        {...rest}
        value={(props as any).value}
        onChange={(val, row) => (props as any).onChange?.(val, row)}
        errorMessage={errorMessage}
      />
    );
  }

  return (
    <Controller
      name={name as Path<TFieldValues>}
      control={control}
      render={({ field, fieldState }) => (
        <LookupGridFieldInner<T>
          {...rest}
          value={field.value}
          onChange={(val, _row) => field.onChange(val)}
          errorMessage={fieldState.error?.message || errorMessage}
        />
      )}
    />
  );
}

export const LookupGridField = React.memo(LookupGridFieldWrapper) as typeof LookupGridFieldWrapper;
export const FormGridLookupField = LookupGridField; // Backward compatibility alias
