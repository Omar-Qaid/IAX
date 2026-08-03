import { useCallback, useEffect, useState } from 'react';

type StoredValue<T> = T | ((current: T) => T);

export interface LocalStorageOptions<T> {
  serialize?: (value: T) => string;
  deserialize?: (value: string) => T;
}

export const useLocalStorage = <T>(
  key: string,
  initialValue: T | (() => T),
  options: LocalStorageOptions<T> = {},
) => {
  const resolveInitialValue = useCallback(
    () => (initialValue instanceof Function ? initialValue() : initialValue),
    [initialValue],
  );
  const serialize = options.serialize ?? JSON.stringify;
  const deserialize = options.deserialize ?? JSON.parse;

  const readValue = useCallback((): T => {
    if (typeof window === 'undefined') return resolveInitialValue();
    try {
      const stored = window.localStorage.getItem(key);
      return stored == null ? resolveInitialValue() : deserialize(stored);
    } catch {
      return resolveInitialValue();
    }
  }, [deserialize, key, resolveInitialValue]);

  const [value, setValue] = useState<T>(readValue);

  const updateValue = useCallback((next: StoredValue<T>) => {
    setValue((current) => {
      const resolved = next instanceof Function ? next(current) : next;
      try { window.localStorage.setItem(key, serialize(resolved)); } catch { /* restricted storage */ }
      return resolved;
    });
  }, [key, serialize]);

  const removeValue = useCallback(() => {
    try { window.localStorage.removeItem(key); } catch { /* restricted storage */ }
    setValue(resolveInitialValue());
  }, [key, resolveInitialValue]);

  useEffect(() => {
    const handleStorage = (event: StorageEvent) => {
      if (event.key === key) setValue(readValue());
    };
    window.addEventListener('storage', handleStorage);
    return () => window.removeEventListener('storage', handleStorage);
  }, [key, readValue]);

  return [value, updateValue, removeValue] as const;
};

