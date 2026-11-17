import { useMemo, useState } from 'react';
import type { ColumnDef, FilterModel, SortModel } from '../types';
import { getNestedValue, normalizeValueOptions } from '../DataGridUtils';

/** Value -> label maps for dropdown columns, so search and filters match what users see. */
function buildLabelMaps<T>(columns: ColumnDef<T>[]) {
    const maps = new Map<ColumnDef<T>['field'], Map<string, string>>();
    for (const col of columns) {
        if (col.type !== 'singleSelect' || !col.valueOptions) continue;
        maps.set(col.field, new Map(normalizeValueOptions(col.valueOptions).map(o => [o.key, o.label])));
    }
    return maps;
}

/** Negative operators must hold for every candidate (value and label), positive ones for any. */
const NEGATIVE_OPERATORS = new Set(['notEquals', 'doesNotContain']);

interface UseGridDataProcessingOptions<T> {
    rows: T[];
    columns: ColumnDef<T>[];
    globalSearch: string;
    filters: FilterModel[];
    sortModel: SortModel[];
    serverSide: boolean;
}

/**
 * Hook to handle client-side data processing (filtering, sorting, searching).
 * In server-side mode, it simply returns the original rows.
 */
export function useGridDataProcessing<T>({
    rows,
    columns,
    globalSearch,
    filters,
    sortModel,
    serverSide,
}: UseGridDataProcessingOptions<T>) {
    // Cell editors replace render callbacks on each keystroke. Only data-related
    // definitions should invalidate filtering/sorting of the complete dataset.
    const [dataColumns, setDataColumns] = useState(columns);
    if (columns.length !== dataColumns.length || columns.some((column, index) => {
        const previous = dataColumns[index];
        return column.field !== previous.field || column.type !== previous.type || column.valueGetter !== previous.valueGetter
            // Dropdown labels often load after the grid: refresh label matching when they arrive.
            || column.valueOptions !== previous.valueOptions;
    })) setDataColumns(columns);
    const processedRows = useMemo(() => {
        if (serverSide) return rows;

        const colByField = new Map<ColumnDef<T>['field'], ColumnDef<T>>();
        for (const c of dataColumns) colByField.set(c.field, c);

        let result: T[] = rows;
        const labelMaps = buildLabelMaps(dataColumns);
        const labelOf = (col: ColumnDef<T>, val: unknown) =>
            val == null ? undefined : labelMaps.get(col.field)?.get(String(val));

        // 1. Search (stored value or, for dropdowns, the label shown)
        if (globalSearch) {
            const lower = globalSearch.toLowerCase();
            result = result.filter(row =>
                dataColumns.some(col => {
                    const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);
                    if (val == null) return false;
                    if (String(val).toLowerCase().includes(lower)) return true;
                    return labelOf(col, val)?.toLowerCase().includes(lower) ?? false;
                }),
            );
        }

        // 2. Filters
        if (filters.length > 0) {
            const compiled = filters.map(filter => {
                const col = colByField.get(filter.field);
                const isString = typeof filter.value === 'string';
                let regex: RegExp | null = null;
                if (filter.operator === 'matches' && isString) {
                    try { regex = new RegExp(filter.value as string, 'i'); }
                    catch { regex = null; }
                }
                let inSet: Set<string> | null = null;
                if (filter.operator === 'in') {
                    // Accept an array, or a comma-separated string from the filter row / persisted state.
                    const values = Array.isArray(filter.value)
                        ? (filter.value as unknown[])
                        : String(filter.value ?? '').split(',').map(v => v.trim()).filter(Boolean);
                    inSet = new Set(values.map(v => String(v).toLowerCase()));
                }
                return {
                    filter,
                    col,
                    lowerValue: filter.value == null ? '' : String(filter.value).toLowerCase(),
                    regex,
                    inSet,
                    isNumber: col?.type === 'number',
                };
            });

            result = result.filter(row =>
                compiled.every(({ filter, col, lowerValue, regex, inSet, isNumber }) => {
                    if (!col) return true;
                    const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);
                    // Blank checks take no value, so they run before the empty-value skip.
                    if (filter.operator === 'isEmpty') return val == null || String(val).trim() === '';
                    if (filter.operator === 'isNotEmpty') return val != null && String(val).trim() !== '';
                    if (filter.value == null || filter.value === '') return true;
                    if (Array.isArray(filter.value) && filter.value.length === 0) return true;
                    if (val == null) return filter.operator === 'notEquals' || filter.operator === 'doesNotContain';

                    const matches = (candidate: unknown): boolean => {
                        const strVal = String(candidate).toLowerCase();
                        switch (filter.operator) {
                            case 'contains': return strVal.includes(lowerValue);
                            case 'doesNotContain': return !strVal.includes(lowerValue);
                            case 'startsWith': return strVal.startsWith(lowerValue);
                            case 'endsWith': return strVal.endsWith(lowerValue);
                            case 'equals':
                                return isNumber
                                    ? Number(candidate) === Number(filter.value)
                                    : strVal === lowerValue;
                            case 'notEquals':
                                return isNumber
                                    ? Number(candidate) !== Number(filter.value)
                                    : strVal !== lowerValue;
                            case 'gt': return isNumber ? Number(candidate) > Number(filter.value) : String(candidate) > String(filter.value);
                            case 'lt': return isNumber ? Number(candidate) < Number(filter.value) : String(candidate) < String(filter.value);
                            case 'in': return inSet !== null && inSet.has(strVal);
                            case 'matches': return regex ? regex.test(String(candidate)) : true;
                            default: return true;
                        }
                    };

                    // Dropdown columns match on the stored value or the label shown.
                    const label = labelOf(col, val);
                    const candidates: unknown[] =
                        label !== undefined && label !== String(val) ? [val, label] : [val];
                    return NEGATIVE_OPERATORS.has(filter.operator)
                        ? candidates.every(matches)
                        : candidates.some(matches);
                }),
            );
        }

        // 3. Sorting
        if (sortModel.length > 0) {
            const { field, sort } = sortModel[0];
            const col = colByField.get(field as ColumnDef<T>['field']);
            const getter = col?.valueGetter;
            
            if (result === rows) result = result.slice();
            result.sort((a, b) => {
                const valA = getter ? getter({ row: a }) : getNestedValue(a, field as string);
                const valB = getter ? getter({ row: b }) : getNestedValue(b, field as string);
                if (valA === valB) return 0;
                if (valA == null) return sort === 'asc' ? -1 : 1;
                if (valB == null) return sort === 'asc' ? 1 : -1;
                const cmp = col?.type === 'number' ? Number(valA) - Number(valB) : valA < valB ? -1 : 1;
                return sort === 'asc' ? cmp : -cmp;
            });
        }

        return result;
    }, [serverSide, rows, dataColumns, globalSearch, filters, sortModel]);

    return processedRows;
}
