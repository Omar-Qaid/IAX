import type { ColumnDef } from './types';

export const downloadFile = (content: string, fileName: string, contentType: string) => {
  const a = document.createElement('a');
  const file = new Blob([content], { type: contentType });
  const url = URL.createObjectURL(file);
  a.href = url;
  a.download = fileName;
  a.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
};

export function computeFlexWidths<T>(
  columns: ColumnDef<T>[],
  containerWidth: number
): ColumnDef<T>[] {
  let totalFlex = 0;
  let fixedWidth = 0;
  let pinnedWidth = 0;
  for (const col of columns) {
    if (col.hidden) continue;
    if (col.pinned) {
      pinnedWidth += col.width || 150;
    } else if (col.flex) {
      totalFlex += col.flex;
    } else {
      fixedWidth += col.width || 150;
    }
  }
  if (totalFlex === 0) return columns;

  const available = Math.max(0, containerWidth - fixedWidth - pinnedWidth);

  let changed = false;
  const next = columns.map((col) => {
    if (!col.flex || col.hidden || col.pinned) return col;
    const newWidth = Math.floor(Math.max(col.minWidth || 80, (col.flex / totalFlex) * available));
    if (col.width === newWidth) return col;
    changed = true;
    return { ...col, width: newWidth };
  });
  return changed ? next : columns;
}

/** Quotes a CSV field and neutralises spreadsheet formula injection (=, +, -, @, tab, CR). */
export function toCsvField(value: unknown): string {
  let str = value != null ? String(value) : '';
  if (/^[=+\-@\t\r]/.test(str) && !/^-?\d+(\.\d+)?$/.test(str)) str = `'${str}`;
  return `"${str.replace(/"/g, '""')}"`;
}

/** UTF-8 BOM so Excel opens Arabic text correctly. */
export const CSV_BOM = '﻿';

export function generateCSV<T>(rows: T[], columns: ColumnDef<T>[]): string {
  const visibleCols = columns.filter((c) => !c.hidden);
  // Dropdown columns export the label users see, not the stored code.
  const labels = visibleCols.map((col) =>
    col.type === 'singleSelect' && col.valueOptions
      ? new Map(normalizeValueOptions(col.valueOptions).map((o) => [o.key, o.label]))
      : null
  );
  const header = visibleCols.map((c) => toCsvField(c.headerName)).join(',');
  const csvRows = rows.map((row) =>
    visibleCols
      .map((col, i) => {
        const val = col.valueGetter
          ? col.valueGetter({ row })
          : getNestedValue(row, String(col.field));
        return toCsvField(val != null ? (labels[i]?.get(String(val)) ?? val) : val);
      })
      .join(',')
  );
  return CSV_BOM + [header, ...csvRows].join('\r\n');
}

export function getNestedValue(obj: unknown, path: string): unknown {
  if (!obj || !path) return undefined;
  return path.split('.').reduce<unknown>((value, key) => {
    if (typeof value !== 'object' || value === null) return undefined;
    return (value as Record<string, unknown>)[key];
  }, obj);
}

export interface NormalizedValueOption {
  /** String form MUI's select works with. */
  key: string;
  /** The option's own value, with its type (number, boolean, string, null). */
  value: unknown;
  label: string;
  disabled?: boolean;
}

/** Accepts plain values or `{ value, label }` options, keeping value types and real labels. */
export function normalizeValueOptions(valueOptions: readonly unknown[]): NormalizedValueOption[] {
  return valueOptions.map((option) => {
    const isObject = typeof option === 'object' && option !== null && 'value' in option;
    const value = isObject ? (option as { value: unknown }).value : option;
    const rawLabel = isObject ? (option as { label?: unknown }).label : option;
    const label =
      typeof rawLabel === 'string' || typeof rawLabel === 'number'
        ? String(rawLabel)
        : value == null
          ? ''
          : String(value);
    const disabled = isObject ? Boolean((option as { disabled?: unknown }).disabled) : undefined;
    return { key: value == null ? '' : String(value), value, label, disabled };
  });
}
