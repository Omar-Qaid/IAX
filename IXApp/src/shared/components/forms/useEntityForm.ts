import { useCallback, useMemo, useState } from 'react';

export type EntityFormErrors<T> = Partial<Record<keyof T, string>>;

export interface UseEntityFormOptions<T extends object> {
  initialValues: T;
  validate?: (values: T) => EntityFormErrors<T>;
  onSubmit?: (values: T) => void | Promise<void>;
}

export function useEntityForm<T extends object>({ initialValues, validate, onSubmit }: UseEntityFormOptions<T>) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<EntityFormErrors<T>>({});
  const [submitting, setSubmitting] = useState(false);
  const dirty = useMemo(() => JSON.stringify(values) !== JSON.stringify(initialValues), [initialValues, values]);

  const setFieldValue = useCallback(<K extends keyof T>(field: K, value: T[K]) => {
    setValues(current => ({ ...current, [field]: value }));
    setErrors(current => ({ ...current, [field]: undefined }));
  }, []);

  const reset = useCallback((nextValues: T = initialValues) => {
    setValues(nextValues);
    setErrors({});
  }, [initialValues]);

  const submit = useCallback(async () => {
    const nextErrors = validate?.(values) ?? {};
    setErrors(nextErrors);
    if (Object.values(nextErrors).some(Boolean)) return false;
    setSubmitting(true);
    try {
      await onSubmit?.(values);
      return true;
    } finally {
      setSubmitting(false);
    }
  }, [onSubmit, validate, values]);

  return { values, setValues, setFieldValue, errors, setErrors, dirty, submitting, reset, submit };
}
