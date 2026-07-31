import { useState, useCallback } from 'react';
import type { ColumnDef } from '../types';

interface UseGridAutosizeOptions<T> {
    setColumns: React.Dispatch<React.SetStateAction<ColumnDef<T>[]>>;
    processedRows: T[];
    initialColumns: ColumnDef<T>[];
}

export function useGridAutosize<T>({
    setColumns,
    processedRows,
    initialColumns,
}: UseGridAutosizeOptions<T>) {
    const [isAutosized, setIsAutosized] = useState(false);

    const calculateOptimalWidth = useCallback((col: ColumnDef<T>, data: T[]) => {
        let maxLen = String(col.headerName).length;
        data.forEach(row => {
            const val = col.valueGetter ? col.valueGetter({ row }) : row[col.field as keyof T];
            if (val != null) maxLen = Math.max(maxLen, String(val).length);
        });
        return Math.min(500, Math.max(col.minWidth || 80, maxLen * 8 + 32));
    }, []);

    const handleAutosizeAll = useCallback(() => {
        const sample = processedRows.slice(0, 50);
        setColumns(prev => prev.map(col =>
            col.hidden ? col : { ...col, width: calculateOptimalWidth(col, sample), flex: undefined },
        ));
        setIsAutosized(true);
    }, [processedRows, calculateOptimalWidth, setColumns]);

    const handleAutosizeColumn = useCallback((field: string) => {
        const sample = processedRows.slice(0, 50);
        setColumns(prev => prev.map(col =>
            col.field !== field ? col : { ...col, width: calculateOptimalWidth(col, sample), flex: undefined },
        ));
    }, [processedRows, calculateOptimalWidth, setColumns]);

    const handleUnAutosizeColumn = useCallback((field: string) => {
        setColumns(prev => prev.map(col => {
            if (col.field !== field) return col;
            const initial = initialColumns.find(i => i.field === field);
            return { ...col, width: initial?.width, flex: initial?.flex };
        }));
    }, [initialColumns, setColumns]);

    return {
        isAutosized,
        setIsAutosized,
        handleAutosizeAll,
        handleAutosizeColumn,
        handleUnAutosizeColumn,
    };
}
