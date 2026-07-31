import { useCallback, useMemo } from 'react';
import type { ColumnDef, FilterModel, SortModel, SelectionMode } from '../types';

const PERSISTENCE_VERSION = 2;

interface PersistedColState {
    field: string;
    hidden?: boolean;
    width?: number;
    flex?: number;
    pinned?: 'left' | 'right' | null;
}

export interface PersistedGridState {
    version?: number;
    columns?: PersistedColState[];
    sortModel?: SortModel[];
    filters?: FilterModel[];
    rowHeight?: number;
    showColumnBorders?: boolean;
    showCellBorders?: boolean;
    selectionMode?: SelectionMode;
}

function readState(key: string): PersistedGridState {
    try {
        const raw = localStorage.getItem(key);
        if (!raw) return {};
        const parsed = JSON.parse(raw) as PersistedGridState;
        if (parsed.version !== PERSISTENCE_VERSION) {
            localStorage.removeItem(key);
            return {};
        }
        return parsed;
    } catch {
        return {};
    }
}

function mergeColumns<T>(
    initialColumns: ColumnDef<T>[],
    persisted: PersistedColState[] | undefined,
): ColumnDef<T>[] {
    if (!persisted?.length) return initialColumns;

    const byField = new Map(initialColumns.map(c => [String(c.field), c]));
    const result: ColumnDef<T>[] = [];
    const seen = new Set<string>();

    for (const saved of persisted) {
        let col = byField.get(saved.field);
        
        if (!col) {
            let altField = '';
            if (saved.field.endsWith('AR')) {
                altField = saved.field.slice(0, -2);
            } else {
                altField = saved.field + 'AR';
            }
            col = byField.get(altField);
        }

        if (!col) continue;

        const matchedField = String(col.field);
        if (seen.has(matchedField)) continue;

        const hasWidth = saved.width !== undefined;
        result.push({
            ...col,
            hidden:  saved.hidden,
            width:   hasWidth ? saved.width : col.width,
            flex:    hasWidth ? saved.flex : (saved.flex ?? col.flex),
            pinned:  saved.pinned !== undefined ? saved.pinned : col.pinned,
        });
        seen.add(matchedField);
    }

    for (const col of initialColumns) {
        if (!seen.has(String(col.field))) result.push(col);
    }

    return result;
}

export interface GridInitialState<T> {
    columns: ColumnDef<T>[];
    sortModel: SortModel[];
    filters: FilterModel[];
    rowHeight?: number;
    showColumnBorders?: boolean;
    showCellBorders?: boolean;
    selectionMode?: SelectionMode;
}

interface UseGridPersistenceReturn<T> {
    initialState: GridInitialState<T>;
    persist: (state: PersistedGridState) => void;
    clear: () => void;
}

export function useGridPersistence<T>(
    storageKey: string | undefined,
    initialColumns: ColumnDef<T>[],
): UseGridPersistenceReturn<T> {
    const initialState = useMemo<GridInitialState<T>>(() => {
        if (!storageKey) {
            return { columns: initialColumns, sortModel: [], filters: [] };
        }
        const saved = readState(storageKey);
        return {
            columns:          mergeColumns(initialColumns, saved.columns),
            sortModel:        saved.sortModel        ?? [],
            filters:          saved.filters          ?? [],
            rowHeight:        saved.rowHeight,
            showColumnBorders: saved.showColumnBorders,
            showCellBorders:  saved.showCellBorders,
            selectionMode:    saved.selectionMode,
        };
    }, [storageKey, initialColumns]);

    const persist = useCallback(
        (state: PersistedGridState) => {
            if (!storageKey) return;
            try {
                localStorage.setItem(storageKey, JSON.stringify({ ...state, version: PERSISTENCE_VERSION }));
            } catch { /* storage quota / private browsing */ }
        },
        [storageKey],
    );

    const clear = useCallback(() => {
        if (!storageKey) return;
        try { localStorage.removeItem(storageKey); } catch { /* ignore */ }
    }, [storageKey]);

    return { initialState, persist, clear };
}
