import { useState, useCallback } from 'react';
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

    const allSelected = processedRows.length > 0 && selectedIds.length === processedRows.length;

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
