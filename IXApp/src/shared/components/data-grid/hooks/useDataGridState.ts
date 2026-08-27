import { useEffect, useRef, useState } from 'react';
import type { ColumnDef, SelectionMode } from '../types';
import type { GridInitialState } from './useGridPersistence';

function areColumnsStructurallyEqual<T>(cols1: ColumnDef<T>[], cols2: ColumnDef<T>[]): boolean {
  if (cols1.length !== cols2.length) return false;
  for (let i = 0; i < cols1.length; i++) {
    if (cols1[i].field !== cols2[i].field) return false;
    if (cols1[i].headerName !== cols2[i].headerName) return false;
  }
  return true;
}

interface UIStateOptions<T> {
  initialState: GridInitialState<T>;
  initialColumns: ColumnDef<T>[];
  initialSelectionMode?: SelectionMode;
  initialShowColumnBorders?: boolean;
  initialShowCellBorders?: boolean;
  rowHeight: number;
}

export function useDataGridState<T>(options: UIStateOptions<T>) {
  const {
    initialState,
    initialColumns,
    initialSelectionMode,
    initialShowColumnBorders,
    initialShowCellBorders,
    rowHeight,
  } = options;

  const [columns, setColumns] = useState<ColumnDef<T>[]>(initialState.columns || initialColumns);
  const [selectionMode, setSelectionMode] = useState<SelectionMode>(
    initialState.selectionMode ?? initialSelectionMode ?? 'single'
  );
  const [localRowHeight, setLocalRowHeight] = useState(initialState.rowHeight ?? rowHeight);
  const [showColumnBorders, setShowColumnBorders] = useState(
    initialState.showColumnBorders ?? initialShowColumnBorders ?? false
  );
  const [showCellBorders, setShowCellBorders] = useState(
    initialState.showCellBorders ?? initialShowCellBorders ?? true
  );
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [activeSidebarTab, setActiveSidebarTab] = useState<
    'columns' | 'filters' | 'features' | null
  >(null);
  const [focusedCell, setFocusedCell] = useState<{
    rowIndex: number;
    colIndex: number;
  } | null>(null);

  const prevInitialSelectionMode = useRef(initialSelectionMode);
  useEffect(() => {
    if (initialSelectionMode === prevInitialSelectionMode.current) return;
    prevInitialSelectionMode.current = initialSelectionMode;
    if (initialSelectionMode) setSelectionMode(initialSelectionMode);
  }, [initialSelectionMode]);

  const prevInitialStateColumns = useRef(initialState.columns);
  useEffect(() => {
    if (initialState.columns === prevInitialStateColumns.current) return;
    const previousColumns = prevInitialStateColumns.current;
    prevInitialStateColumns.current = initialState.columns;

    if (!areColumnsStructurallyEqual(initialState.columns, previousColumns)) {
      setColumns(initialState.columns);
      return;
    }

    setColumns((current) => {
      const currentByField = new Map(current.map((column) => [String(column.field), column]));
      return initialState.columns.map((column) => {
        const active = currentByField.get(String(column.field));
        if (!active) return column;
        return {
          ...column,
          width: active.width,
          flex: active.flex,
          hidden: active.hidden,
          pinned: active.pinned,
        };
      });
    });
  }, [initialState.columns]);

  return {
    columns,
    setColumns,
    selectionMode,
    setSelectionMode,
    localRowHeight,
    setLocalRowHeight,
    showColumnBorders,
    setShowColumnBorders,
    showCellBorders,
    setShowCellBorders,
    isSidebarOpen,
    setIsSidebarOpen,
    activeSidebarTab,
    setActiveSidebarTab,
    focusedCell,
    setFocusedCell,
  };
}
