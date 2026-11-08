import { useState, useCallback, useMemo } from 'react';
import type { SelectionMode } from '../types';

interface UseGridSelectionOptions<T> {
    processedRows: T[];
    getRowId: (row: T) => string | number;
    selectionMode: SelectionMode;
    onSelectionChange?: (ids: (string | number)[]) => void;
    initialSelectedIds?: (string | number)[];
}

export function useGridSelection<T>({
    processedRows,
    getRowId,
    selectionMode,
    onSelectionChange,
    initialSelectedIds = [],
}: UseGridSelectionOptions<T>) {
    const [selectedIds, setSelectedIds] = useState<(string | number)[]>(initialSelectedIds);

    const handleSelectionChange = useCallback((newSelection: (string | number)[]) => {
        setSelectedIds(newSelection);
        onSelectionChange?.(newSelection);
    }, [onSelectionChange]);

    const handleSelectAll = useCallback((checked: boolean) => {
        const ids = checked ? processedRows.map(row => getRowId(row)) : [];
        setSelectedIds(ids);
        onSelectionChange?.(ids);
    }, [processedRows, getRowId, onSelectionChange]);

    // Compared as sets, not by length: selection survives filtering, so a stale id outside
    // processedRows could make the counts agree while no visible row is actually selected.
    const allSelected = useMemo(() => {
        if (processedRows.length === 0) return false;
        const selected = new Set(selectedIds);
        return processedRows.every(row => selected.has(getRowId(row)));
    }, [processedRows, selectedIds, getRowId]);

    const [prevSelectionMode, setPrevSelectionMode] = useState(selectionMode);
    if (selectionMode !== prevSelectionMode) {
        setPrevSelectionMode(selectionMode);
        if (selectionMode === 'single' && selectedIds.length > 1) {
            const trimmed = [selectedIds[0]];
            setSelectedIds(trimmed);
            onSelectionChange?.(trimmed);
        }
    }

    return {
        selectedIds,
        setSelectedIds,
        handleSelectionChange,
        handleSelectAll,
        allSelected,
    };
}
