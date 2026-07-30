import { useMemo } from 'react';
import type { ColumnDef, FilterModel, SortModel } from '../types';
import { getNestedValue } from '../DataGridUtils';

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
    const processedRows = useMemo(() => {
        if (serverSide) return rows;

        const colByField = new Map<ColumnDef<T>['field'], ColumnDef<T>>();
        for (const c of columns) colByField.set(c.field, c);

        let result: T[] = rows;

        // 1. Search
        if (globalSearch) {
            const lower = globalSearch.toLowerCase();
            result = result.filter(row =>
                columns.some(col => {
                    const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);
                    return val != null && String(val).toLowerCase().includes(lower);
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
                if (filter.operator === 'in' && Array.isArray(filter.value)) {
                    inSet = new Set((filter.value as unknown[]).map(v => String(v).toLowerCase()));
                }
                return {
                    filter,
                    col,
                    lowerValue: isString ? (filter.value as string).toLowerCase() : '',
                    regex,
                    inSet,
                    isNumber: col?.type === 'number',
                };
            });

            result = result.filter(row =>
                compiled.every(({ filter, col, lowerValue, regex, inSet, isNumber }) => {
                    if (!filter.value) return true;
                    if (!col) return true;
                    const val = col.valueGetter ? col.valueGetter({ row }) : getNestedValue(row, col.field as string);
                    if (val == null) return false;

                    const strVal = String(val).toLowerCase();

                    switch (filter.operator) {
                        case 'contains': return strVal.includes(lowerValue);
                        case 'doesNotContain': return !strVal.includes(lowerValue);
                        case 'startsWith': return strVal.startsWith(lowerValue);
                        case 'endsWith': return strVal.endsWith(lowerValue);
                        case 'equals':
                            return isNumber
                                ? Number(val) === Number(filter.value)
                                : strVal === lowerValue;
                        case 'notEquals':
                            return isNumber
                                ? Number(val) !== Number(filter.value)
                                : strVal !== lowerValue;
                        case 'gt': return val > filter.value;
                        case 'lt': return val < filter.value;
                        case 'in': return inSet !== null && inSet.has(strVal);
                        case 'matches': return regex ? regex.test(String(val)) : true;
                        default: return true;
                    }
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
                const cmp = valA < valB ? -1 : 1;
                return sort === 'asc' ? cmp : -cmp;
            });
        }

        return result;
    }, [serverSide, rows, columns, globalSearch, filters, sortModel]);

    return processedRows;
}
